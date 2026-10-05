using Application.DTO.Requests.Captcha;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Requests.Comments.Create
{
    public sealed class CreateCommentForm
    {
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public string? HomePageUrl { get; set; }
        public string Content { get; set; } = "";
        public Guid? ParentCommentId { get; set; }
        public CheckCaptchaRequest? Captcha { get; set; }
        public IFormFile? File { get; set; }

        public CreateCommentRequest ToRequest()
        {
            return new(UserName, UserEmail, HomePageUrl, Content, Captcha!, ParentCommentId, ToFileRequest());
        }
            
        public FileUploadRequest? ToFileRequest()
        {
            return File is null ? null : new FileUploadRequest(File.OpenReadStream(), File.FileName, File.Length);
        }           
    }
}
