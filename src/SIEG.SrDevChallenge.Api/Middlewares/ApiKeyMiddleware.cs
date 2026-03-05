using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SIEG.SrDevChallenge.Api.Middlewares;

public class ApiKeyMiddleware(IOptions<ApiKeySettings> apiKeySettings, ILogger<ApiKeyMiddleware> logger) : IMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ApiKeyQueryParam = "apikey";
    
    private readonly ApiKeySettings _apiKeySettings = apiKeySettings.Value;
    private readonly ILogger<ApiKeyMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        
        if (IsExcludedPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var providedApiKey = GetApiKeyFromRequest(context.Request);

        if (string.IsNullOrEmpty(providedApiKey))
        {
            _logger.LogWarning("API Key não fornecida para {Path} de {RemoteIpAddress}", 
                context.Request.Path, context.Connection.RemoteIpAddress);
            
            await WriteUnauthorizedResponse(context, "API Key é obrigatória");
            return;
        }

        if (!IsValidApiKey(providedApiKey))
        {
            _logger.LogWarning("API Key inválida fornecida: {ApiKey} para {Path} de {RemoteIpAddress}", 
                providedApiKey, context.Request.Path, context.Connection.RemoteIpAddress);
                
            await WriteUnauthorizedResponse(context, "API Key inválida");
            return;
        }

        _logger.LogInformation("Acesso autorizado com API Key para {Path}", context.Request.Path);
        
        context.Items["ApiKey"] = providedApiKey;
        context.Items["ApiKeyName"] = GetApiKeyName(providedApiKey);
        
        await next(context);
    }

    private string? GetApiKeyFromRequest(HttpRequest request)
    {
        
        if (request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }
        return null;
    }

    private bool IsValidApiKey(string apiKey)
    {
        if (_apiKeySettings?.ValidApiKeys == null)
            return false;

        return _apiKeySettings.ValidApiKeys.Any(x => 
            string.Equals(x.Key, apiKey, StringComparison.Ordinal));
    }

    private string? GetApiKeyName(string apiKey)
    {
        return _apiKeySettings?.ValidApiKeys?
            .FirstOrDefault(x => string.Equals(x.Key, apiKey, StringComparison.Ordinal))?.Name;
    }

    private bool IsExcludedPath(PathString path)
    {
        var excludedPaths = new[]
        {
            "/openapi",
            "/health",
            "/swagger",
            "/favicon.ico"
        };

        return excludedPaths.Any(excluded => 
            path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
    }

    private async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";
        
        ProblemDetails response = new()
        {
            Title = "Unauthorized",
            Detail = message,
            Status = 401,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

public class ApiKeySettings
{
    public List<ApiKeyInfo> ValidApiKeys { get; set; } = new();
}

public class ApiKeyInfo
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}