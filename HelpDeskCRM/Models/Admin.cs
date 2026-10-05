using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpDeskCRM.Models
{
    public class Admin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminId { get; set; }

        [Required]
        [StringLength(100)]
        public string AdminName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // 1 = Active
        // 0 = Not Active
        [Required]
        public int Status { get; set; } = 1;

        public DateTime CreatedAt { get; set; }
    }
}

/*
         SELECT
    employee_id,
    first_name,
    last_name,
    hire_date,
    salary
FROM employees
WHERE MONTH(hire_date) IN (10, 11, 12)
  AND YEAR(hire_date) % 4 <> 0
  AND DAY(hire_date) % 2 <> 0
  AND DATENAME(WEEKDAY, hire_date) NOT IN ('Saturday', 'Sunday')
  AND salary > 70000
ORDER BY
    YEAR(hire_date) DESC,
    MONTH(hire_date),
    DAY(hire_date) DESC;*/