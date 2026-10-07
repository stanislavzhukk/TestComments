using Application.DTO.Requests.Captcha;
using Application.DTO.Requests.Comments.Create;
using Application.Interfaces;
using Application.Options;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Captcha options
        services.AddOptions<CaptchaOptions>()
            .Bind(configuration.GetSection(CaptchaOptions.SectionName))
            .Validate(o => o.CodeLength is >= 3 and <= 10, "Captcha:CodeLength must be 3-10")
            .Validate(o => o.TtlMinutes > 0, "Captcha:TtlMinutes must be positive")
            .Validate(o => o.Width > 0 && o.Height > 0, "Captcha size must be positive")
            .ValidateOnStart();

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateCommentRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CheckCaptchaRequestValidator>();

        // Register application services
        services.AddScoped<ICommentService, CommentService>();

        return services;
    }
}
