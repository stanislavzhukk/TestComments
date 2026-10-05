using Domain.Enums;

namespace Application.DTO.Responses
{
    public sealed record StoredFileResponse(
    AttachmentType Type,
    string StoredFileName,
    string ContentType,
    long SizeBytes);
}
