using HelpDeskCRM.Data;
using HelpDeskCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        // =====================================================
        // DATABASE
        // =====================================================

        private readonly ApplicationDbContext _db;

        public CustomersController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // MAIN CUSTOMER PAGE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string section = "customerList")
        {
            var customers = await _db.Customers
                .AsNoTracking()
                .ToListAsync();

            var model = new CustomerManagementViewModel
            {
                Customers = customers,
                ActiveSection = section
            };

            return View(model);
        }


        // =====================================================
        // CREATE CUSTOMER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CustomerManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Customers = await _db.Customers
                    .AsNoTracking()
                    .ToListAsync();

                model.ActiveSection = "addCustomer";

                return View("Index", model);
            }

            string email = model.NewCustomer.Email.Trim();

            bool emailExists = await _db.Customers
                .AnyAsync(x =>
                    x.Email.ToLower() == email.ToLower()
                );

            if (emailExists)
            {
                ModelState.AddModelError(
                    "NewCustomer.Email",
                    "A customer with this email already exists."
                );

                model.Customers = await _db.Customers
                    .AsNoTracking()
                    .ToListAsync();

                model.ActiveSection = "addCustomer";

                return View("Index", model);
            }

            var customer = new Customer
            {
                FirstName = model.NewCustomer.FirstName,
                LastName = model.NewCustomer.LastName,
                Email = email,
                Phone = model.NewCustomer.Phone,
                CompanyName = model.NewCustomer.CompanyName,
                City = model.NewCustomer.City,
                LastUpdated = DateTime.Now
            };

            _db.Customers.Add(customer);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Customer created successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section = "customerList"
                }
            );
        }


        // =====================================================
        // CUSTOMER DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int customerId)
        {
            // -------------------------------------------------
            // GET SELECTED CUSTOMER
            // -------------------------------------------------

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId
                );


            // -------------------------------------------------
            // GET ALL CUSTOMERS
            // -------------------------------------------------

            var customers = await _db.Customers
                .AsNoTracking()
                .ToListAsync();


            // -------------------------------------------------
            // GET CUSTOMER TICKETS
            // -------------------------------------------------

            var customerTickets = await _db.Tickets
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.LastUpdated)
                .ToListAsync();


            // -------------------------------------------------
            // CREATE VIEW MODEL
            // -------------------------------------------------

            var model = new CustomerManagementViewModel
            {
                Customers = customers,

                SelectedCustomer = customer,

                CustomerTickets = customerTickets,

                ActiveSection = "customerDetails"
            };


            // -------------------------------------------------
            // CUSTOMER NOT FOUND
            // -------------------------------------------------

            if (customer == null)
            {
                model.Message =
                    $"No customer found with ID {customerId}.";

                model.CustomerTickets =
                    new List<Tickets>();
            }


            return View("Index", model);
        }


        // =====================================================
        // FIND CUSTOMER FOR EDIT
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> FindForEdit(
            int customerId)
        {
            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId
                );

            var customers = await _db.Customers
                .AsNoTracking()
                .ToListAsync();

            var model = new CustomerManagementViewModel
            {
                Customers = customers,

                SelectedCustomer = customer,

                ActiveSection = "editCustomer"
            };

            if (customer == null)
            {
                model.Message =
                    $"Customer with ID {customerId} was not found.";
            }

            return View("Index", model);
        }


        // =====================================================
        // UPDATE CUSTOMER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Customer model)
        {
            // -------------------------------------------------
            // VALIDATION
            // -------------------------------------------------

            if (!ModelState.IsValid)
            {
                var customers = await _db.Customers
                    .AsNoTracking()
                    .ToListAsync();

                var viewModel =
                    new CustomerManagementViewModel
                    {
                        Customers = customers,

                        SelectedCustomer = model,

                        ActiveSection = "editCustomer"
                    };

                return View(
                    "Index",
                    viewModel
                );
            }


            // -------------------------------------------------
            // FIND CUSTOMER
            // -------------------------------------------------

            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId == model.CustomerId
                );

            if (customer == null)
            {
                TempData["ErrorMessage"] =
                    $"Customer with ID {model.CustomerId} was not found.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // -------------------------------------------------
            // DUPLICATE EMAIL CHECK
            // -------------------------------------------------

            string email = model.Email.Trim();

            bool emailExists = await _db.Customers
                .AnyAsync(x =>
                    x.CustomerId != customer.CustomerId &&
                    x.Email.ToLower() == email.ToLower()
                );

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "Another customer already uses this email."
                );

                var customers = await _db.Customers
                    .AsNoTracking()
                    .ToListAsync();

                var viewModel =
                    new CustomerManagementViewModel
                    {
                        Customers = customers,

                        SelectedCustomer = model,

                        ActiveSection = "editCustomer"
                    };

                return View(
                    "Index",
                    viewModel
                );
            }


            // -------------------------------------------------
            // UPDATE DATABASE RECORD
            // -------------------------------------------------

            customer.FirstName =
                model.FirstName;

            customer.LastName =
                model.LastName;

            customer.Email =
                email;

            customer.Phone =
                model.Phone;

            customer.CompanyName =
                model.CompanyName;

            customer.City =
                model.City;

            customer.LastUpdated =
                DateTime.Now;


            // -------------------------------------------------
            // SAVE CHANGES
            // -------------------------------------------------

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Customer updated successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section = "customerList"
                }
            );
        }


        // =====================================================
        // FIND CUSTOMER FOR DELETE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> FindForDelete(
            int customerId)
        {
            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId
                );

            var customers = await _db.Customers
                .AsNoTracking()
                .ToListAsync();

            var model =
                new CustomerManagementViewModel
                {
                    Customers = customers,

                    SelectedCustomer = customer,

                    ActiveSection = "deleteCustomer"
                };

            if (customer == null)
            {
                model.Message =
                    $"Customer with ID {customerId} was not found.";
            }

            return View(
                "Index",
                model
            );
        }


        // =====================================================
        // DELETE CUSTOMER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int customerId)
        {
            // -------------------------------------------------
            // FIND CUSTOMER
            // -------------------------------------------------

            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId
                );

            if (customer == null)
            {
                TempData["ErrorMessage"] =
                    $"Customer with ID {customerId} was not found.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // -------------------------------------------------
            // CHECK CUSTOMER TICKETS
            // -------------------------------------------------

            bool hasTickets = await _db.Tickets
                .AnyAsync(
                    x => x.CustomerId == customerId
                );

            if (hasTickets)
            {
                TempData["ErrorMessage"] =
                    "This customer cannot be deleted because tickets are associated with the customer.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section = "deleteCustomer"
                    }
                );
            }


            // -------------------------------------------------
            // DELETE CUSTOMER
            // -------------------------------------------------

            _db.Customers.Remove(customer);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Customer {customerId} deleted successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section = "customerList"
                }
            );
        }
    }
}