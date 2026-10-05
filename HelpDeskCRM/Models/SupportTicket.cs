namespace HelpDeskCRM.Models
{
    public class SupportTicket
    {
        public int TicketId { get; set; }

        public int CustomerId { get; set; }

        public int? EmployeeId { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}