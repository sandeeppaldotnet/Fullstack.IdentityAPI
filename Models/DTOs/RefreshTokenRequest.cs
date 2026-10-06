using System.ComponentModel.DataAnnotations;

namespace Fullstack.IdentityAPI.Models.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } =
       string.Empty;

    }
}
