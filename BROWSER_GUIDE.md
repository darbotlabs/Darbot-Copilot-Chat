# Browser Integration with MCP Guide

This guide covers the integrated Chrome-based browser functionality in Darbot Copilot Chat, which provides full browser automation and interaction capabilities through the Model Context Protocol (MCP).

## Overview

The browser integration allows:
- **Direct browser interaction within the chat interface**
- **Chat-to-browser communication via MCP**
- **External tool access to browser sessions over MCP**
- **Real-time browser automation and scripting**
- **Screenshot capture and content extraction**

## Features

### Browser Tab Interface
- **Session Management**: Create, start, stop, and delete browser sessions
- **Navigation Controls**: URL bar, back/forward buttons, refresh
- **Embedded Browser**: Full iframe browser display with sandboxing
- **Script Execution**: Run JavaScript directly in browser context
- **Action Logging**: Real-time log of all browser actions

### MCP Integration
- **Browser Tools**: 9+ MCP tools for browser automation
- **Session APIs**: RESTful endpoints for session management
- **Real-time Communication**: MCP protocol for chat interaction
- **External Access**: Allow other tools to control browser sessions

## Browser Tab Usage

### Creating a Browser Session

1. Navigate to the **Browser** tab in the chat interface
2. In the "Browser Sessions" section, enter:
   - **Session Name**: e.g., "Research Session"
   - **Initial URL**: e.g., "https://www.google.com"
3. Click **Add Session**

### Starting and Using a Session

1. Click **Start** on a browser session
2. Select the session to make it active
3. Use the navigation controls:
   - Enter URL in the address bar and click **Go**
   - Use back/forward/refresh buttons
   - Take screenshots with the camera button

### Script Execution

1. Scroll to the "MCP Script Execution" section
2. Enter JavaScript code in the text area:
   ```javascript
   document.querySelector('title').textContent
   ```
3. Click **Execute Script** to run the code
4. View results in the results text area

## MCP Tools for Browser Control

The browser integration provides the following MCP tools:

### Navigation Tools
- **`browser_navigate`**: Navigate to a URL
- **`browser_create_session`**: Create new browser session
- **`browser_list_sessions`**: List all available sessions

### Interaction Tools
- **`browser_click`**: Click elements by CSS selector
- **`browser_type`**: Type text into input fields
- **`browser_wait_for_element`**: Wait for elements to appear

### Content Tools
- **`browser_get_content`**: Extract page title and content
- **`browser_screenshot`**: Capture full page screenshots
- **`browser_execute_script`**: Run custom JavaScript

## API Endpoints

### Session Management
```http
GET    /api/browser/sessions                    # List all sessions
POST   /api/browser/sessions                    # Create session
GET    /api/browser/sessions/{id}               # Get session details
POST   /api/browser/sessions/{id}/start         # Start session
POST   /api/browser/sessions/{id}/stop          # Stop session
DELETE /api/browser/sessions/{id}               # Delete session
```

### Browser Actions
```http
POST   /api/browser/sessions/{id}/actions       # Execute action
POST   /api/browser/sessions/{id}/navigate      # Navigate to URL
POST   /api/browser/sessions/{id}/screenshot    # Take screenshot
POST   /api/browser/sessions/{id}/execute       # Run JavaScript
GET    /api/browser/sessions/{id}/content       # Get page content
```

## Chat Integration Examples

### Basic Navigation
```
Chat: "Open Google and search for artificial intelligence"
```
The chat can use MCP tools to:
1. Create or select a browser session
2. Navigate to google.com
3. Find the search box
4. Type "artificial intelligence"
5. Click search
6. Report results back to chat

### Content Extraction
```
Chat: "What's the title of the current page?"
```
The chat uses `browser_get_content` to extract and return the page title.

### Screenshot and Analysis
```
Chat: "Take a screenshot and describe what you see"
```
The chat can:
1. Use `browser_screenshot` to capture the page
2. Analyze the image content
3. Provide a description of the page

## External Tool Integration

