using Application.DTO.Requests.Captcha;

namespace Application.DTO.Requests.Comments
{
    public sealed record CreateCommentRequest(
        string UserName,
        string UserEmail,
        string? HomePageUrl,
        string Content,
        CheckCaptchaRequest Captcha,
        Guid? ParentCommentId = null
    );
}
