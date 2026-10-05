using Application.DTO.Requests.Captcha;

namespace Application.DTO.Requests.Comments.Create
{
    public sealed record CreateCommentRequest(
        string UserName,
        string UserEmail,
        string? HomePageUrl,
        string Content,
        CheckCaptchaRequest Captcha,
        Guid? ParentCommentId = null,
        FileUploadRequest? File = null
    );
}
