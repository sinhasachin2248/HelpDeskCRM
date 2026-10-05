using System.Net.Sockets;

namespace HelpDeskCRM.Models
{
    public static class CRMDataStore
    {
        public static List<Customer> Customers { get; set; }
            = new List<Customer>();

        public static List<Employee> Employees { get; set; }
            = new List<Employee>();

        public static List<Tickets> Tickets { get; set; }
            = new List<Tickets>();

        public static List<Admin> Admins { get; set; }
            = new List<Admin>();
    }
}