namespace Cuttr.Api.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(
            RequestDelegate next,
            ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Log the request
            await LogRequest(context);

            // Copy the original response body stream
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                // Replace the response body with a memory stream
                context.Response.Body = responseBody;

                // Proceed to the next middleware/component
                await _next(context);

                // Log the response
                await LogResponse(context);

                // Copy the response back to the original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            // Read the request body
            var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();

            // Reset the request body stream position
            context.Request.Body.Position = 0;

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
