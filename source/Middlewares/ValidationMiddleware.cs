using FluentValidation;
using System.Text.Json;

namespace Api.Middlewares
{
    public sealed class ValidationMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception error)
            {
                await Handle(context, error);
            }
        }

        private static async Task Handle(HttpContext http, Exception exception)
        {
            var status = GetStatus(exception);

            var response = new
            {
                status = status,
                detail = exception.Message,
                errors = GetErrors(exception)
            };

            http.Response.ContentType = "application/json";

            http.Response.StatusCode = status;

            await http.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static int GetStatus(Exception exception) => exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        private static IDictionary<string, string> GetErrors(Exception exception)
        {
            var result = new Dictionary<string, string>();

            if (exception is ValidationException failures)
            {
                foreach (var error in failures.Errors)
                {
                    result.Add(error.PropertyName, error.ErrorMessage);
                }
            }

            return result;
        }
    }
}