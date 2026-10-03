using Application.DTO.Requests.Captcha;
using Application.DTO.Responses;
using Domain.Common;

namespace Application.Interfaces
{
    public interface ICaptchaService
    {
        Result<CaptchaResponse> GenerateCaptcha();
        Result ValidateCaptcha(ValidateCaptchaRequest request);
    }
}
