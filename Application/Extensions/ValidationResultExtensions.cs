using Domain.Common;
using FluentValidation.Results;

namespace Application.Extensions
{
    public static class ValidationResultExtensions
    {
        public static Error ToError(this ValidationResult validation) =>
            Error.Validation(
                "Validation.Failed",
                "One or more fields are invalid",
                validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
    }
}
