using Domain.Common;

namespace Application.Interfaces
{
    public interface ICommentContentSanitizer
    {
        Result<string> Sanitize(string input);
    }
}
