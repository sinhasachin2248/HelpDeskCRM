using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace HelpDeskCRM.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;


        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
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


        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string CompanyName { get; set; } = string.Empty;


        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;


        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }


    // =====================================================
    // STRICT EMAIL VALIDATION
    // =====================================================

    public class StrictEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult(
                    "Email is required."
                );
            }

            string email = value.ToString()!.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                return new ValidationResult(
                    "Email is required."
                );
            }


            // Basic structure
            if (email.Contains(" "))
            {
                return new ValidationResult(
                    "Email cannot contain spaces."
                );
            }


            // Exactly one @
            if (email.Count(c => c == '@') != 1)
            {
                return new ValidationResult(
                    "Please enter a valid email address."
                );
            }


            string[] parts = email.Split('@');

            string username = parts[0];
            string domain = parts[1];


            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(domain))
            {
                return new ValidationResult(
                    "Please enter a valid email address."
                );
            }


            // Domain must contain a dot
            if (!domain.Contains('.'))
            {
                return new ValidationResult(
                    "Email must contain a valid domain, e.g. gmail.com."
                );
            }


            // No consecutive dots
            if (email.Contains(".."))
            {
                return new ValidationResult(
                    "Email cannot contain consecutive dots."
                );
            }


            // Username cannot start/end with dot
            if (username.StartsWith('.') ||
                username.EndsWith('.'))
            {
                return new ValidationResult(
                    "Please enter a valid email address."
                );
            }


            // Domain cannot start/end with dot
            if (domain.StartsWith('.') ||
                domain.EndsWith('.'))
            {
                return new ValidationResult(
                    "Please enter a valid email address."
                );
            }


            // Strict email format
            string pattern =
                @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@" +
                @"[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)+$";


            if (!Regex.IsMatch(email, pattern))
            {
                return new ValidationResult(
                    "Please enter a valid email address."
                );
            }


            // TLD must have at least 2 letters
            string tld = domain.Split('.').Last();

            if (tld.Length < 2 ||
                !Regex.IsMatch(tld, @"^[a-zA-Z]+$"))
            {
                return new ValidationResult(
                    "Please enter a valid email domain."
                );
            }


            return ValidationResult.Success;
        }
    }
}