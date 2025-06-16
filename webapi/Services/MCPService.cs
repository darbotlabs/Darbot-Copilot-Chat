// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CopilotChat.WebApi.Models;

namespace CopilotChat.WebApi.Services;

/// <summary>
/// Service for managing MCP (Model Context Protocol) operations.
/// </summary>
public class MCPService
{
    private readonly ILogger<MCPService> _logger;
    private readonly ConcurrentDictionary<string, MCPConnection> _connections = new();
    private readonly ConcurrentBag<MCPMessage> _messages = new();
    private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
    
    private MCPServerConfig _serverConfig = new();
    private TcpListener? _server;
    private CancellationTokenSource? _serverCancellationTokenSource;
    private bool _isServerRunning = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MCPService"/> class.
    /// </summary>
    public MCPService(ILogger<MCPService> logger)
    {
        _logger = logger;
        
        // Initialize with some default tools
        InitializeDefaultTools();
    }

    #region Server Management

    /// <summary>
    /// Get the current server configuration.
    /// </summary>
    public Task<MCPServerConfig> GetServerConfigAsync()
    {
        return Task.FromResult(_serverConfig);
    }

    /// <summary>
    /// Update the server configuration.
    /// </summary>
    public async Task<MCPServerConfig> UpdateServerConfigAsync(MCPServerConfig config)
    {
        if (_isServerRunning)
        {
            throw new InvalidOperationException("Cannot update configuration while server is running");
        }

        _serverConfig = config;
        _logger.LogInformation("MCP server configuration updated");
        
        return await Task.FromResult(_serverConfig);
    }

    /// <summary>
    /// Start the MCP server.
    /// </summary>
    public async Task StartServerAsync()
    {
        if (_isServerRunning)
        {
            throw new InvalidOperationException("MCP server is already running");
        }

        try
        {
            _server = new TcpListener(IPAddress.Any, _serverConfig.Port);
            _serverCancellationTokenSource = new CancellationTokenSource();
            
            _server.Start();
            _isServerRunning = true;
            _serverConfig.IsEnabled = true;

            _logger.LogInformation("MCP server started on port {Port}", _serverConfig.Port);

            // Add server start message
            await AddMessageAsync(new MCPMessage
            {
                Id = Guid.NewGuid().ToString(),
                Type = MCPMessageType.Notification,
                Method = "server_start",
                Content = new { port = _serverConfig.Port, uri = $"mcp://localhost:{_serverConfig.Port}" },
                Direction = MCPMessageDirection.Outbound
            });

            // Start accepting connections in background
            _ = Task.Run(() => AcceptClientsAsync(_serverCancellationTokenSource.Token));
        }
        catch (Exception ex)
        {
            _isServerRunning = false;
            _serverConfig.IsEnabled = false;
            _logger.LogError(ex, "Failed to start MCP server");
            throw;
        }
    }

    /// <summary>
    /// Stop the MCP server.
    /// </summary>
    public async Task StopServerAsync()
    {
        if (!_isServerRunning)
        {
            return;
        }

        try
        {
            _serverCancellationTokenSource?.Cancel();
            _server?.Stop();
            
            // Close all client connections
            foreach (var client in _clients.Values)
            {
                client?.Close();
            }
            _clients.Clear();

            _isServerRunning = false;
            _serverConfig.IsEnabled = false;

            _logger.LogInformation("MCP server stopped");

            // Add server stop message
            await AddMessageAsync(new MCPMessage
            {
                Id = Guid.NewGuid().ToString(),
                Type = MCPMessageType.Notification,
                Method = "server_stop",
                Content = new { port = _serverConfig.Port },
                Direction = MCPMessageDirection.Outbound
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MCP server");
            throw;
        }
    }

    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _server != null)
        {
            try
            {
                var tcpClient = await _server.AcceptTcpClientAsync();
                var clientId = Guid.NewGuid().ToString();
                _clients[clientId] = tcpClient;

                _logger.LogInformation("MCP client connected: {ClientId}", clientId);

                // Handle client in background
                _ = Task.Run(() => HandleClientAsync(clientId, tcpClient, cancellationToken));
            }
            catch (ObjectDisposedException)
            {
                // Server was stopped
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting MCP client connection");
            }
        }
    }

    private async Task HandleClientAsync(string clientId, TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[4096];

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await ProcessClientMessageAsync(clientId, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MCP client {ClientId}", clientId);
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
            client?.Close();
            _logger.LogInformation("MCP client disconnected: {ClientId}", clientId);
        }
    }

