using HelpDeskCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/employee-action-attempts")]
    public class EmployeeActionAttemptsApiController
        : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EmployeeActionAttemptsApiController(
            ApplicationDbContext db)
        {
            _db = db;
        }


        // =====================================================
        // GET ALL EMPLOYEE ACTIONS
        // GET:
        // /api/employee-action-attempts
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAllAttempts()
        {
            var attempts =
                await _db.EmployeeActionAttempts
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .Select(x => new
                    {
                        id = x.Id,

                        employeeId =
                            x.EmployeeId,

                        actionName =
                            x.ActionName,

                        attemptCount =
                            x.AttemptCount,

                        insertedOn =
                            x.InsertedOn,

                        updatedOn =
                            x.UpdatedOn

                        // STATUS INTENTIONALLY NOT RETURNED
                    })
                    .ToListAsync();


            return Ok(attempts);
        }


        // =====================================================
        // GET ACTIONS FOR ONE EMPLOYEE
        // GET:
        // /api/employee-action-attempts/10008
        // =====================================================

        [HttpGet("{employeeId:int}")]
        public async Task<IActionResult> GetEmployeeAttempts(
            int employeeId)
        {
            var attempts =
                await _db.EmployeeActionAttempts
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.EmployeeId ==
                            employeeId)
                    .OrderBy(x => x.Id)
                    .Select(x => new
                    {
                        id = x.Id,

                        employeeId =
                            x.EmployeeId,

                        actionName =
                            x.ActionName,

                        attemptCount =
                            x.AttemptCount,

                        insertedOn =
                            x.InsertedOn,

                        updatedOn =
                            x.UpdatedOn

                        // STATUS INTENTIONALLY NOT RETURNED
                    })
                    .ToListAsync();


            if (attempts.Count == 0)
            {
                return NotFound(new
                {
                    message =
                        $"No action records found for employee {employeeId}."
                });
            }


            return Ok(attempts);
        }
    }
}