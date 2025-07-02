# Production Security Configuration Guide

This document outlines security configurations and best practices for deploying Darbot Copilot Chat in production environments.

## 🔐 Authentication and Authorization

### Azure AD Configuration

#### Backend Security Headers Middleware
```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        
        await _next(context);
    }
}
```

### Input Validation and Sanitization

#### Backend Model Validation
```csharp
public class SecureChatMessageRequest
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    [RegularExpression(@"^[^\<\>\&\""]*$", ErrorMessage = "Invalid characters detected")]
    public string Message { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[a-zA-Z0-9-_]+$")]
    public string ChatId { get; set; } = string.Empty;
}
```

## 🛡️ MCP Security

### Connection Security
```csharp
public class SecureMCPService : MCPService
{
    private readonly Dictionary<string, int> _connectionAttempts = new();
    private const int MAX_CONNECTIONS_PER_IP = 5;
    
    public override async Task<bool> AcceptConnectionAsync(string clientIp)
    {
        if (_connectionAttempts.GetValueOrDefault(clientIp, 0) >= MAX_CONNECTIONS_PER_IP)
        {
            return false;
        }
        
        _connectionAttempts[clientIp] = _connectionAttempts.GetValueOrDefault(clientIp, 0) + 1;
        return await base.AcceptConnectionAsync(clientIp);
    }
}
```

### Message Validation
```csharp
public class MCPMessageValidator
{
    public bool IsValidMessage(MCPMessage message)
    {
        // Validate message structure
        if (string.IsNullOrEmpty(message.Method) || string.IsNullOrEmpty(message.Id))
            return false;
            
        // Validate method name against whitelist
        var allowedMethods = new[] { "initialize", "list_tools", "call_tool", "disconnect" };
        if (!allowedMethods.Contains(message.Method))
            return false;
            
        // Validate content size
        var contentJson = JsonSerializer.Serialize(message.Content);
        if (contentJson.Length > 10000) // 10KB limit
            return false;
            
        return true;
    }
}
```

## 🔒 Data Protection

### Encryption Configuration
```csharp
public class EncryptionService
{
    private readonly IConfiguration _configuration;
    
    public string EncryptSensitiveData(string data)
    {
        var key = _configuration["Encryption:Key"];
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var encrypted = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);
        
        var result = new byte[aes.IV.Length + encrypted.Length];
        aes.IV.CopyTo(result, 0);
        encrypted.CopyTo(result, aes.IV.Length);
        
        return Convert.ToBase64String(result);
    }
}
```

## 🔥 Rate Limiting

### API Rate Limiting
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ApiPolicy", partitioner: httpContext =>
    {
        var userId = httpContext.User?.Identity?.Name ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    
    options.AddPolicy("MCPPolicy", partitioner: httpContext =>
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});
```

## 🔍 Security Monitoring

### Security Event Logging
```csharp
public class SecurityLogger
{
    private readonly ILogger<SecurityLogger> _logger;
    
    public void LogSecurityEvent(string eventType, string details, string? userId = null)
    {
        _logger.LogWarning("Security Event: {EventType} - {Details} - User: {UserId}", 
            eventType, details, userId ?? "Unknown");
    }
    
    public void LogFailedAuthentication(string userId, string ipAddress)
    {
        LogSecurityEvent("FailedAuthentication", $"IP: {ipAddress}", userId);
    }
    
    public void LogSuspiciousMCPActivity(string activity, string clientId)
    {
        LogSecurityEvent("SuspiciousMCPActivity", $"Activity: {activity} - Client: {clientId}");
    }
}
```

## 🌐 Network Security

### Production CORS Policy
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
```

## 📋 Security Checklist

### Pre-Production Security Audit

#### Authentication & Authorization
- [ ] Azure AD properly configured with appropriate scopes
- [ ] JWT tokens validated and expired tokens rejected
- [ ] User permissions properly enforced
- [ ] Service-to-service authentication secured

#### Input Validation & Output Encoding
- [ ] All user inputs validated and sanitized
- [ ] SQL injection prevention implemented
- [ ] XSS protection in place
- [ ] Command injection prevention

#### MCP Security
- [ ] Connection rate limiting implemented
- [ ] Message size limits enforced
- [ ] Method whitelist validation
- [ ] Client authentication required

#### Data Protection
- [ ] Sensitive data encrypted at rest
- [ ] TLS encryption for data in transit
- [ ] Secure key management
- [ ] PII handling compliance

#### Infrastructure Security
- [ ] Security headers configured
- [ ] CORS properly configured
- [ ] Rate limiting enabled
- [ ] Monitoring and alerting active

### Security Testing Commands

#### Dependency Vulnerability Scan
```bash
# .NET dependencies
dotnet list package --vulnerable --include-transitive

# Node.js dependencies
npm audit
yarn audit
```

#### Docker Security Scan
```bash
# Scan container image
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image your-image-name:latest
```

#### Network Security Test
```bash
# Test TLS configuration
nmap --script ssl-enum-ciphers -p 443 your-domain.com

# Test security headers
curl -I https://your-domain.com
```

## 🚨 Incident Response

### Security Incident Playbook

#### Immediate Response (0-15 minutes)
1. Identify and isolate affected systems
2. Preserve evidence and logs
3. Assess scope and impact
4. Notify security team and management

#### Containment (15-60 minutes)
1. Block malicious IP addresses
2. Revoke compromised credentials
3. Disable affected services if necessary
4. Apply emergency patches

#### Recovery and Lessons Learned
1. Restore services from clean backups
2. Update security configurations
3. Document incident and response
4. Implement additional safeguards

### Emergency Contacts Template
```
Security Team Lead: [Contact Information]
System Administrator: [Contact Information]
Legal/Compliance: [Contact Information]
External Security Consultant: [Contact Information]
```

## 📚 Security Resources and References

- [OWASP Application Security Verification Standard](https://owasp.org/www-project-application-security-verification-standard/)
- [Microsoft Security Development Lifecycle](https://www.microsoft.com/en-us/securityengineering/sdl)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)
- [Azure Security Baseline](https://docs.microsoft.com/security/benchmark/azure/)
- [CIS Controls](https://www.cisecurity.org/controls/)