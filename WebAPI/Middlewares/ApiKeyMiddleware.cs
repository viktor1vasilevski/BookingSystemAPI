namespace WebAPI.Middlewares;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _apiKey = configuration["ApiKey"];
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var extractedApiKey) ||
            extractedApiKey != _apiKey)
        {
            _logger.LogWarning("Unauthorized request. Invalid or missing API key.");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized. Invalid API Key.");
            return;
        }

        await _next(context);
    }
}
