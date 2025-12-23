namespace Sahty.Shared.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();

        public AuthResponseDto(string token, DateTimeOffset expiresAt, string userId, string email, string[] roles)
        {
            Token = token;
            ExpiresAt = expiresAt;
            UserId = userId;
            Email = email;
            Roles = roles;
        }

        public AuthResponseDto() { }
    }
}
