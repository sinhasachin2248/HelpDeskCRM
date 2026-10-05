using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.API.Models
{
    public class EmployeeActionAttempt
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string ActionName { get; set; } = string.Empty;

        public int AttemptCount { get; set; }

        public DateTime InsertedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public int Status { get; set; }
    }
}