    private async Task ProcessClientMessageAsync(string clientId, string message)
    {
        try
        {
            var mcpMessage = JsonSerializer.Deserialize<MCPMessage>(message);
            if (mcpMessage != null)
            {
                mcpMessage.ConnectionId = clientId;
                mcpMessage.Direction = MCPMessageDirection.Inbound;
                mcpMessage.Timestamp = DateTime.UtcNow;
                
                await AddMessageAsync(mcpMessage);
                
                // Process the message and send response if needed
                await ProcessMCPMessageAsync(mcpMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message from client {ClientId}: {Message}", clientId, message);
        }
    }

    private async Task ProcessMCPMessageAsync(MCPMessage message)
    {
        // Handle different MCP message types and methods
        switch (message.Method.ToLowerInvariant())
        {
            case "initialize":
                await HandleInitializeAsync(message);
                break;
            case "list_tools":
                await HandleListToolsAsync(message);
                break;
            case "call_tool":
                await HandleCallToolAsync(message);
                break;
            default:
                _logger.LogWarning("Unknown MCP method: {Method}", message.Method);
                break;
        }
    }

    private async Task HandleInitializeAsync(MCPMessage message)
    {
        var response = new MCPMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = MCPMessageType.Response,
            Method = "initialize",
            Content = new 
            { 
                capabilities = _serverConfig.Capabilities,
                serverInfo = new { name = "Darbot Copilot Chat", version = "1.0.0" }
            },
            Direction = MCPMessageDirection.Outbound,
            ConnectionId = message.ConnectionId
        };

        await SendResponseAsync(response);
    }

    private async Task HandleListToolsAsync(MCPMessage message)
    {
        var tools = await GetToolsAsync();
        var response = new MCPMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = MCPMessageType.Response,
            Method = "list_tools",
            Content = new { tools },
            Direction = MCPMessageDirection.Outbound,
            ConnectionId = message.ConnectionId
        };

        await SendResponseAsync(response);
    }

    private async Task HandleCallToolAsync(MCPMessage message)
    {
        // TODO: Implement tool calling logic
        var response = new MCPMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = MCPMessageType.Response,
            Method = "call_tool",
            Content = new { result = "Tool call not implemented yet" },
            Direction = MCPMessageDirection.Outbound,
            ConnectionId = message.ConnectionId
        };

