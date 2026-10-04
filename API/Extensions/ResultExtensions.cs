using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Cannot convert a successful result to a problem.");

            var error = result.Error!;

            var (statusCode, title) = error.Type switch
            {
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            ProblemDetails problem = error.Fields is { Count: > 0 }
                ? new ValidationProblemDetails(
                    error.Fields.ToDictionary(f => ToCamelCase(f.Key), f => f.Value))
                : new ProblemDetails();

            problem.Status = statusCode;
            problem.Title = title;
            problem.Detail = error.Message;
            problem.Extensions["errorCode"] = error.Code;

            return new ObjectResult(problem) { StatusCode = statusCode };
        }

        private static string ToCamelCase(string path)
        {
            return string.Join('.', path.Split('.').Select(s => char.ToLowerInvariant(s[0]) + s[1..]));
        }
    }
}
