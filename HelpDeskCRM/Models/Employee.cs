using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }


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


        [Required(ErrorMessage = "Department is required.")]
        public string Department { get; set; } = string.Empty;


        [Required(ErrorMessage = "Designation is required.")]
        public string Designation { get; set; } = string.Empty;


        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;


        public string Status { get; set; } = "Active";


        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}