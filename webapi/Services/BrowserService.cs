// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Concurrent;
using CopilotChat.WebApi.Models;
using PuppeteerSharp;

namespace CopilotChat.WebApi.Services;

/// <summary>
/// Browser automation service using PuppeteerSharp.
/// </summary>
public class BrowserService : IBrowserService, IDisposable
{
    private static readonly string[] BrowserArgs = { "--no-sandbox", "--disable-setuid-sandbox" };
    
    private readonly ILogger<BrowserService> _logger;
    private readonly ConcurrentDictionary<string, MCPBrowserSession> _sessions = new();
    private readonly ConcurrentDictionary<string, IBrowser> _browsers = new();
    private readonly ConcurrentDictionary<string, IPage> _pages = new();
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserService"/> class.
    /// </summary>
    public BrowserService(ILogger<BrowserService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<MCPBrowserSession>> GetSessionsAsync()
    {
        return Task.FromResult(_sessions.Values.AsEnumerable());
    }

    /// <inheritdoc/>
    public Task<MCPBrowserSession?> GetSessionAsync(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    /// <inheritdoc/>
    public Task<MCPBrowserSession> CreateSessionAsync(string name, string? initialUrl = null, MCPBrowserViewport? viewport = null)
    {
        var session = new MCPBrowserSession
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            CurrentUrl = initialUrl ?? "about:blank",
            Status = MCPBrowserStatus.Closed,
            CreatedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow,
            Viewport = viewport ?? new MCPBrowserViewport()
        };

        _sessions.TryAdd(session.Id, session);
        _logger.LogInformation("Created browser session {SessionId} with name '{Name}'", session.Id, name);

        return Task.FromResult(session);
    }

    /// <inheritdoc/>
    public async Task<MCPBrowserSession?> StartSessionAsync(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }

        try
        {
            session.Status = MCPBrowserStatus.Starting;
            session.LastActiveAt = DateTime.UtcNow;

            // Download Chromium if needed
            await new BrowserFetcher().DownloadAsync();

            // Launch browser
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = BrowserArgs
            });

            var page = await browser.NewPageAsync();

