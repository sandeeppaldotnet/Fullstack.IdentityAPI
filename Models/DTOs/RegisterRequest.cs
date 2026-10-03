using System.ComponentModel.DataAnnotations;

namespace Fullstack.IdentityAPI.Models.DTOs
{
    public class RegisterRequest
    {

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
     
        public string AdharNumber { get; set; } = string.Empty;


    }
}
