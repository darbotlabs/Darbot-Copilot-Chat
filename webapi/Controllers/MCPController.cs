// Copyright (c) Microsoft. All rights reserved.

using System.Net;
using CopilotChat.WebApi.Models;
using CopilotChat.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CopilotChat.WebApi.Controllers;

/// <summary>
/// Controller for MCP (Model Context Protocol) operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MCPController : ControllerBase
{
    private readonly MCPService _mcpService;
    private readonly ILogger<MCPController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MCPController"/> class.
    /// </summary>
    public MCPController(MCPService mcpService, ILogger<MCPController> logger)
    {
        _mcpService = mcpService;
        _logger = logger;
    }

    /// <summary>
    /// Get MCP server configuration.
    /// </summary>
    [HttpGet("server/config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MCPServerConfig>> GetServerConfigAsync()
    {
        try
        {
            var config = await _mcpService.GetServerConfigAsync();
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MCP server configuration");
            return StatusCode(500, "Failed to get server configuration");
        }
    }

    /// <summary>
    /// Update MCP server configuration.
    /// </summary>
    [HttpPut("server/config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MCPServerConfig>> UpdateServerConfigAsync([FromBody] MCPServerConfig config)
    {
        try
        {
            if (config.Port <= 0 || config.Port > 65535)
            {
                return BadRequest("Port must be between 1 and 65535");
            }

            var updatedConfig = await _mcpService.UpdateServerConfigAsync(config);
            return Ok(updatedConfig);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid server configuration");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update MCP server configuration");
            return StatusCode(500, "Failed to update server configuration");
        }
    }

    /// <summary>
    /// Start the MCP server.
    /// </summary>
    [HttpPost("server/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> StartServerAsync()
    {
        try
        {
            await _mcpService.StartServerAsync();
            return Ok(new { message = "MCP server started successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to start MCP server");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MCP server");
            return StatusCode(500, "Failed to start server");
        }
    }

    /// <summary>
    /// Stop the MCP server.
    /// </summary>
    [HttpPost("server/stop")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> StopServerAsync()
    {
        try
        {
            await _mcpService.StopServerAsync();
            return Ok(new { message = "MCP server stopped successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MCP server");
            return StatusCode(500, "Failed to stop server");
        }
    }

    /// <summary>
    /// Get all MCP connections.
    /// </summary>
    [HttpGet("connections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MCPConnection>>> GetConnectionsAsync()
    {
        try
        {
            var connections = await _mcpService.GetConnectionsAsync();
            return Ok(connections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MCP connections");
            return StatusCode(500, "Failed to get connections");
        }
    }

    /// <summary>
    /// Create a new MCP connection.
    /// </summary>
    [HttpPost("connections")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MCPConnection>> CreateConnectionAsync([FromBody] MCPConnection connection)
    {
        try
        {
            if (string.IsNullOrEmpty(connection.Name) || string.IsNullOrEmpty(connection.Uri))
            {
                return BadRequest("Name and URI are required");
            }

            if (!Uri.TryCreate(connection.Uri, UriKind.Absolute, out var uri) || uri.Scheme != "mcp")
            {
                return BadRequest("Invalid MCP URI format. Must start with 'mcp://'");
            }

            var createdConnection = await _mcpService.CreateConnectionAsync(connection);
            return CreatedAtAction(nameof(GetConnectionAsync), new { id = createdConnection.Id }, createdConnection);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid connection data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create MCP connection");
            return StatusCode(500, "Failed to create connection");
        }
    }

    /// <summary>
    /// Get a specific MCP connection.
    /// </summary>
    [HttpGet("connections/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MCPConnection>> GetConnectionAsync(string id)
    {
        try
        {
            var connection = await _mcpService.GetConnectionAsync(id);
            if (connection == null)
            {
                return NotFound($"Connection with ID '{id}' not found");
            }

            return Ok(connection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MCP connection {ConnectionId}", id);
            return StatusCode(500, "Failed to get connection");
        }
    }

    /// <summary>
    /// Update an MCP connection.
    /// </summary>
    [HttpPut("connections/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MCPConnection>> UpdateConnectionAsync(string id, [FromBody] MCPConnection connection)
    {
        try
        {
            if (id != connection.Id)
            {
                return BadRequest("Connection ID mismatch");
            }

            var updatedConnection = await _mcpService.UpdateConnectionAsync(connection);
            if (updatedConnection == null)
            {
                return NotFound($"Connection with ID '{id}' not found");
            }

            return Ok(updatedConnection);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid connection data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update MCP connection {ConnectionId}", id);
            return StatusCode(500, "Failed to update connection");
        }
    }

    /// <summary>
    /// Delete an MCP connection.
    /// </summary>
    [HttpDelete("connections/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteConnectionAsync(string id)
    {
        try
        {
            var deleted = await _mcpService.DeleteConnectionAsync(id);
            if (!deleted)
            {
                return NotFound($"Connection with ID '{id}' not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete MCP connection {ConnectionId}", id);
            return StatusCode(500, "Failed to delete connection");
        }
    }

    /// <summary>
    /// Connect to an MCP endpoint.
    /// </summary>
    [HttpPost("connections/{id}/connect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> ConnectAsync(string id)
    {
        try
        {
            await _mcpService.ConnectAsync(id);
            return Ok(new { message = "Connection established successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Connection not found {ConnectionId}", id);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to connect {ConnectionId}", id);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to MCP endpoint {ConnectionId}", id);
            return StatusCode(500, "Failed to connect");
        }
    }

    /// <summary>
    /// Disconnect from an MCP endpoint.
    /// </summary>
    [HttpPost("connections/{id}/disconnect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DisconnectAsync(string id)
    {
        try
        {
            await _mcpService.DisconnectAsync(id);
            return Ok(new { message = "Disconnected successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Connection not found {ConnectionId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from MCP endpoint {ConnectionId}", id);
            return StatusCode(500, "Failed to disconnect");
        }
    }

    /// <summary>
    /// Get MCP message history.
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MCPMessage>>> GetMessagesAsync(
        [FromQuery] string? connectionId = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            if (limit <= 0 || limit > 1000)
            {
                return BadRequest("Limit must be between 1 and 1000");
            }

            var messages = await _mcpService.GetMessagesAsync(connectionId, limit);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MCP messages");
            return StatusCode(500, "Failed to get messages");
        }
    }

    /// <summary>
    /// Send an MCP message.
    /// </summary>
    [HttpPost("messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MCPMessage>> SendMessageAsync([FromBody] MCPMessage message)
    {
        try
        {
            if (string.IsNullOrEmpty(message.Method) || string.IsNullOrEmpty(message.ConnectionId))
            {
                return BadRequest("Method and ConnectionId are required");
            }

            var sentMessage = await _mcpService.SendMessageAsync(message);
            return Ok(sentMessage);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid message data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send MCP message");
            return StatusCode(500, "Failed to send message");
        }
    }

    /// <summary>
    /// Get available MCP tools.
    /// </summary>
    [HttpGet("tools")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MCPTool>>> GetToolsAsync()
    {
        try
        {
            var tools = await _mcpService.GetToolsAsync();
            return Ok(tools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MCP tools");
            return StatusCode(500, "Failed to get tools");
        }
    }
}