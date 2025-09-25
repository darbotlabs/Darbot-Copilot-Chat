# GitHub Copilot Instructions for Darbot Copilot Chat

## Project Overview

**Darbot Copilot Chat** is an enterprise-ready AI chat application built on Microsoft Semantic Kernel with integrated Model Context Protocol (MCP) support. The application enables seamless integration with external AI services and supports multi-user collaborative AI conversations.

### Architecture
- **Frontend**: React 18+ with TypeScript, Fluent UI components, cyberpunk-themed interface
- **Backend**: .NET 9 Web API service with SignalR for real-time communication
- **Memory Pipeline**: .NET 9 worker service for processing semantic memory
- **Browser Integration**: Chrome-based browser with MCP automation support
- **Plugin System**: Extensible architecture with built-in web search capabilities

## Core Technologies & Frameworks

### Backend (.NET 9)
- **Microsoft Semantic Kernel**: Core AI orchestration framework
- **SignalR**: Real-time chat communication
- **Azure OpenAI/OpenAI**: LLM integration
- **Entity Framework**: Data access layer
- **PuppeteerSharp**: Browser automation
- **Azure Services**: Cosmos DB, Cognitive Search, Application Insights
- **Authentication**: Azure AD/MSAL integration

### Frontend (React/TypeScript)
- **React 18**: UI framework with hooks and modern patterns
- **TypeScript 5.5+**: Type safety and development experience
- **Fluent UI**: Microsoft's design system components
- **Redux Toolkit**: State management
- **SignalR Client**: Real-time communication
- **React Markdown**: Message rendering with GitHub Flavored Markdown

### Build & Development Tools
- **.NET CLI**: Primary build system (`dotnet build`, `dotnet test`)
- **npm/yarn**: Frontend package management
- **ESLint/Prettier**: TypeScript/JavaScript linting and formatting
- **dotnet format**: C# code formatting
- **Playwright**: End-to-end testing

## Development Guidelines

### Code Style & Standards

#### .NET/C# Guidelines
- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `dotnet format` for automatic formatting
- Apply `this.` qualification for member access
- Prefer explicit type declarations for clarity
- Use async/await patterns consistently
- Follow dependency injection patterns established in `Program.cs`

#### TypeScript/React Guidelines
- Follow TypeScript ESLint rules and React best practices
- Use functional components with hooks over class components
- Implement proper prop typing with TypeScript interfaces
- Use Fluent UI components consistently for UI elements
- Follow established Redux Toolkit patterns for state management
- Use `yarn lint:fix` for automatic linting corrections

### Project Structure

#### Backend Structure (`webapi/`)
```
Controllers/     # API endpoints
Services/        # Business logic and external integrations
Storage/         # Data access layer
Plugins/         # Semantic Kernel plugins
Auth/           # Authentication logic
Models/         # Data transfer objects
Options/        # Configuration models
```

#### Frontend Structure (`webapp/src/`)
```
components/     # Reusable React components
libs/          # Utility libraries and services
redux/         # State management (actions, reducers, store)
styles/        # SCSS styling
```

### Key Architectural Patterns

#### Semantic Kernel Integration
- Plugins are the primary extension mechanism
- Use `KernelFunction` attributes for function registration
- Implement proper error handling in kernel functions
- Follow established patterns in `Plugins/Chat/ChatPlugin.cs`

#### Real-time Communication
- SignalR hubs in `Hubs/` for chat functionality
- Frontend uses `@microsoft/signalr` client
- Message broadcasting follows established patterns

#### Memory & Document Processing
- Kernel Memory handles document ingestion and semantic search
- Support for distributed processing via `memorypipeline`
- Integration with Azure Cognitive Search and Cosmos DB

### Configuration Management

#### Environment Configuration
- Backend: `appsettings.json` and user secrets
- Frontend: `.env` files for environment variables
- Security-sensitive values should use Azure Key Vault or user secrets
- Follow patterns established in existing configuration files

#### Service Registration
- Use extension methods in `Extensions/` for service configuration
- Follow dependency injection patterns in `Program.cs`
- Implement proper configuration validation

### Testing Guidelines

#### .NET Testing
- Use xUnit for unit tests
- Integration tests in `integration-tests/` project
- Mock external dependencies appropriately
- Test both success and error scenarios

#### Frontend Testing
- React Testing Library for component tests
- Playwright for end-to-end tests
- Test user interactions and state changes

### Build & Deployment

#### Development Commands
```bash
# Backend
dotnet restore
dotnet build
dotnet test
dotnet format  # Code formatting

# Frontend
npm install --legacy-peer-deps
yarn build
yarn test
yarn lint:fix
```

#### Key Build Files
- `CopilotChat.sln`: Main solution file
- `Directory.Build.props`: Shared MSBuild properties
- `Directory.Packages.props`: Centralized package management
- `package.json`: Frontend dependencies and scripts

### Security Considerations

#### Authentication & Authorization
- Azure AD integration for enterprise scenarios
- JWT token validation and refresh patterns
- Proper CORS configuration in backend
- Secure storage of API keys and secrets

#### Data Protection
- Sanitize user inputs for XSS protection
- Validate all API inputs
- Use parameterized queries for database operations
- Implement proper error handling without exposing internals

### Browser Integration & MCP

#### Browser Service
- Chrome automation via PuppeteerSharp
- MCP (Model Context Protocol) integration
- Session management and cleanup
- Error handling for browser operations

#### Plugin Development
- Implement `IPluginFunction` interface
- Use appropriate parameter validation
- Handle asynchronous operations properly
- Follow established plugin patterns

### Performance & Scaling

#### Backend Performance
- Use async/await throughout the stack
- Implement proper caching strategies
- Monitor with Application Insights
- Optimize database queries

#### Frontend Performance
- Use React.memo for expensive components
- Implement proper loading states
- Optimize bundle size with code splitting
- Use Fluent UI efficiently

### Common Patterns & Best Practices

#### Error Handling
- Use structured logging throughout
- Implement graceful degradation
- Return appropriate HTTP status codes
- Handle async operation failures

#### State Management
- Use Redux Toolkit for complex state
- Keep component state local when possible
- Implement proper loading and error states
- Use TypeScript for type safety

#### API Design
- Follow RESTful conventions
- Use appropriate HTTP methods
- Implement proper pagination
- Version APIs appropriately

## Common Issues & Solutions

### Dependency Conflicts
- Use `--legacy-peer-deps` for npm install due to React Scripts TypeScript version constraints
- Keep .NET packages aligned with centralized package management

### Build Issues
- Run `dotnet format` to fix style violations
- Use `yarn lint:fix` for TypeScript/JavaScript formatting
- Check for missing environment configuration

### Authentication Issues
- Verify Azure AD app registration settings
- Check CORS configuration for frontend URLs
- Ensure proper redirect URIs are configured

## Documentation & Resources

- [Main README](../README.md): Setup and deployment instructions
- [Contributing Guide](../CONTRIBUTING.md): Development workflow
- [Browser Guide](../BROWSER_GUIDE.md): Browser integration details
- [MCP Guide](../MCP_GUIDE.md): Model Context Protocol integration
- [Security Guide](../SECURITY_GUIDE.md): Security best practices

When working on this codebase, prioritize maintainability, security, and user experience. Follow established patterns and conventions, and ensure all changes are properly tested and documented.