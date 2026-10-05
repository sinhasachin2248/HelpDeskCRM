using HelpDeskCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        // =====================================================
        // DATABASE TABLES
        // =====================================================

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Tickets> Tickets { get; set; }

        public DbSet<CustomerAccount> CustomerAccounts { get; set; }

        public DbSet<EmployeeActionAttempt>
            EmployeeActionAttempts
        { get; set; }


        // =====================================================
        // DATABASE NAMING CONVENTION
        // C# Property -> MySQL snake_case
        // =====================================================

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =================================================
            // ADMINS
            // =================================================

            modelBuilder.Entity<Admin>()
                .ToTable("admins");

            modelBuilder.Entity<Admin>()
                .Property(x => x.AdminId)
                .HasColumnName("admin_id");

            modelBuilder.Entity<Admin>()
                .Property(x => x.AdminName)
                .HasColumnName("admin_name");

            modelBuilder.Entity<Admin>()
                .Property(x => x.UserId)
                .HasColumnName("user_id");

            modelBuilder.Entity<Admin>()
                .Property(x => x.PasswordHash)
                .HasColumnName("password_hash");

            modelBuilder.Entity<Admin>()
                .Property(x => x.Status)
                .HasColumnName("status")
                .HasColumnType("int");

            modelBuilder.Entity<Admin>()
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at");


            // =================================================
            // CUSTOMERS
            // =================================================

            modelBuilder.Entity<Customer>()
                .ToTable("customers");

            modelBuilder.Entity<Customer>()
                .Property(x => x.CustomerId)
                .HasColumnName("customer_id");

            modelBuilder.Entity<Customer>()
                .Property(x => x.FirstName)
                .HasColumnName("first_name");

            modelBuilder.Entity<Customer>()
                .Property(x => x.LastName)
                .HasColumnName("last_name");

            modelBuilder.Entity<Customer>()
                .Property(x => x.Email)
                .HasColumnName("email");

            modelBuilder.Entity<Customer>()
                .Property(x => x.Phone)
                .HasColumnName("phone");

            modelBuilder.Entity<Customer>()
                .Property(x => x.CompanyName)
                .HasColumnName("company_name");

            modelBuilder.Entity<Customer>()
                .Property(x => x.City)
                .HasColumnName("city");

            modelBuilder.Entity<Customer>()
                .Property(x => x.LastUpdated)
                .HasColumnName("last_updated");


            // =================================================
            // EMPLOYEES
            // =================================================

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


            // =================================================
            // TICKETS
            // =================================================

            modelBuilder.Entity<Tickets>()
                .ToTable("tickets");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.TicketId)
                .HasColumnName("ticket_id");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.Subject)
                .HasColumnName("subject");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.Description)
                .HasColumnName("description");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.CustomerId)
                .HasColumnName("customer_id");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.CustomerName)
                .HasColumnName("customer_name");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.EmployeeId)
                .HasColumnName("employee_id");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.AssignedEmployee)
                .HasColumnName("assigned_employee");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.Priority)
                .HasColumnName("priority");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.Status)
                .HasColumnName("status");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.Category)
                .HasColumnName("category");

            modelBuilder.Entity<Tickets>()
                .Property(x => x.LastUpdated)
                .HasColumnName("last_updated");


            // =================================================
            // CUSTOMER ACCOUNTS
            // =================================================

            modelBuilder.Entity<CustomerAccount>()
                .ToTable("customer_accounts");

            modelBuilder.Entity<CustomerAccount>()
                .Property(x => x.CustomerAccountId)
                .HasColumnName("customer_account_id");

            modelBuilder.Entity<CustomerAccount>()
                .Property(x => x.CustomerId)
                .HasColumnName("customer_id");

            modelBuilder.Entity<CustomerAccount>()
                .Property(x => x.PasswordHash)
                .HasColumnName("password_hash");

            modelBuilder.Entity<CustomerAccount>()
                .Property(x => x.CreatedAt)
                .HasColumnName("created_at");


            // =================================================
            // EMPLOYEE ACTION ATTEMPTS
            // =================================================

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


            // =================================================
            // UNIQUE EMPLOYEE + ACTION
            // =================================================

            modelBuilder.Entity<EmployeeActionAttempt>()
                .HasIndex(x =>
                    new
                    {
                        x.EmployeeId,
                        x.ActionName
                    })
                .IsUnique();
        }
    }
}