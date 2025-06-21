using System.Security.Authentication;
using System.Text.Json;
using AuthenticationApp.Domain.Exceptions;
using AuthenticationApp.Domain.Response;

namespace AuthenticationApp.Middleware
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

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception in pipeline");
            context.Response.ContentType = "application/json";
            ApiResponse<object> apiResponse;
            switch (exception)
            {
                case NotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    apiResponse = ApiResponse<object>.CreateFailure(exception.Message);
                    break;
                case ValidationException ve:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse = ApiResponse<object>.CreateFailure(exception.Message, ve.ValidationErrors);
                    break;
                case InvalidCredentialException ics:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    apiResponse = ApiResponse<object>.CreateFailure(ics.Message);
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse = ApiResponse<object>.CreateFailure("Ocorreu um erro inesperado.", new[] { exception.Message });
                    break;
            }

            var json = JsonSerializer.Serialize(apiResponse);
            return context.Response.WriteAsync(json);
        }
    }
}
