// Copyright (c) Microsoft. All rights reserved.

using CopilotChat.WebApi.Models;

namespace CopilotChat.WebApi.Services;

/// <summary>
/// Interface for browser automation service.
/// </summary>
public interface IBrowserService
{
    /// <summary>
    /// Get all browser sessions.
    /// </summary>
    Task<IEnumerable<MCPBrowserSession>> GetSessionsAsync();

    /// <summary>
    /// Get a specific browser session.
    /// </summary>
    Task<MCPBrowserSession?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Create a new browser session.
    /// </summary>
    Task<MCPBrowserSession> CreateSessionAsync(string name, string? initialUrl = null, MCPBrowserViewport? viewport = null);

    /// <summary>
    /// Start a browser session.
    /// </summary>
    Task<MCPBrowserSession?> StartSessionAsync(string sessionId);

    /// <summary>
    /// Stop a browser session.
    /// </summary>
    Task<MCPBrowserSession?> StopSessionAsync(string sessionId);

    /// <summary>
    /// Delete a browser session.
    /// </summary>
    Task<bool> DeleteSessionAsync(string sessionId);

    /// <summary>
    /// Execute a browser action.
    /// </summary>
    Task<MCPBrowserActionResponse> ExecuteActionAsync(MCPBrowserActionRequest request);
}