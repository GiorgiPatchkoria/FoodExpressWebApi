using FoodExpress.Services.Services.Implementations;
using FoodExpress.Responses;

namespace FoodExpress.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                ServiceException { Error: ServiceError.Unauthorized } => StatusCodes.Status401Unauthorized,
                ServiceException { Error: ServiceError.Forbidden } => StatusCodes.Status403Forbidden,
                ServiceException { Error: ServiceError.Validation } => StatusCodes.Status400BadRequest,
                ServiceException { Error: ServiceError.NotFound } => StatusCodes.Status404NotFound,
                ServiceException { Error: ServiceError.Conflict } => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
            }

            var message = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred." : exception.Message;

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = (exception as ServiceException)?.Errors
            };

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response, cancellationToken: context.RequestAborted);
        }
    }
}
