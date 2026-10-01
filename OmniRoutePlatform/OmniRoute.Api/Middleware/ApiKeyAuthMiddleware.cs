using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OmniRoute.Api.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-API-KEY";

        // UpCode Production API Key
        private const string UPCODE_API_KEY = "upcode_live_sec_key_2026_99a7b";

        public ApiKeyAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Allow the Android Gateway Polling endpoint to bypass API key check
            if (context.Request.Path.Value != null && context.Request.Path.Value.Contains("android-poll"))
            {
                await _next(context);
                return;
            }

            // Enforce API Key on all other endpoints (including batch-send)
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: X-API-KEY header is missing.");
                return;
            }

            if (!UPCODE_API_KEY.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid API Key provided.");
                return;
            }

            await _next(context);
        }
    }
}