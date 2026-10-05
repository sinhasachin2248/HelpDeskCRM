using System.Collections.Generic;

namespace HelpDeskCRM.Models
{
    public class CustomerPortalViewModel
    {
        public Customer? Customer { get; set; }

        public List<Tickets> Tickets { get; set; } = new();

        public Tickets NewTicket { get; set; } = new();

        public Tickets? SelectedTicket { get; set; }

        public string ActiveSection { get; set; } = "myTickets";

        public string Message { get; set; } = string.Empty;
    }
}