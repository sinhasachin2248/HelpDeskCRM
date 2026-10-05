using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class CustomerRegisterViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email is required.")]
        [StrictEmail]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(
            @"^[6-9][0-9]{9}$",
            ErrorMessage = "Phone number must be exactly 10 digits and start with 6-9."
        )]
        public string Phone { get; set; } = string.Empty;


        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;


        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            "Password",
            ErrorMessage = "Passwords do not match."
        )]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}