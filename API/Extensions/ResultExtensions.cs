using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                throw new InvalidOperationException("Cannot convert a successful result to a problem.");

            var (statusCode, title) = result.Error!.Type switch
            {
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = result.Error.Message
            };
            problem.Extensions["errorCode"] = result.Error.Code;

            return new ObjectResult(problem) { StatusCode = statusCode };
        }
    }
}
