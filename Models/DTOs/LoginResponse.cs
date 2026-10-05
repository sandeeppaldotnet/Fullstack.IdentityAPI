namespace Fullstack.IdentityAPI.Models.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string TokenType { get; set; } = "Bearer";

        public int ExpiresIn { get; set; }

        public string RefreshToken { get; set; } = string.Empty;

        public UserResponse User { get; set; } = new();

    }
}
