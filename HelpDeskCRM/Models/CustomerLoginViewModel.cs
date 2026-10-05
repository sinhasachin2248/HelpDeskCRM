using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class CustomerLoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [StrictEmail]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}