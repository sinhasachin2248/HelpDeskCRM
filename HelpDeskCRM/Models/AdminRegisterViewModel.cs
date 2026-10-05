using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class AdminRegisterViewModel
    {
        [Required(ErrorMessage = "Admin Name is required.")]
        [StringLength(
            100,
            MinimumLength = 2,
            ErrorMessage = "Admin Name must be between 2 and 100 characters."
        )]
        public string AdminName { get; set; } = string.Empty;


        [Required(ErrorMessage = "User ID is required.")]
        [StringLength(
            30,
            MinimumLength = 3,
            ErrorMessage = "User ID must be between 3 and 30 characters."
        )]
        public string UserId { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare(
            "Password",
            ErrorMessage = "Password and Confirm Password must match."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}