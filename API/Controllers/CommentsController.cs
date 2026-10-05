using API.Extensions;
using Application.DTO.Requests.Comments.Create;
using Application.DTO.Requests.Comments.Get;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController(ICommentService commentService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetComments([FromQuery] GetPagedCommentsRequest request, CancellationToken ct){
            var result = await commentService.GetCommentsAsync(request, ct);

            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToActionResult();
        }



        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentForm form, CancellationToken ct){
            var request = form.ToRequest();

            var clientInfo = new ClientInfoRequest(
                Ip: HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent: Request.Headers["User-Agent"].ToString());

            var result = await commentService.CreateCommentAsync(request, clientInfo, ct);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetComment), new { id = result.Value!.Id }, result.Value)
                : result.ToActionResult();
        }
         
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComment([FromRoute] Guid id, CancellationToken ct){
            var result = await commentService.GetCommentAsync(id, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToActionResult();
        }
    }
}
