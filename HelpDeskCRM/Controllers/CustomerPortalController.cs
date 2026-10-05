using HelpDeskCRM.Data;
using HelpDeskCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HelpDeskCRM.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerPortalController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerPortalController(
            ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string section = "myTickets")
        {
            var customerId = GetCustomerId();

            if (customerId == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId.Value
                );

            if (customer == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var tickets = await _db.Tickets
                .AsNoTracking()
                .Where(
                    x => x.CustomerId == customerId.Value
                )
                .OrderByDescending(
                    x => x.LastUpdated
                )
                .ToListAsync();

            var model = new CustomerPortalViewModel
            {
                Customer = customer,
                Tickets = tickets,
                ActiveSection = section
            };

            return View(model);
        }


        // =====================================================
        // CREATE CUSTOMER TICKET
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(
            CustomerPortalViewModel model)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId.Value
                );

            if (customer == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            if (!ModelState.IsValid)
            {
                model.Customer = customer;

                model.Tickets = await _db.Tickets
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.CustomerId ==
                            customerId.Value
                    )
                    .ToListAsync();

                model.ActiveSection = "createTicket";

                return View("Index", model);
            }

            var ticket = new Tickets
            {
                Subject =
                    model.NewTicket.Subject.Trim(),

                Description =
                    model.NewTicket.Description.Trim(),

                // NEVER take CustomerId from the form
                CustomerId =
                    customer.CustomerId,

                CustomerName =
                    $"{customer.FirstName} {customer.LastName}",

                EmployeeId = null,

                AssignedEmployee =
                    string.Empty,

                Priority =
                    string.IsNullOrWhiteSpace(
                        model.NewTicket.Priority)
                        ? "Medium"
                        : model.NewTicket.Priority,

                Status = "Open",

                Category =
                    string.IsNullOrWhiteSpace(
                        model.NewTicket.Category)
                        ? "General"
                        : model.NewTicket.Category,

                LastUpdated = DateTime.Now
            };

            _db.Tickets.Add(ticket);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Ticket {ticket.TicketId} created successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section = "myTickets"
                }
            );
        }


        // =====================================================
        // CUSTOMER TICKET DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> TicketDetails(
            int ticketId)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var ticket = await _db.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.TicketId == ticketId &&
                        x.CustomerId == customerId.Value
                );

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.CustomerId ==
                        customerId.Value
                );

            if (customer == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var model = new CustomerPortalViewModel
            {
                Customer = customer,

                Tickets = await _db.Tickets
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.CustomerId ==
                            customerId.Value
                    )
                    .ToListAsync(),

                SelectedTicket = ticket,

                ActiveSection = "ticketDetails"
            };

            if (ticket == null)
            {
                model.Message =
                    "Ticket not found.";
            }

            return View("Index", model);
        }


        // =====================================================
        // CUSTOMER DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var customerId = GetCustomerId();

            if (customerId == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.CustomerId ==
                        customerId.Value
                );

            if (customer == null)
            {
                return RedirectToAction(
                    "CustomerLogin",
                    "Account"
                );
            }

            var model = new CustomerPortalViewModel
            {
                Customer = customer,

                Tickets = await _db.Tickets
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.CustomerId ==
                            customerId.Value
                    )
                    .ToListAsync(),

                ActiveSection = "customerDetails"
            };

            return View("Index", model);
        }


        // =====================================================
        // GET LOGGED-IN CUSTOMER ID
        // =====================================================

        private int? GetCustomerId()
        {
            var claim = User.FindFirst(
                ClaimTypes.NameIdentifier
            );

            if (claim == null)
            {
                return null;
            }

            if (int.TryParse(
                    claim.Value,
                    out int customerId))
            {
                return customerId;
            }

            return null;
        }
    }
}