using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO.Requests.Captcha
{
    public sealed record ValidateCaptchaRequest(
        Guid CaptchaId,
        string UserInput);
}
