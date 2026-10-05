namespace HelpDeskCRM.API.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; }
    }
}