using HelpDeskCRM.Data;
using HelpDeskCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/tickets")]
    public class TicketsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public TicketsApiController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // GET 1 - GET ALL TICKETS
        // GET: /api/tickets
        // =====================================================
        
        [HttpGet]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await _db.Tickets
                .AsNoTracking()
                .Select(t => new
                {
                    ticketId = t.TicketId,
                    subject = t.Subject,
                    description = t.Description,
                    customerId = t.CustomerId,
                    customerName = t.CustomerName,
                    employeeId = t.EmployeeId,
                    assignedEmployee = t.AssignedEmployee,
                    priority = t.Priority,
                    status = t.Status,
                    category = t.Category,
                    lastUpdated = t.LastUpdated
                })
                .ToListAsync();

            return Ok(tickets);
        }


        // =====================================================
        // GET 2 - GET SINGLE TICKET
        // GET: /api/tickets/101
        // =====================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTicket(
            int id)
        {
            var ticket = await _db.Tickets
                .AsNoTracking()
                .Where(t => t.TicketId == id)
                .Select(t => new
                {
                    ticketId = t.TicketId,
                    subject = t.Subject,
                    description = t.Description,
                    customerId = t.CustomerId,
                    customerName = t.CustomerName,
                    employeeId = t.EmployeeId,
                    assignedEmployee = t.AssignedEmployee,
                    priority = t.Priority,
                    status = t.Status,
                    category = t.Category,
                    lastUpdated = t.LastUpdated
                })
                .FirstOrDefaultAsync();

            if (ticket == null)
            {
                return NotFound(new
                {
                    message = $"Ticket with ID {id} was not found."
                });
            }

            return Ok(ticket);
        }


        // =====================================================
        // GET 3 - GET PENDING TICKETS
        // GET: /api/tickets/pending
        // =====================================================

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingTickets()
        {
            var tickets = await _db.Tickets
                .AsNoTracking()
                .Where(t => t.Status == "Pending")
                .Select(t => new
                {
                    ticketId = t.TicketId,
                    subject = t.Subject,
                    customerName = t.CustomerName,
                    assignedEmployee = t.AssignedEmployee,
                    priority = t.Priority,
                    status = t.Status
                })
                .ToListAsync();

            return Ok(tickets);
        }


        // =====================================================
        // POST 1 - CREATE TICKET
        // POST: /api/tickets
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> CreateTicket(
            [FromBody] CreateTicketRequest request)
        {
            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId == request.CustomerId
                );

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }


            Employee? employee = null;

            if (request.EmployeeId.HasValue)
            {
                employee = await _db.Employees
                    .FirstOrDefaultAsync(
                        x => x.EmployeeId ==
                             request.EmployeeId.Value
                    );

                if (employee == null)
                {
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });
                }
            }


            var ticket = new Tickets
            {
                Subject = request.Subject.Trim(),

                Description = request.Description.Trim(),

                CustomerId = customer.CustomerId,

                CustomerName =
                    $"{customer.FirstName} {customer.LastName}",

                EmployeeId =
                    employee?.EmployeeId,

                AssignedEmployee =
                    employee == null
                        ? string.Empty
                        : $"{employee.FirstName} {employee.LastName}",

                Priority = request.Priority,

                Status =
                    string.IsNullOrWhiteSpace(request.Status)
                        ? "Open"
                        : request.Status,

                Category = request.Category,

                LastUpdated = DateTime.Now
            };


            _db.Tickets.Add(ticket);

            await _db.SaveChangesAsync();


            return CreatedAtAction(
                nameof(GetTicket),
                new
                {
                    id = ticket.TicketId
                },
                new
                {
                    message = "Ticket created successfully.",
                    ticketId = ticket.TicketId
                }
            );
        }


        // =====================================================
        // POST 2 - CREATE PENDING TICKET
        // POST: /api/tickets/pending
        // =====================================================

        [HttpPost("pending")]
        public async Task<IActionResult> CreatePendingTicket(
            [FromBody] CreatePendingTicketRequest request)
        {
            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId == request.CustomerId
                );

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }


            var ticket = new Tickets
            {
                Subject = request.Subject.Trim(),

                Description = request.Description.Trim(),

                CustomerId = customer.CustomerId,

                CustomerName =
                    $"{customer.FirstName} {customer.LastName}",

                EmployeeId = null,

                AssignedEmployee = string.Empty,

                Priority = request.Priority,

                Status = "Pending",

                Category = request.Category,

                LastUpdated = DateTime.Now
            };


            _db.Tickets.Add(ticket);

            await _db.SaveChangesAsync();


            return CreatedAtAction(
                nameof(GetTicket),
                new
                {
                    id = ticket.TicketId
                },
                new
                {
                    message =
                        "Pending ticket created successfully.",

                    ticketId = ticket.TicketId
                }
            );
        }


        // =====================================================
        // PUT - UPDATE TICKET
        // PUT: /api/tickets/101
        // =====================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTicket(
            int id,
            [FromBody] UpdateTicketRequest request)
        {
            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.TicketId == id
                );

            if (ticket == null)
            {
                return NotFound(new
                {
                    message =
                        $"Ticket with ID {id} was not found."
                });
            }


            // -------------------------------------------------
            // CUSTOMER VALIDATION
            // -------------------------------------------------

            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    x => x.CustomerId ==
                         request.CustomerId
                );

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Customer not found."
                });
            }


            // -------------------------------------------------
            // EMPLOYEE VALIDATION
            // -------------------------------------------------

            Employee? employee = null;

            if (request.EmployeeId.HasValue)
            {
                employee = await _db.Employees
                    .FirstOrDefaultAsync(
                        x => x.EmployeeId ==
                             request.EmployeeId.Value
                    );

                if (employee == null)
                {
                    return NotFound(new
                    {
                        message = "Employee not found."
                    });
                }
            }


            // -------------------------------------------------
            // UPDATE
            // -------------------------------------------------

            ticket.Subject =
                request.Subject.Trim();

            ticket.Description =
                request.Description.Trim();

            ticket.CustomerId =
                customer.CustomerId;

            ticket.CustomerName =
                $"{customer.FirstName} {customer.LastName}";

            ticket.EmployeeId =
                employee?.EmployeeId;

            ticket.AssignedEmployee =
                employee == null
                    ? string.Empty
                    : $"{employee.FirstName} {employee.LastName}";

            ticket.Priority =
                request.Priority;

            ticket.Status =
                request.Status;

            ticket.Category =
                request.Category;

            ticket.LastUpdated =
                DateTime.Now;


            await _db.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Ticket updated successfully.",

                ticketId = ticket.TicketId
            });
        }


        // =====================================================
        // DELETE - DELETE TICKET
        // DELETE: /api/tickets/101
        // =====================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTicket(
            int id)
        {
            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.TicketId == id
                );

            if (ticket == null)
            {
                return NotFound(new
                {
                    message =
                        $"Ticket with ID {id} was not found."
                });
            }


            _db.Tickets.Remove(ticket);

            await _db.SaveChangesAsync();


            return Ok(new
            {
                message =
                    $"Ticket {id} deleted successfully.",

                ticketId = id
            });
        }


        // =====================================================
        // POST - ASSIGN TICKET
        // POST: /api/tickets/101/assign
        // =====================================================

        [HttpPost("{id:int}/assign")]
        public async Task<IActionResult> AssignTicket(
            int id,
            [FromBody] AssignTicketRequest request)
        {
            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.TicketId == id
                );

            if (ticket == null)
            {
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }


            var employee = await _db.Employees
                .FirstOrDefaultAsync(
                    x => x.EmployeeId ==
                         request.EmployeeId
                );

            if (employee == null)
            {
                return NotFound(new
                {
                    message = "Employee not found."
                });
            }


            ticket.EmployeeId =
                employee.EmployeeId;

            ticket.AssignedEmployee =
                $"{employee.FirstName} {employee.LastName}";

            ticket.LastUpdated =
                DateTime.Now;


            await _db.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Ticket assigned successfully.",

                ticketId = ticket.TicketId,

                employeeId =
                    employee.EmployeeId,

                assignedEmployee =
                    ticket.AssignedEmployee
            });
        }


        // =====================================================
        // POST - UNASSIGN TICKET
        // POST: /api/tickets/101/unassign
        // =====================================================

        [HttpPost("{id:int}/unassign")]
        public async Task<IActionResult> UnassignTicket(
            int id)
        {
            var ticket = await _db.Tickets
                .FirstOrDefaultAsync(
                    x => x.TicketId == id
                );

            if (ticket == null)
            {
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }


            ticket.EmployeeId = null;

            ticket.AssignedEmployee =
                string.Empty;

            ticket.LastUpdated =
                DateTime.Now;


            await _db.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Ticket unassigned successfully.",

                ticketId = ticket.TicketId
            });
        }
    }


    // =========================================================
    // CREATE TICKET REQUEST
    // =========================================================

    public class CreateTicketRequest
    {
        public string Subject { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public int CustomerId { get; set; }

        public int? EmployeeId { get; set; }

        public string Priority { get; set; }
            = string.Empty;

        public string Status { get; set; }
            = "Open";

        public string Category { get; set; }
            = string.Empty;
    }


    // =========================================================
    // CREATE PENDING TICKET REQUEST
    // =========================================================

    public class CreatePendingTicketRequest
    {
        public string Subject { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public int CustomerId { get; set; }

        public string Priority { get; set; }
            = string.Empty;

        public string Category { get; set; }
            = string.Empty;
    }


    // =========================================================
    // UPDATE TICKET REQUEST
    // =========================================================

    public class UpdateTicketRequest
    {
        public string Subject { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public int CustomerId { get; set; }

        public int? EmployeeId { get; set; }

        public string Priority { get; set; }
            = string.Empty;

        public string Status { get; set; }
            = string.Empty;

        public string Category { get; set; }
            = string.Empty;
    }


    // =========================================================
    // ASSIGN TICKET REQUEST
    // =========================================================

    public class AssignTicketRequest
    {
        public int EmployeeId { get; set; }
    }
}