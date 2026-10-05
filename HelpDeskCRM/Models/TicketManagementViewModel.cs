using System.Collections.Generic;

namespace HelpDeskCRM.Models
{
    public class TicketManagementViewModel
    {
        public Tickets NewTicket { get; set; } = new Tickets();

        public Tickets? SelectedTicket { get; set; }

        public List<Tickets> Tickets { get; set; } = new();

        public List<Customer> Customers { get; set; } = new();

        public List<Employee> Employees { get; set; } = new();

        public string ActiveSection { get; set; } = "ticketList";

        public string Message { get; set; } = string.Empty;
    }
}