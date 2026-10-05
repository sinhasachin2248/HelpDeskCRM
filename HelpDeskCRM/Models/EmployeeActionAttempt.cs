using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskCRM.Models
{
    [Table("employee_action_attempts")]
    public class EmployeeActionAttempt
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("action_name")]
        public string ActionName { get; set; }
            = string.Empty;

        [Column("attempt_count")]
        public int AttemptCount { get; set; }

        [Column("inserted_on")]
        public DateTime InsertedOn { get; set; }

        [Column("updated_on")]
        public DateTime UpdatedOn { get; set; }

        [Column("status")]
        public int Status { get; set; }
    }
}