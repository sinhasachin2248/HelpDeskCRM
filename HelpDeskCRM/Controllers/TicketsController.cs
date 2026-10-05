using HelpDeskCRM.Data;
using HelpDeskCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TicketsController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // TICKET LIST
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string section = "ticketList")
        {
            var model = new TicketManagementViewModel
            {
                Tickets = await _db.Tickets
                    .AsNoTracking()
                    .ToListAsync(),

                Customers = await _db.Customers
                    .AsNoTracking()
                    .ToListAsync(),

                Employees = await _db.Employees
                    .AsNoTracking()
                    .ToListAsync(),

                ActiveSection = section
            };

            return View(model);
        }


        // =====================================================
        // CREATE TICKET
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TicketManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLists(model);

                model.ActiveSection = "addTicket";

                return View("Index", model);
            }


            // -------------------------------------------------
            // FIND CUSTOMER
            // -------------------------------------------------

            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId ==
                         model.NewTicket.CustomerId
                );

            if (customer == null)
            {
                ModelState.AddModelError(
                    "NewTicket.CustomerId",
                    "Selected customer was not found."
                );

                await LoadLists(model);

                model.ActiveSection = "addTicket";

                return View("Index", model);
            }


            // -------------------------------------------------
            // FIND EMPLOYEE IF ASSIGNED
            // -------------------------------------------------

            Employee? employee = null;

            if (model.NewTicket.EmployeeId.HasValue)
            {
                employee = await _db.Employees
                    .FirstOrDefaultAsync(
                        x => x.EmployeeId ==
                             model.NewTicket.EmployeeId.Value
                    );

                if (employee == null)
                {
                    ModelState.AddModelError(
                        "NewTicket.EmployeeId",
                        "Selected employee was not found."
                    );

                    await LoadLists(model);

                    model.ActiveSection = "addTicket";

                    return View("Index", model);
                }
            }


            // -------------------------------------------------
            // CREATE TICKET
            // -------------------------------------------------

            var ticket = new Tickets
            {
                Subject =
                    model.NewTicket.Subject.Trim(),

                Description =
                    model.NewTicket.Description.Trim(),

                CustomerId =
                    customer.CustomerId,

                CustomerName =
                    $"{customer.FirstName} {customer.LastName}",

                EmployeeId =
                    employee?.EmployeeId,

                AssignedEmployee =
                    employee == null
                        ? string.Empty
                        : $"{employee.FirstName} {employee.LastName}",

                Priority =
                    model.NewTicket.Priority,

                Status =
                    string.IsNullOrWhiteSpace(
                        model.NewTicket.Status)
                        ? "Open"
                        : model.NewTicket.Status,

                Category =
                    model.NewTicket.Category,

                LastUpdated =
                    DateTime.Now
            };


            _db.Tickets.Add(ticket);

            await _db.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Ticket created successfully.";


            return RedirectToAction(
                nameof(Index),
                new
                {
                    section = "ticketList"
                }
            );
        }


        // =====================================================
        // TICKET DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int ticketId)
        {
            // -------------------------------------------------
            // FIND SELECTED TICKET
            // -------------------------------------------------

            var ticket = await _db.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TicketId == ticketId
                );


            // -------------------------------------------------
            // LOAD ALL DATA REQUIRED BY THE VIEW
            // -------------------------------------------------

            var model = new TicketManagementViewModel
            {
                Tickets = await _db.Tickets
                    .AsNoTracking()
                    .ToListAsync(),

                Customers = await _db.Customers
                    .AsNoTracking()
                    .ToListAsync(),

                Employees = await _db.Employees
                    .AsNoTracking()
                    .ToListAsync(),

                SelectedTicket = ticket,

                ActiveSection = "ticketDetails"
            };


            // -------------------------------------------------
            // TICKET NOT FOUND
            // -------------------------------------------------

            if (ticket == null)
            {
                model.Message =
                    $"Ticket with ID {ticketId} was not found.";
            }


            return View(
                "Index",
                model
            );
        }


        // =====================================================
        // ASSIGN / REASSIGN TICKET
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicket(
            int ticketId,
            int? employeeId)
        {
            // -------------------------------------------------
            // FIND TICKET
            // -------------------------------------------------

            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.TicketId == ticketId
                );


            if (ticket == null)
            {
                TempData["ErrorMessage"] =
                    $"Ticket with ID {ticketId} was not found.";

                return RedirectToAction(
                    nameof(Index)
                );
            }


            // -------------------------------------------------
            // UNASSIGN TICKET
            // -------------------------------------------------

            if (!employeeId.HasValue)
            {
                ticket.EmployeeId = null;

                ticket.AssignedEmployee =
                    string.Empty;

                ticket.LastUpdated =
                    DateTime.Now;

                await _db.SaveChangesAsync();


                TempData["SuccessMessage"] =
                    $"Ticket {ticketId} has been unassigned.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        ticketId = ticketId
                    }
                );
            }


            // -------------------------------------------------
            // FIND EMPLOYEE
            // -------------------------------------------------

            var employee = await _db.Employees
                .FirstOrDefaultAsync(
                    x => x.EmployeeId ==
                         employeeId.Value
                );


            if (employee == null)
            {
                TempData["ErrorMessage"] =
                    $"Employee with ID {employeeId.Value} was not found.";

                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        ticketId = ticketId
                    }
                );
            }


            // -------------------------------------------------
            // ASSIGN EMPLOYEE
            // -------------------------------------------------

            ticket.EmployeeId =
                employee.EmployeeId;

            ticket.AssignedEmployee =
                $"{employee.FirstName} {employee.LastName}";

            ticket.LastUpdated =
                DateTime.Now;


            await _db.SaveChangesAsync();


            TempData["SuccessMessage"] =
                $"Ticket {ticketId} assigned to {ticket.AssignedEmployee}.";


            return RedirectToAction(
                nameof(Details),
                new
                {
                    ticketId = ticketId
                }
            );
        }


        // =====================================================
        // HELPER — LOAD LISTS
        // =====================================================

        private async Task LoadLists(
            TicketManagementViewModel model)
        {
            model.Tickets = await _db.Tickets
                .AsNoTracking()
                .ToListAsync();

            model.Customers = await _db.Customers
                .AsNoTracking()
                .ToListAsync();

            model.Employees = await _db.Employees
                .AsNoTracking()
                .ToListAsync();
        }
    }
}