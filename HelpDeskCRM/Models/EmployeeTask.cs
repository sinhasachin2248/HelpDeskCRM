namespace HelpDeskCRM.Models
{
    public class EmployeeTask
    {
        public int TaskId { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int EmployeeId { get; set; }
    }
}