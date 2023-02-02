using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Core.Models.Users
{
    public class LoginDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "Your password is limited to {2} to {1} characters", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