### MCP Client Example (Node.js)
```javascript
import { MCPClient } from '@modelcontextprotocol/sdk';

const client = new MCPClient();
await client.connect('mcp://localhost:3000');

// Create browser session
const session = await client.callTool('browser_create_session', {
  name: 'External Session',
  initialUrl: 'https://example.com'
});

// Navigate and interact
await client.callTool('browser_navigate', {
  sessionId: session.id,
  url: 'https://github.com'
});

// Take screenshot
const screenshot = await client.callTool('browser_screenshot', {
  sessionId: session.id
});
```

### Python Example
```python
import asyncio
from mcp_client import MCPClient

async def automate_browser():
    client = MCPClient()
    await client.connect('mcp://localhost:3000')
    
    # Create session
    result = await client.call_tool('browser_create_session', {
        'name': 'Python Session',
        'initialUrl': 'https://www.wikipedia.org'
    })
    
    session_id = result['data']['id']
    
    # Search for something
    await client.call_tool('browser_type', {
        'sessionId': session_id,
        'selector': 'input[name="search"]',
        'text': 'machine learning'
    })
    
    await client.call_tool('browser_click', {
        'sessionId': session_id,
        'selector': 'button[type="submit"]'
    })

asyncio.run(automate_browser())
```

## Security Considerations

### Sandboxing
The browser iframe includes sandbox restrictions:
- `allow-same-origin`: Required for content access
- `allow-scripts`: Enables JavaScript execution
- `allow-forms`: Allows form submissions
- `allow-popups`: Controlled popup handling

### Content Security
- All browser sessions run in isolated processes
- Screenshots are base64 encoded for safe transmission
- Script execution is logged and monitored
- Rate limiting prevents abuse

### Network Access
- Browser sessions have full internet access
- Consider firewall rules for production deployments
- Monitor outbound connections for security

## Production Deployment

### Requirements
- **Chrome/Chromium**: Automatically downloaded by PuppeteerSharp
- **Memory**: ~100MB per browser session
- **CPU**: Moderate for browser rendering
- **Disk**: Space for Chromium and screenshots

### Configuration
```json
{
  "Browser": {
    "MaxSessions": 10,
    "SessionTimeout": "01:00:00",
    "ScreenshotQuality": 80,
    "HeadlessMode": true,
    "EnableLogging": true
  }
}
```

### Scaling
- Each browser session runs in a separate process
- Consider horizontal scaling for many concurrent users
- Implement session cleanup and resource monitoring

## Troubleshooting

### Common Issues

**Browser won't start**
- Check Chrome/Chromium installation
- Verify sufficient memory and CPU
- Check firewall/security restrictions

**JavaScript execution fails**
- Verify page has finished loading
- Check for JavaScript errors in browser console
- Ensure selector exists before interaction

**Screenshots are blank**
- Wait for page content to load
- Check viewport size configuration
- Verify page is not blocked by CORS

**Session cleanup**
- Implement timeout mechanisms
- Monitor browser process memory usage
- Regularly clean up inactive sessions

### Debug Mode
Enable detailed logging:
```json
{
  "Logging": {
    "LogLevel": {
      "CopilotChat.WebApi.Services.BrowserService": "Debug"
    }
  }
}
```

## Advanced Usage

### Custom Browser Configuration
```javascript
// Custom viewport and device emulation
await client.callTool('browser_create_session', {
  name: 'Mobile Session',
  viewport: {
    width: 375,
    height: 667,
    deviceScaleFactor: 2,
    isMobile: true
  }
});
```

### Complex Automation Workflows
```javascript
// Multi-step automation example
const workflow = [
  { action: 'navigate', url: 'https://example.com/login' },
  { action: 'type', selector: '#username', text: 'user@example.com' },
  { action: 'type', selector: '#password', text: 'password' },
  { action: 'click', selector: '#login-button' },
  { action: 'wait_for_element', selector: '.dashboard' },
  { action: 'screenshot' }
];

for (const step of workflow) {
  await client.callTool(`browser_${step.action}`, {
    sessionId: sessionId,
    ...step
  });
}
```

This browser integration provides a powerful foundation for web automation, testing, and interactive browsing within the chat environment, all accessible through the standardized MCP protocol.