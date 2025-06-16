// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace CopilotChat.WebApi.Models;

/// <summary>
/// Represents an MCP (Model Context Protocol) connection configuration.
/// </summary>
public class MCPConnection
{
    /// <summary>
    /// Unique identifier for the connection.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for the connection.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// MCP URI for the connection.
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Connection status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MCPConnectionStatus Status { get; set; } = MCPConnectionStatus.Disconnected;

    /// <summary>
    /// Type of connection (server or client).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MCPConnectionType Type { get; set; } = MCPConnectionType.Client;

    /// <summary>
    /// Last successful connection timestamp.
    /// </summary>
    public DateTime? LastConnected { get; set; }

    /// <summary>
    /// Additional connection metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// MCP connection status enumeration.
/// </summary>
public enum MCPConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

/// <summary>
/// MCP connection type enumeration.
/// </summary>
public enum MCPConnectionType
{
    Client,
    Server
}

/// <summary>
/// Represents an MCP message.
/// </summary>
public class MCPMessage
{
    /// <summary>
    /// Unique identifier for the message.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Message timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Message type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MCPMessageType Type { get; set; }

    /// <summary>
    /// Message method/action.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Message content.
    /// </summary>
    public object Content { get; set; } = new();

    /// <summary>
    /// Message direction.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MCPMessageDirection Direction { get; set; }

    /// <summary>
    /// Connection ID this message is associated with.
    /// </summary>
    public string? ConnectionId { get; set; }
}

/// <summary>
/// MCP message type enumeration.
/// </summary>
public enum MCPMessageType
{
    Request,
    Response,
    Notification,
    Error
}

/// <summary>
/// MCP message direction enumeration.
/// </summary>
public enum MCPMessageDirection
{
    Inbound,
    Outbound
}

/// <summary>
/// MCP server configuration.
/// </summary>
public class MCPServerConfig
{
    /// <summary>
    /// Whether the MCP server is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Port the MCP server listens on.
    /// </summary>
    public int Port { get; set; } = 3000;

    /// <summary>
    /// Server capabilities.
    /// </summary>
    public List<string> Capabilities { get; set; } = new() { "chat", "tools", "memory" };

    /// <summary>
    /// Authentication settings.
    /// </summary>
    public MCPAuthConfig? Authentication { get; set; }
}

/// <summary>
/// MCP authentication configuration.
/// </summary>
public class MCPAuthConfig
{
    /// <summary>
    /// Authentication type.
    /// </summary>
    public string Type { get; set; } = "none";

    /// <summary>
    /// API key for authentication.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Additional authentication parameters.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// MCP tool definition.
/// </summary>
public class MCPTool
{
    /// <summary>
    /// Tool name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tool description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Tool parameters schema.
    /// </summary>
    public object Schema { get; set; } = new();

    /// <summary>
    /// Whether the tool is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}