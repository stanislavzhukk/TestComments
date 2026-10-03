using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Application.DTO.Requests.Comment
{
    public sealed record GetPagedCommentsRequest(
        int PageNumber = 1,
        int PageSize = 25,
        string SortBy = "createdAt",
        bool Desc = true
    );
}
