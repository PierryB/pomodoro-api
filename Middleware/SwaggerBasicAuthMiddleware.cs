using System.Net;
using System.Text;

namespace pomodoro_api.Middleware;

public class SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly RequestDelegate _next = next;
    private readonly IConfiguration _configuration = configuration;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            var username = _configuration["Swagger:Username"];
            var password = _configuration["Swagger:Password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Swagger credentials not configured");
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
            {
                Challenge(context);
                return;
            }

            var authHeader = value.ToString();
            if (!authHeader.StartsWith("Basic "))
            {
                Challenge(context);
                return;
            }

            var encodedCredentials = authHeader["Basic ".Length..].Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);

            if (parts.Length != 2 ||
                parts[0] != username ||
                parts[1] != password)
            {
                Challenge(context);
                return;
            }
        }

        await _next(context);
    }

    private static void Challenge(HttpContext context)
    {
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\"";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }
}
