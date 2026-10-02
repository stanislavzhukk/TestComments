using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO.Requests.Comment
{
    public sealed record CreateCommentRequest(
        string UserName,
        string UserEmail,
        string? HomePageUrl,
        string Content,
        Guid? ParentCommentId = null
    );
}
