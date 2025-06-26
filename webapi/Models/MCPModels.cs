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

/// <summary>
/// Browser session information.
/// </summary>
public class MCPBrowserSession
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Session name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current URL in the browser.
    /// </summary>
    public string CurrentUrl { get; set; } = string.Empty;

    /// <summary>
    /// Browser session status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MCPBrowserStatus Status { get; set; } = MCPBrowserStatus.Closed;

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the session was last active.
    /// </summary>
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Browser viewport size.
    /// </summary>
    public MCPBrowserViewport? Viewport { get; set; }

    /// <summary>
    /// Session metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Browser status enumeration.
/// </summary>
public enum MCPBrowserStatus
{
    Closed,
    Starting,
    Active,
    Loading,
    Error
}

/// <summary>
/// Browser viewport configuration.
/// </summary>
public class MCPBrowserViewport
{
    /// <summary>
    /// Viewport width.
    /// </summary>
    public int Width { get; set; } = 1920;

    /// <summary>
    /// Viewport height.
    /// </summary>
    public int Height { get; set; } = 1080;

    /// <summary>
    /// Device scale factor.
    /// </summary>
    public double DeviceScaleFactor { get; set; } = 1.0;

    /// <summary>
    /// Whether the viewport is mobile.
    /// </summary>
    public bool IsMobile { get; set; } = false;
}

/// <summary>
/// Browser action request.
/// </summary>
public class MCPBrowserActionRequest
{
    /// <summary>
    /// Browser session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Action type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MCPBrowserActionType Action { get; set; }

    /// <summary>
    /// Action parameters.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Browser action types.
/// </summary>
public enum MCPBrowserActionType
{
    Navigate,
    Click,
    Type,
    Scroll,
    Screenshot,
    GetContent,
    GetTitle,
    WaitForElement,
    ExecuteScript,
    Back,
    Forward,
    Refresh,
    Close
}

/// <summary>
/// Browser action response.
/// </summary>
public class MCPBrowserActionResponse
{
    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Error message if action failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Action execution timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}