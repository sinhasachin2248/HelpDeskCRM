namespace HelpDeskCRM.API.Models
{
    public class ApiLog
    {
        public int Id { get; set; }

        public string ProcessName { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public DateTime LoggedAt { get; set; }

        public int Status { get; set; }

        public string ResponseValue { get; set; } = string.Empty;
    }
}