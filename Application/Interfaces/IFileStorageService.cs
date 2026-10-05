using Application.DTO.Requests.Comments.Create;
using Application.DTO.Responses;
using Domain.Common;

namespace Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<Result<StoredFileResponse>> SaveAsync(FileUploadRequest file, CancellationToken ct);
        void Delete(string storedFileName);
    }
}
