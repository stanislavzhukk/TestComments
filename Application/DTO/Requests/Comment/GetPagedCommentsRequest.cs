using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTO.Requests.Comment
{
    public sealed record GetPagedCommentsRequest(
        int PageNumber,
        int PageSize
    );
}
