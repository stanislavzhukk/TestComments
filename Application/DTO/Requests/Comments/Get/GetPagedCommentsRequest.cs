namespace Application.DTO.Requests.Comments.Get
{
    public sealed record GetPagedCommentsRequest(
        int PageNumber = 1,
        int PageSize = 25,
        string SortBy = "createdAt",
        bool Desc = true
    );
}
