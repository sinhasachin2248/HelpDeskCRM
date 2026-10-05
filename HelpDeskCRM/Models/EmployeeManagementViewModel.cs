namespace HelpDeskCRM.Models
{
    public class EmployeeManagementViewModel
    {
        public Employee NewEmployee { get; set; } = new Employee();

        public Employee? SelectedEmployee { get; set; }

        public List<Employee> Employees { get; set; } = new();

        public List<Tickets> AssignedTasks { get; set; } = new();

        public int? SearchEmployeeId { get; set; }

        public string ActiveSection { get; set; } = "addEmployee";

        public string Message { get; set; } = string.Empty;


        // =====================================================
        // MOBILE API RESPONSE
        // =====================================================

        public MobileApiResponseViewModel? MobileApiResponse
        {
            get;
            set;
        }
    }


    // =========================================================
    // MOBILE API RESPONSE
    // =========================================================

    public class MobileApiResponseViewModel
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

   
        public string ErrorMessages { get; set; } = "null";

        public string Exception { get; set; } = "null";

        public string Data { get; set; } = "null";

        public string PersonName { get; set; } = "null";

        public bool HasEmployeeData { get; set; }

        public MobileEmployeeResponseViewModel? Employee
        {
            get;
            set;
        }

        public string ResponseStatus { get; set; } =
            string.Empty;

        public DateTime? SavedAt { get; set; }
    }


    // =========================================================
    // EMPLOYEE DETAILS RETURNED BY MOBILE API
    // =========================================================

    public class MobileEmployeeResponseViewModel
    {
        public string MobileNo { get; set; } =
            string.Empty;

        public string Name { get; set; } =
            string.Empty;
    }
}