using API.Extensions;
using Application.DTO.Responses;
using Application.Interfaces;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaptchaController(ICaptchaService captchaService) : ControllerBase
    {
        [HttpGet("generate")]
        [EndpointSummary("Generates a new CAPTCHA challenge.")]
        [ProducesResponseType<Result<CaptchaResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public IActionResult GenerateCaptcha()
        {
            var result = captchaService.GenerateCaptcha();
            return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
        }
    }
}
