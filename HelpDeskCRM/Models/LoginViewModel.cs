using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "User ID is required.")]
        public string Username { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;


        public bool RememberMe { get; set; }
    }
}