namespace Application.DTO.Responses
{
    public sealed record CaptchaResponse(
        string Image,
        Guid Id
    );
}
