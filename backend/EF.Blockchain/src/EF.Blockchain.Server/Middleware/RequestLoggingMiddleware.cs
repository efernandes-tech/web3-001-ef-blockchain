using Serilog;

namespace EF.Blockchain.Server.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path;

        context.Request.EnableBuffering();
        string body = "";

        if (context.Request.ContentLength > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var start = DateTime.Now;
        await _next(context);
        var duration = DateTime.Now - start;

        Log.Information("{Method} {Path} => {StatusCode} ({Duration} ms) | Body: {Body}",
            method, path, context.Response.StatusCode, duration.TotalMilliseconds, body);
    }
}
