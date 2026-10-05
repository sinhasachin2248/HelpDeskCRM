using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class Tickets
    {
        [Key]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer is required.")]
        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public int? EmployeeId { get; set; }

        public string AssignedEmployee { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required.")]
        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public string Category { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}