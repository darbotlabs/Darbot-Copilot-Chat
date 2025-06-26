// Copyright (c) Microsoft. All rights reserved.

using CopilotChat.WebApi.Models;
using CopilotChat.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CopilotChat.WebApi.Controllers;

/// <summary>
/// Controller for managing browser automation via MCP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BrowserController : ControllerBase
{
    private readonly IBrowserService _browserService;
    private readonly ILogger<BrowserController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserController"/> class.
    /// </summary>
    public BrowserController(IBrowserService browserService, ILogger<BrowserController> logger)
    {
        _browserService = browserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all browser sessions.
    /// </summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<MCPBrowserSession>>> GetSessionsAsync()
    {
        try
        {
            var sessions = await _browserService.GetSessionsAsync();
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting browser sessions");
            return StatusCode(500, "Failed to get browser sessions");
        }
    }

    /// <summary>
    /// Get a specific browser session.
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<MCPBrowserSession>> GetSessionAsync(string sessionId)
    {
        try
        {
            var session = await _browserService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session '{sessionId}' not found");
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting browser session {SessionId}", sessionId);
            return StatusCode(500, "Failed to get browser session");
        }
    }

    /// <summary>
    /// Create a new browser session.
    /// </summary>
    [HttpPost("sessions")]
    public async Task<ActionResult<MCPBrowserSession>> CreateSessionAsync([FromBody] CreateBrowserSessionRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Session name is required");
            }

            var session = await _browserService.CreateSessionAsync(request.Name, request.InitialUrl, request.Viewport);
            return CreatedAtAction(nameof(GetSessionAsync), new { sessionId = session.Id }, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating browser session");
            return StatusCode(500, "Failed to create browser session");
        }
    }

    /// <summary>
    /// Start a browser session.
    /// </summary>
    [HttpPost("sessions/{sessionId}/start")]
    public async Task<ActionResult<MCPBrowserSession>> StartSessionAsync(string sessionId)
    {
        try
        {
            var session = await _browserService.StartSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session '{sessionId}' not found");
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting browser session {SessionId}", sessionId);
            return StatusCode(500, "Failed to start browser session");
        }
    }

    /// <summary>
    /// Stop a browser session.
    /// </summary>
    [HttpPost("sessions/{sessionId}/stop")]
    public async Task<ActionResult<MCPBrowserSession>> StopSessionAsync(string sessionId)
    {
        try
        {
            var session = await _browserService.StopSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound($"Session '{sessionId}' not found");
            }
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping browser session {SessionId}", sessionId);
            return StatusCode(500, "Failed to stop browser session");
        }
    }

    /// <summary>
    /// Delete a browser session.
    /// </summary>
    [HttpDelete("sessions/{sessionId}")]
    public async Task<ActionResult> DeleteSessionAsync(string sessionId)
    {
        try
        {
            var success = await _browserService.DeleteSessionAsync(sessionId);
            if (!success)
            {
                return NotFound($"Session '{sessionId}' not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting browser session {SessionId}", sessionId);
            return StatusCode(500, "Failed to delete browser session");
        }
    }

    /// <summary>
    /// Execute a browser action.
    /// </summary>
    [HttpPost("sessions/{sessionId}/actions")]
    public async Task<ActionResult<MCPBrowserActionResponse>> ExecuteActionAsync(
        string sessionId,
        [FromBody] MCPBrowserActionRequest request)
    {
        try
        {
            if (request.SessionId != sessionId)
            {
                return BadRequest("Session ID mismatch");
            }

            var response = await _browserService.ExecuteActionAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing browser action for session {SessionId}", sessionId);
            return StatusCode(500, "Failed to execute browser action");
        }
    }

    /// <summary>
    /// Navigate to a URL.
    /// </summary>
    [HttpPost("sessions/{sessionId}/navigate")]
    public async Task<ActionResult<MCPBrowserActionResponse>> NavigateAsync(
        string sessionId,
        [FromBody] NavigateRequest request)
    {
        try
        {
            var actionRequest = new MCPBrowserActionRequest
            {
                SessionId = sessionId,
                Action = MCPBrowserActionType.Navigate,
                Parameters = new Dictionary<string, object> { ["url"] = request.Url }
            };

            var response = await _browserService.ExecuteActionAsync(actionRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating browser session {SessionId} to {Url}", sessionId, request.Url);
            return StatusCode(500, "Failed to navigate");
        }
    }

    /// <summary>
    /// Take a screenshot of the current page.
    /// </summary>
    [HttpPost("sessions/{sessionId}/screenshot")]
    public async Task<ActionResult<MCPBrowserActionResponse>> TakeScreenshotAsync(string sessionId)
    {
        try
        {
            var actionRequest = new MCPBrowserActionRequest
            {
                SessionId = sessionId,
                Action = MCPBrowserActionType.Screenshot,
                Parameters = new Dictionary<string, object>()
            };

            var response = await _browserService.ExecuteActionAsync(actionRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error taking screenshot for session {SessionId}", sessionId);
            return StatusCode(500, "Failed to take screenshot");
        }
    }

    /// <summary>
    /// Execute JavaScript in the browser.
    /// </summary>
    [HttpPost("sessions/{sessionId}/execute")]
    public async Task<ActionResult<MCPBrowserActionResponse>> ExecuteScriptAsync(
        string sessionId,
        [FromBody] ExecuteScriptRequest request)
    {
        try
        {
            var actionRequest = new MCPBrowserActionRequest
            {
                SessionId = sessionId,
                Action = MCPBrowserActionType.ExecuteScript,
                Parameters = new Dictionary<string, object> { ["script"] = request.Script }
            };

            var response = await _browserService.ExecuteActionAsync(actionRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing script for session {SessionId}", sessionId);
            return StatusCode(500, "Failed to execute script");
        }
    }

    /// <summary>
    /// Get page content (title, text, etc.).
    /// </summary>
    [HttpGet("sessions/{sessionId}/content")]
    public async Task<ActionResult<MCPBrowserActionResponse>> GetContentAsync(string sessionId)
    {
        try
        {
            var actionRequest = new MCPBrowserActionRequest
            {
                SessionId = sessionId,
                Action = MCPBrowserActionType.GetContent,
                Parameters = new Dictionary<string, object>()
            };

            var response = await _browserService.ExecuteActionAsync(actionRequest);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content for session {SessionId}", sessionId);
            return StatusCode(500, "Failed to get page content");
        }
    }
}

/// <summary>
/// Request model for creating a browser session.
/// </summary>
public class CreateBrowserSessionRequest
{
    /// <summary>
    /// Session name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Initial URL to navigate to.
    /// </summary>
    public string? InitialUrl { get; set; }

    /// <summary>
    /// Browser viewport configuration.
    /// </summary>
    public MCPBrowserViewport? Viewport { get; set; }
}

/// <summary>
/// Request model for navigation.
/// </summary>
public class NavigateRequest
{
    /// <summary>
    /// URL to navigate to.
    /// </summary>
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Request model for script execution.
/// </summary>
public class ExecuteScriptRequest
{
    /// <summary>
    /// JavaScript code to execute.
    /// </summary>
    public string Script { get; set; } = string.Empty;
}