            // Set viewport
            if (session.Viewport != null)
            {
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = session.Viewport.Width,
                    Height = session.Viewport.Height,
                    DeviceScaleFactor = session.Viewport.DeviceScaleFactor,
                    IsMobile = session.Viewport.IsMobile
                });
            }

            _browsers.TryAdd(sessionId, browser);
            _pages.TryAdd(sessionId, page);

            session.Status = MCPBrowserStatus.Active;
            session.LastActiveAt = DateTime.UtcNow;

            // Navigate to initial URL if specified
            if (!string.IsNullOrEmpty(session.CurrentUrl) && session.CurrentUrl != "about:blank")
            {
                await page.GoToAsync(session.CurrentUrl);
            }

            _logger.LogInformation("Started browser session {SessionId}", sessionId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start browser session {SessionId}", sessionId);
            session.Status = MCPBrowserStatus.Error;
            return session;
        }
    }

    /// <inheritdoc/>
    public async Task<MCPBrowserSession?> StopSessionAsync(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return null;
        }

        try
        {
            if (_browsers.TryRemove(sessionId, out var browser))
            {
                await browser.CloseAsync();
            }

            _pages.TryRemove(sessionId, out _);

            session.Status = MCPBrowserStatus.Closed;
            session.LastActiveAt = DateTime.UtcNow;

            _logger.LogInformation("Stopped browser session {SessionId}", sessionId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop browser session {SessionId}", sessionId);
            session.Status = MCPBrowserStatus.Error;
            return session;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteSessionAsync(string sessionId)
    {
        if (!_sessions.TryRemove(sessionId, out var session))
        {
            return false;
        }

        try
        {
            // Stop the session first if it's running
            if (session.Status == MCPBrowserStatus.Active || session.Status == MCPBrowserStatus.Loading)
            {
                await StopSessionAsync(sessionId);
            }

            _logger.LogInformation("Deleted browser session {SessionId}", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete browser session {SessionId}", sessionId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<MCPBrowserActionResponse> ExecuteActionAsync(MCPBrowserActionRequest request)
    {
        if (!_sessions.TryGetValue(request.SessionId, out var session))
        {
            return new MCPBrowserActionResponse
            {
                Success = false,
                Error = $"Session '{request.SessionId}' not found"
            };
        }

        if (!_pages.TryGetValue(request.SessionId, out var page))
        {
            return new MCPBrowserActionResponse
            {
                Success = false,
                Error = $"No active page for session '{request.SessionId}'. Please start the session first."
            };
        }

        try
        {
            session.Status = MCPBrowserStatus.Loading;
            session.LastActiveAt = DateTime.UtcNow;

            var response = await ExecuteActionInternalAsync(page, session, request);

            session.Status = MCPBrowserStatus.Active;
            session.LastActiveAt = DateTime.UtcNow;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute action {Action} for session {SessionId}", request.Action, request.SessionId);

            session.Status = MCPBrowserStatus.Error;

            return new MCPBrowserActionResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<MCPBrowserActionResponse> ExecuteActionInternalAsync(IPage page, MCPBrowserSession session, MCPBrowserActionRequest request)
    {
        switch (request.Action)
        {
            case MCPBrowserActionType.Navigate:
                if (request.Parameters.TryGetValue("url", out var urlObj) && urlObj is string url)
                {
                    await page.GoToAsync(url);
                    session.CurrentUrl = url;
                    return new MCPBrowserActionResponse
                    {
                        Success = true,
                        Data = new { url = url, title = await page.GetTitleAsync() }
                    };
                }
                return new MCPBrowserActionResponse { Success = false, Error = "URL parameter is required" };

            case MCPBrowserActionType.Click:
                if (request.Parameters.TryGetValue("selector", out var selectorObj) && selectorObj is string selector)
                {
                    await page.ClickAsync(selector);
                    return new MCPBrowserActionResponse { Success = true, Data = new { selector } };
                }
                return new MCPBrowserActionResponse { Success = false, Error = "Selector parameter is required" };

            case MCPBrowserActionType.Type:
                if (request.Parameters.TryGetValue("selector", out var typeSelectorObj) && typeSelectorObj is string typeSelector &&
                    request.Parameters.TryGetValue("text", out var textObj) && textObj is string text)
                {
                    await page.TypeAsync(typeSelector, text);
                    return new MCPBrowserActionResponse { Success = true, Data = new { selector = typeSelector, text } };
                }
                return new MCPBrowserActionResponse { Success = false, Error = "Selector and text parameters are required" };

            case MCPBrowserActionType.Screenshot:
                var screenshot = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Type = ScreenshotType.Png,
                    FullPage = true
                });
                var base64Screenshot = Convert.ToBase64String(screenshot);
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { screenshot = base64Screenshot, format = "png" }
                };

            case MCPBrowserActionType.GetContent:
                var title = await page.GetTitleAsync();
                var content = await page.GetContentAsync();
                var currentUrl = page.Url;
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { title, content, url = currentUrl }
                };

            case MCPBrowserActionType.GetTitle:
                var pageTitle = await page.GetTitleAsync();
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { title = pageTitle }
                };

            case MCPBrowserActionType.ExecuteScript:
                if (request.Parameters.TryGetValue("script", out var scriptObj) && scriptObj is string script)
                {
                    var result = await page.EvaluateExpressionAsync(script);
                    return new MCPBrowserActionResponse
                    {
                        Success = true,
                        Data = new { script, result }
                    };
                }
                return new MCPBrowserActionResponse { Success = false, Error = "Script parameter is required" };

            case MCPBrowserActionType.Back:
                await page.GoBackAsync();
                session.CurrentUrl = page.Url;
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { url = page.Url }
                };

            case MCPBrowserActionType.Forward:
                await page.GoForwardAsync();
                session.CurrentUrl = page.Url;
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { url = page.Url }
                };

            case MCPBrowserActionType.Refresh:
                await page.ReloadAsync();
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { url = page.Url }
                };

            case MCPBrowserActionType.WaitForElement:
                if (request.Parameters.TryGetValue("selector", out var waitSelectorObj) && waitSelectorObj is string waitSelector)
                {
                    var timeout = 30000; // Default 30 seconds
                    if (request.Parameters.TryGetValue("timeout", out var timeoutObj) && timeoutObj is int timeoutValue)
                    {
                        timeout = timeoutValue;
                    }

                    await page.WaitForSelectorAsync(waitSelector, new WaitForSelectorOptions { Timeout = timeout });
                    return new MCPBrowserActionResponse
                    {
                        Success = true,
                        Data = new { selector = waitSelector, timeout }
                    };
                }
                return new MCPBrowserActionResponse { Success = false, Error = "Selector parameter is required" };

            case MCPBrowserActionType.Scroll:
                var x = 0;
                var y = 0;
                if (request.Parameters.TryGetValue("x", out var xObj) && xObj is int xValue)
                {
                    x = xValue;
                }
                if (request.Parameters.TryGetValue("y", out var yObj) && yObj is int yValue)
                {
                    y = yValue;
                }

                await page.EvaluateExpressionAsync($"window.scrollTo({x}, {y})");
                return new MCPBrowserActionResponse
                {
                    Success = true,
                    Data = new { x, y }
                };

            default:
                return new MCPBrowserActionResponse
                {
                    Success = false,
                    Error = $"Unknown action: {request.Action}"
                };
        }
    }

    /// <summary>
    /// Dispose of resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            try
            {
                // Close all browsers
                foreach (var browser in _browsers.Values)
                {
                    browser.CloseAsync().Wait(5000); // Wait up to 5 seconds
                }

                _browsers.Clear();
                _pages.Clear();
                _sessions.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing browser service");
            }
        }

        _disposed = true;
    }
}