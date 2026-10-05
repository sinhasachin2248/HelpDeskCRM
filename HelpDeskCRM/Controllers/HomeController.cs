using HelpDeskCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        // =====================================================
        // DATABASE
        // =====================================================

        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // DASHBOARD
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // -------------------------------------------------
            // TOTAL CUSTOMERS
            // ------------------------------------------------- 

            ViewBag.CustomerCount =
                await _db.Customers.CountAsync();


            // -------------------------------------------------
            // TOTAL EMPLOYEES
            // -------------------------------------------------

            ViewBag.EmployeeCount =
                await _db.Employees.CountAsync();


            // -------------------------------------------------
            // TOTAL TICKETS
            // -------------------------------------------------

            ViewBag.TicketCount =
                await _db.Tickets.CountAsync();


            // -------------------------------------------------
            // PENDING TICKETS
            // -------------------------------------------------

            ViewBag.PendingTickets =
                await _db.Tickets.CountAsync(
                    x => x.Status == "Pending"    
                );


            return View();
        }


        // =====================================================
        // PRIVACY
        // =====================================================

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}