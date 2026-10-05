using HelpDeskCRM.API.Models;
using HelpDeskCRMAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================================================
        // DATABASE TABLES
        // =========================================================

        public DbSet<Employee> Employees { get; set; } = null!;

        public DbSet<EmployeeActionAttempt> EmployeeActionAttempts
        {
            get;
            set;
        } = null!;

        public DbSet<EmployeeMobileApiLog> EmployeeMobileApiLogs
        {
            get;
            set;
        } = null!;

        public DbSet<ApiLog> ApiLogs { get; set; } = null!;


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =========================================================
            // EMPLOYEES
            // =========================================================

            modelBuilder.Entity<Employee>()
                .ToTable("employees");

            modelBuilder.Entity<Employee>()
                .Property(x => x.EmployeeId)
                .HasColumnName("employee_id");

            modelBuilder.Entity<Employee>()
                .Property(x => x.FirstName)
                .HasColumnName("first_name");

            modelBuilder.Entity<Employee>()
                .Property(x => x.LastName)
                .HasColumnName("last_name");

            modelBuilder.Entity<Employee>()
                .Property(x => x.Email)
                .HasColumnName("email");

            modelBuilder.Entity<Employee>()
                .Property(x => x.Phone)
                .HasColumnName("phone");

            modelBuilder.Entity<Employee>()
                .Property(x => x.Department)
                .HasColumnName("department");

            modelBuilder.Entity<Employee>()
                .Property(x => x.Designation)
                .HasColumnName("designation");

            modelBuilder.Entity<Employee>()
                .Property(x => x.City)
                .HasColumnName("city");

            modelBuilder.Entity<Employee>()
                .Property(x => x.Status)
                .HasColumnName("status");

            modelBuilder.Entity<Employee>()
                .Property(x => x.LastUpdated)
                .HasColumnName("last_updated");


            // =========================================================
            // EMPLOYEE ACTION ATTEMPTS
            // =========================================================

            modelBuilder.Entity<EmployeeActionAttempt>()
                .ToTable("employee_action_attempts");

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.Id)
                .HasColumnName("id");

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.EmployeeId)
                .HasColumnName("employee_id");

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.ActionName)
                .HasColumnName("action_name")
                .HasMaxLength(100);

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.AttemptCount)
                .HasColumnName("attempt_count");

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.InsertedOn)
                .HasColumnName("inserted_on")
                .HasColumnType("datetime(0)");

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.UpdatedOn)
                .HasColumnName("updated_on")
                .HasColumnType("datetime(0)");

            modelBuilder.Entity<EmployeeActionAttempt>()
                .Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("int");


            // =========================================================
            // UNIQUE EMPLOYEE + ACTION
            // =========================================================

            modelBuilder.Entity<EmployeeActionAttempt>()
                .HasIndex(x => new
                {
                    x.EmployeeId,
                    x.ActionName
                })
                .IsUnique();


            // =========================================================
            // API LOGS
            // =========================================================

            modelBuilder.Entity<ApiLog>()
                .ToTable("api_logs");

            modelBuilder.Entity<ApiLog>()
                .Property(x => x.Id)
                .HasColumnName("id");

            modelBuilder.Entity<ApiLog>()
                .Property(x => x.ProcessName)
                .HasColumnName("process_name");

            modelBuilder.Entity<ApiLog>()
                .Property(x => x.Method)
                .HasColumnName("method");

            modelBuilder.Entity<ApiLog>()
                .Property(x => x.LoggedAt)
                .HasColumnName("date_time")
                .HasColumnType("datetime(0)");

            modelBuilder.Entity<ApiLog>()
                .Property(x => x.Status)
                .HasColumnName("status");

            modelBuilder.Entity<ApiLog>()
                .Property(x => x.ResponseValue)
                .HasColumnName("response_value")
                .HasMaxLength(20);


            // =========================================================
            // EMPLOYEE MOBILE API LOGS
            // =========================================================

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .ToTable("employee_mobile_api_logs");

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .Property(x => x.Id)
                .HasColumnName("id");

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .Property(x => x.MobileNo)
                .HasColumnName("mobile_no")
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .Property(x => x.EmployeeName)
                .HasColumnName("employee_name")
                .HasMaxLength(150);

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .Property(x => x.ResponseMessage)
                .HasColumnName("response_message");

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .Property(x => x.ResponseStatus)
                .HasColumnName("response_status")
                .HasMaxLength(50);

            modelBuilder.Entity<EmployeeMobileApiLog>()
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime(0)");
        }
    }
}