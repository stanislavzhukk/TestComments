using API.Extensions;
using Application.DTO.Requests.Comments;
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
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request, CancellationToken ct){
            var result = await commentService.CreateCommentAsync(request, ct);
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
