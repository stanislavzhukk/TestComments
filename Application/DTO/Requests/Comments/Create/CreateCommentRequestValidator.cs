using Application.DTO.Requests.Captcha;
using Application.Options;
using Domain.Constants;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.DTO.Requests.Comments.Create
{
    public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
    {
        public CreateCommentRequestValidator(IValidator<CheckCaptchaRequest> captchaValidator)
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(CommentLimits.UserNameMax).WithMessage($"User name must not exceed {CommentLimits.UserNameMax} characters")
                .Matches("^[a-zA-Z0-9]+$").WithMessage("Only Latin letters and digits are allowed");

            RuleFor(x => x.UserEmail)
                .NotEmpty()
                .MaximumLength(CommentLimits.EmailMax).WithMessage($"Email must not exceed {CommentLimits.EmailMax} characters")
                .EmailAddress();

            RuleFor(x => x.HomePageUrl)
                .MaximumLength(CommentLimits.HomePageMax)
                .Must(BeHttpUrl).WithMessage("Must be a valid http/https URL")
                .When(x => !string.IsNullOrWhiteSpace(x.HomePageUrl));

            RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(CommentLimits.ContentMax).WithMessage($"Content must not exceed {CommentLimits.ContentMax} characters");

            RuleFor(x => x.Captcha)
                .NotNull()
                .SetValidator(captchaValidator);
        }

        private bool BeHttpUrl(string? value){
            return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
