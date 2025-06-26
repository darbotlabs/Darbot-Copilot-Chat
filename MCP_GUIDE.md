# Model Context Protocol (MCP) Integration Guide

This guide explains how to use the Model Context Protocol (MCP) functionality in Darbot Copilot Chat to connect with external AI services and applications.

## 🔗 What is MCP?

The Model Context Protocol (MCP) is an open protocol that enables secure connections between host applications (like IDEs, AI tools, or chat applications) and AI models. It allows bidirectional communication and tool sharing between different AI services.

## 🚀 Getting Started

### 1. Access the MCP Tab

1. Open Darbot Copilot Chat in your browser
2. Navigate to any chat conversation
3. Click on the **MCP** tab next to Documents, Plans, and Persona tabs

### 2. Start the MCP Server

The built-in MCP server allows other applications to connect to Darbot Copilot Chat:

1. In the MCP tab, go to the **MCP Server** section
2. Configure the server port (default: 3000)
3. Toggle the **MCP Server** switch to enable it
4. The server will start on `mcp://localhost:{port}`

### 3. Add MCP Connections

To connect to external MCP services:

1. In the **MCP Connections** section, fill out:
   - **Connection Name**: A descriptive name (e.g., "GitHub Copilot")
   - **MCP URI**: The MCP endpoint (e.g., `mcp://localhost:3001`)
2. Click **Add Connection**
3. Click **Connect** to establish the connection

## 🔧 Configuration Examples

### Connecting to GitHub Copilot

```javascript
// Example MCP URI for GitHub Copilot integration
Connection Name: GitHub Copilot
MCP URI: mcp://localhost:3001
```

### Connecting to VS Code Extensions

```javascript
// Example MCP URI for VS Code MCP extension
Connection Name: VS Code MCP
MCP URI: mcp://localhost:3002
```

### Connecting to Custom AI Services

```javascript
// Example MCP URI for custom AI service
Connection Name: Custom AI Service
MCP URI: mcp://your-service.com:3000
```

## 📡 Available Tools

Darbot Copilot Chat exposes the following tools via MCP:

### `chat`
Send a message to a chat conversation.
```json
{
  "name": "chat",
  "description": "Send a message to the chat conversation",
  "parameters": {
    "message": "string - The message to send",
    "chatId": "string - The chat conversation ID"
  }
}
```

### `search_memory`
Search through chat memory and documents.
```json
{
  "name": "search_memory",
  "description": "Search through chat memory and documents",
  "parameters": {
    "query": "string - The search query",
    "chatId": "string - The chat conversation ID"
  }
}
```

### `get_conversation_history`
Get the conversation history for a chat.
```json
{
  "name": "get_conversation_history",
  "description": "Get the conversation history for a chat",
  "parameters": {
    "chatId": "string - The chat conversation ID",
    "limit": "number - Maximum number of messages to return (default: 50)"
  }
}
```

## 🔄 Message Flow

### Inbound Messages
External applications can send the following types of messages:

- **initialize**: Establish connection and exchange capabilities
- **list_tools**: Get available tools and their schemas
- **call_tool**: Execute a specific tool with parameters

### Outbound Messages
Darbot Copilot Chat can send:

- **notifications**: Server status changes, connection events
- **responses**: Results from tool calls and requests
- **errors**: Error information and debugging details

## 📊 Monitoring

### Message Log
The MCP tab includes a real-time message log showing:
- Timestamp of each message
- Direction (inbound/outbound)
- Message type (request/response/notification)
- Method name
- Full message content

### Connection Status
Monitor connection health with real-time status indicators:
- 🟢 **Connected**: Active and healthy connection
- 🔴 **Disconnected**: No active connection
- 🟡 **Connecting**: Connection in progress

## 🛠️ Development

### Creating MCP Clients

To create an application that connects to Darbot Copilot Chat:

1. Implement MCP client protocol
2. Connect to `mcp://localhost:{port}` where port is your configured server port
3. Send `initialize` message to establish connection
4. Use `list_tools` to discover available capabilities
5. Call tools using `call_tool` messages

### Example Client Code (Node.js)

```javascript
const net = require('net');

class MCPClient {
    constructor(host, port) {
        this.host = host;
        this.port = port;
        this.socket = null;
    }

    connect() {
        return new Promise((resolve, reject) => {
            this.socket = net.createConnection(this.port, this.host);
            
            this.socket.on('connect', () => {
                console.log('Connected to MCP server');
                this.initialize().then(resolve).catch(reject);
            });
            
            this.socket.on('data', (data) => {
                const message = JSON.parse(data.toString());
                this.handleMessage(message);
            });
            
            this.socket.on('error', reject);
        });
    }

    async initialize() {
        const message = {
            id: Date.now().toString(),
            type: 'request',
            method: 'initialize',
            content: {
                clientInfo: { name: 'My MCP Client', version: '1.0.0' }
            }
        };
        
        return this.sendMessage(message);
    }

    async listTools() {
        const message = {
            id: Date.now().toString(),
            type: 'request',
            method: 'list_tools',
            content: {}
        };
        
        return this.sendMessage(message);
    }

    async callTool(toolName, parameters) {
        const message = {
            id: Date.now().toString(),
            type: 'request',
            method: 'call_tool',
            content: {
                tool: toolName,
                parameters: parameters
            }
        };
        
        return this.sendMessage(message);
    }

    sendMessage(message) {
        return new Promise((resolve, reject) => {
            const data = JSON.stringify(message);
            this.socket.write(data, (error) => {
                if (error) reject(error);
                else resolve();
            });
        });
    }

    handleMessage(message) {
        console.log('Received message:', message);
        // Handle different message types here
    }
}

// Usage
const client = new MCPClient('localhost', 3000);
client.connect().then(async () => {
    const tools = await client.listTools();
    console.log('Available tools:', tools);
    
    // Send a chat message
    await client.callTool('chat', {
        message: 'Hello from MCP client!',
        chatId: 'your-chat-id'
    });
});
```

## 🔐 Security Considerations

### Authentication
- MCP connections currently use direct TCP connections
- Implement additional authentication layers for production use
- Consider using TLS encryption for secure communication

### Network Security
- Configure firewall rules to restrict MCP server access
- Use VPN or private networks for remote connections
- Monitor connection attempts and unusual activity

## 🐛 Troubleshooting

### Common Issues

**Connection Refused**
- Verify MCP server is enabled and running
- Check port configuration and firewall settings
- Ensure no other services are using the same port

**Tool Call Failures**
- Verify tool parameters match the expected schema
- Check that chat IDs are valid and accessible
- Review message logs for error details

**Message Format Errors**
- Ensure JSON messages are properly formatted
- Verify required fields are included
- Check that message types match expected values

### Debug Mode
Enable debug logging by setting environment variable:
```bash
export MCP_DEBUG=true
```

## 📚 Additional Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [MCP GitHub Repository](https://github.com/modelcontextprotocol/protocol)
- [Semantic Kernel Documentation](https://learn.microsoft.com/semantic-kernel/)
- [Contributing to MCP](CONTRIBUTING.md)