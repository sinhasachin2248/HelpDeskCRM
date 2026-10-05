using System.ComponentModel.DataAnnotations;

namespace HelpDeskCRM.Models
{
    public class CustomerAccount
    {
        [Key]
        public int CustomerAccountId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Customer? Customer { get; set; }
    }
}