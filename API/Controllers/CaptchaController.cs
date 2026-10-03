using API.Extensions;
using Application.DTO.Requests.Captcha;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaptchaController(ICaptchaService captchaService) : ControllerBase
    {
        [HttpGet("generate")]
        public IActionResult GenerateCaptcha()
        {
            var result = captchaService.GenerateCaptcha();
            return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
        }
    }
}
