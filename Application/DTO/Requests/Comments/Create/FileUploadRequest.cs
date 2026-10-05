namespace Application.DTO.Requests.Comments.Create
{
    public sealed record FileUploadRequest(Stream Content, string FileName, long Length);
}
