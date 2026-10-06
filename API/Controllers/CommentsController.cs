using API.Extensions;
using Application.DTO.Requests.Comments.Create;
using Application.DTO.Requests.Comments.Get;
using Application.DTO.Responses;
using Application.Interfaces;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// API controller for managing comments.
    /// Provides endpoints for retrieving, creating, and managing user comments with support for nested replies and file attachments.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController(ICommentService commentService) : ControllerBase
    {     
        [HttpGet]
        [EndpointSummary("Retrieves a paginated list of comments")]
        [ProducesResponseType<PagedResult<CommentResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
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
        [EndpointSummary("Creates a new comment with optional file attachment.")]
        [ProducesResponseType<Result<CommentResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
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
        [EndpointSummary("Retrieves a specific comment by its ID, including nested replies and attachments.")]
        [ProducesResponseType<Result<CommentResponse>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetComment([FromRoute] Guid id, CancellationToken ct){
            var result = await commentService.GetCommentAsync(id, ct);
            return result.IsSuccess
                ? Ok(result.Value)
                : result.ToActionResult();
        }
    }
}
