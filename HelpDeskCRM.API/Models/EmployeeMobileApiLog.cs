namespace HelpDeskCRMAPI.Models
{
    public class EmployeeMobileApiLog
    {
        public int Id { get; set; }

        public string MobileNo { get; set; } = string.Empty;

        public string? EmployeeName { get; set; }

        public string? ResponseMessage { get; set; }

        public string? ResponseStatus { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}