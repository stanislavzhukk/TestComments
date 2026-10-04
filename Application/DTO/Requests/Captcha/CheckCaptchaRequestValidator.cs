using Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.DTO.Requests.Captcha
{
    public class CheckCaptchaRequestValidator : AbstractValidator<CheckCaptchaRequest>
    {
        public CheckCaptchaRequestValidator(IOptions<CaptchaOptions> options)
        {
            RuleFor(x => x.CaptchaId).NotEmpty();

            RuleFor(x => x.UserInput)
                .NotEmpty()
                .Length(options.Value.CodeLength).WithMessage($"Captcha code must be exactly {options.Value.CodeLength} characters long")
                .Matches("^[a-zA-Z0-9]+$").WithMessage("Only Latin letters and digits are allowed");
        }
    }
}
