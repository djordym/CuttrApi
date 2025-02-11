using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Cuttr.Api.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        private const string LoggingMiddlewareInvoked = "LoggingMiddlewareInvoked";

        public LoggingMiddleware(
            RequestDelegate next,
            ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Check if the middleware has already been invoked for this request
            if (!context.Items.ContainsKey(LoggingMiddlewareInvoked))
            {
                // Set the flag to indicate the middleware has been invoked
                context.Items[LoggingMiddlewareInvoked] = true;

                // Log the request
                await LogRequest(context);

                // Copy the original response body stream
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    // Replace the response body with a memory stream
                    context.Response.Body = responseBody;

                    try
                    {
                        // Proceed to the next middleware/component
                        await _next(context);
                    }
                    finally
                    {
                        // Log the response
                        await LogResponse(context);

                        // Copy the response back to the original stream
                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                        context.Response.Body = originalBodyStream;
                    }
                }
            }
            else
            {
                // Middleware has already been invoked; proceed without logging
                await _next(context);
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            // Read the request body
            var bodyAsText = string.Empty;
            if (context.Request.ContentLength > 0 &&
                context.Request.Body.CanSeek)
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: System.Text.Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 8192,
                    leaveOpen: true))
                {
                    bodyAsText = await reader.ReadToEndAsync();
                }
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            // Log relevant request information
            _logger.LogInformation("HTTP Request Information: {Method} {Path} {QueryString} {Body}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString,
                bodyAsText);
        }

        private async Task LogResponse(HttpContext context)
        {
            // Reset the response body stream position
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Read the response body
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();

            // Reset the response body stream position
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Log relevant response information
            _logger.LogInformation("HTTP Response Information: {StatusCode} {Body}",
                context.Response.StatusCode,
                text);
        }
    }
}
