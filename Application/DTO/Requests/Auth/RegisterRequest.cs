namespace Application.DTO.Requests.Auth
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public string? Surname { get; set; }
        public required string Password { get; set; }
    }
}
