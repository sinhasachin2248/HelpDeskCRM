namespace HelpDeskCRM.Models
{
    public class AdminManagementViewModel
    {
        public List<Admin> Admins { get; set; }
            = new List<Admin>();

        public Admin? SelectedAdmin { get; set; }

        public string? Message { get; set; }
    }
}