        await SendResponseAsync(response);
    }

    private async Task SendResponseAsync(MCPMessage response)
    {
        try
        {
            if (response.ConnectionId != null && _clients.TryGetValue(response.ConnectionId, out var client))
            {
                var json = JsonSerializer.Serialize(response);
                var data = Encoding.UTF8.GetBytes(json);
                
                await client.GetStream().WriteAsync(data, 0, data.Length);
                await AddMessageAsync(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send response to client {ClientId}", response.ConnectionId);
        }
    }

    #endregion

    #region Connection Management

    /// <summary>
    /// Get all MCP connections.
    /// </summary>
    public Task<IEnumerable<MCPConnection>> GetConnectionsAsync()
    {
        return Task.FromResult(_connections.Values.AsEnumerable());
    }

    /// <summary>
    /// Get a specific MCP connection.
    /// </summary>
    public Task<MCPConnection?> GetConnectionAsync(string id)
    {
        _connections.TryGetValue(id, out var connection);
        return Task.FromResult(connection);
    }

    /// <summary>
    /// Create a new MCP connection.
    /// </summary>
    public Task<MCPConnection> CreateConnectionAsync(MCPConnection connection)
    {
        connection.Id = Guid.NewGuid().ToString();
        connection.Status = MCPConnectionStatus.Disconnected;
        connection.LastConnected = null;

        _connections[connection.Id] = connection;
        
        _logger.LogInformation("Created MCP connection: {ConnectionId} ({Name})", connection.Id, connection.Name);
        
        return Task.FromResult(connection);
    }

    /// <summary>
    /// Update an MCP connection.
    /// </summary>
    public Task<MCPConnection?> UpdateConnectionAsync(MCPConnection connection)
    {
        if (!_connections.ContainsKey(connection.Id))
        {
            return Task.FromResult<MCPConnection?>(null);
        }

        _connections[connection.Id] = connection;
        _logger.LogInformation("Updated MCP connection: {ConnectionId}", connection.Id);
        
        return Task.FromResult<MCPConnection?>(connection);
    }

    /// <summary>
    /// Delete an MCP connection.
    /// </summary>
    public async Task<bool> DeleteConnectionAsync(string id)
    {
        if (_connections.TryRemove(id, out var connection))
        {
            // Disconnect if connected
            if (connection.Status == MCPConnectionStatus.Connected)
            {
                await DisconnectAsync(id);
            }
            
            _logger.LogInformation("Deleted MCP connection: {ConnectionId}", id);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Connect to an MCP endpoint.
    /// </summary>
    public async Task ConnectAsync(string id)
    {
        if (!_connections.TryGetValue(id, out var connection))
        {
            throw new ArgumentException($"Connection with ID '{id}' not found");
        }

        if (connection.Status == MCPConnectionStatus.Connected)
        {
            throw new InvalidOperationException("Connection is already established");
        }

        try
        {
            connection.Status = MCPConnectionStatus.Connecting;
            _connections[id] = connection;

            // Simulate connection process
            await Task.Delay(2000);

            connection.Status = MCPConnectionStatus.Connected;
            connection.LastConnected = DateTime.UtcNow;
            _connections[id] = connection;

            await AddMessageAsync(new MCPMessage
            {
                Id = Guid.NewGuid().ToString(),
                Type = MCPMessageType.Notification,
                Method = "initialize",
                Content = new { status = "connected", uri = connection.Uri },
                Direction = MCPMessageDirection.Inbound,
                ConnectionId = id
            });

            _logger.LogInformation("Connected to MCP endpoint: {ConnectionId}", id);
        }
        catch (Exception ex)
        {
            connection.Status = MCPConnectionStatus.Error;
            _connections[id] = connection;
            _logger.LogError(ex, "Failed to connect to MCP endpoint: {ConnectionId}", id);
            throw;
        }
    }

    /// <summary>
    /// Disconnect from an MCP endpoint.
    /// </summary>
    public async Task DisconnectAsync(string id)
    {
        if (!_connections.TryGetValue(id, out var connection))
        {
            throw new ArgumentException($"Connection with ID '{id}' not found");
        }

        connection.Status = MCPConnectionStatus.Disconnected;
        _connections[id] = connection;

        await AddMessageAsync(new MCPMessage
        {
            Id = Guid.NewGuid().ToString(),
            Type = MCPMessageType.Notification,
            Method = "disconnect",
            Content = new { status = "disconnected" },
            Direction = MCPMessageDirection.Outbound,
            ConnectionId = id
        });

        _logger.LogInformation("Disconnected from MCP endpoint: {ConnectionId}", id);
    }

    #endregion

    #region Message Management

    /// <summary>
    /// Get MCP message history.
    /// </summary>
    public Task<IEnumerable<MCPMessage>> GetMessagesAsync(string? connectionId = null, int limit = 100)
    {
        var messages = _messages.AsEnumerable();
        
        if (!string.IsNullOrEmpty(connectionId))
        {
            messages = messages.Where(m => m.ConnectionId == connectionId);
        }

        messages = messages
            .OrderByDescending(m => m.Timestamp)
            .Take(limit);

        return Task.FromResult(messages);
    }

    /// <summary>
    /// Send an MCP message.
    /// </summary>
    public async Task<MCPMessage> SendMessageAsync(MCPMessage message)
    {
        message.Id = Guid.NewGuid().ToString();
        message.Timestamp = DateTime.UtcNow;
        message.Direction = MCPMessageDirection.Outbound;

        await AddMessageAsync(message);
        
        _logger.LogInformation("Sent MCP message: {MessageId} ({Method})", message.Id, message.Method);
        
        return message;
    }

    private Task AddMessageAsync(MCPMessage message)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    #endregion

    #region Tool Management

    private readonly ConcurrentBag<MCPTool> _tools = new();

    /// <summary>
    /// Get available MCP tools.
    /// </summary>
    public Task<IEnumerable<MCPTool>> GetToolsAsync()
    {
        return Task.FromResult(_tools.AsEnumerable());
    }

    private void InitializeDefaultTools()
    {
        _tools.Add(new MCPTool
        {
            Name = "chat",
            Description = "Send a message to the chat conversation",
            Schema = new
            {
                type = "object",
                properties = new
                {
                    message = new { type = "string", description = "The message to send" },
                    chatId = new { type = "string", description = "The chat conversation ID" }
                },
                required = new[] { "message", "chatId" }
            }
        });

        _tools.Add(new MCPTool
        {
            Name = "search_memory",
            Description = "Search through chat memory and documents",
            Schema = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "The search query" },
                    chatId = new { type = "string", description = "The chat conversation ID" }
                },
                required = new[] { "query", "chatId" }
            }
        });

        _tools.Add(new MCPTool
        {
            Name = "get_conversation_history",
            Description = "Get the conversation history for a chat",
            Schema = new
            {
                type = "object",
                properties = new
                {
                    chatId = new { type = "string", description = "The chat conversation ID" },
                    limit = new { type = "number", description = "Maximum number of messages to return", defaultValue = 50 }
                },
                required = new[] { "chatId" }
            }
        });
    }

    #endregion
}