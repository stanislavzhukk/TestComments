using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO.Requests.Captcha
{
    public sealed record CheckCaptchaRequest(
        Guid CaptchaId,
        string UserInput);
}
