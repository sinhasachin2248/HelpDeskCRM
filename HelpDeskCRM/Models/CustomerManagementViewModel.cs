using System.Collections.Generic;

namespace HelpDeskCRM.Models
{
    public class CustomerManagementViewModel
    {
        public Customer NewCustomer { get; set; } = new Customer();

        public Customer? SelectedCustomer { get; set; }

        public List<Customer> Customers { get; set; } = new();

        public List<Tickets> CustomerTickets { get; set; } = new();

        public int? SearchCustomerId { get; set; }

        public string ActiveSection { get; set; } = "addCustomer";

        public string Message { get; set; } = string.Empty;
    }
}