using Newtonsoft.Json;
using System.Net;

namespace Cuttr.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Proceed to the next middleware/component in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception with Serilog
                _logger.LogError(ex, "An unhandled exception occurred while processing the request. Correlation ID: {CorrelationId}", context.TraceIdentifier);

                // Handle the exception and generate a response
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(
            HttpContext context, Exception exception)
        {
            // Set the response status code and content type
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            // Create a standardized error response
            var errorResponse = new
            {
                message = "An unexpected error occurred.",
                correlationId = context.TraceIdentifier
                // Optionally include exception details in development environment
            };

            var errorJson = JsonConvert.SerializeObject(errorResponse);

            // Write the error response to the HTTP response
            return context.Response.WriteAsync(errorJson);
        }
    }
}
