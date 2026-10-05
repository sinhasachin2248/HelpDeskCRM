using HelpDeskCRM.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data;

namespace HelpDeskCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeActionAttemptsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        public EmployeeActionAttemptsApiController(
            ApplicationDbContext db,
            IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }


        // =====================================================
        // API TEST
        // =====================================================

        [HttpGet("Get")]
        public string Get()
        {
            return "API is working fine!";
        }


        // =====================================================
        // OPEN EMPLOYEE PORTAL
        // =====================================================

        [HttpPost("OpenPortal")]
        public async Task<IActionResult> OpenPortal(
            [FromBody] OpenPortalRequest request)
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection");

                await using var connection =
                    new MySqlConnection(connectionString);

                await using var command =
                    new MySqlCommand(
                        "sp_open_employee_portal",
                        connection);

                command.CommandType =
                    CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "p_employee_id",
                    request.EmployeeId);

                await connection.OpenAsync();

                await using var reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        message =
                            "Unable to record Open Portal attempt.",

                        employeeId =
                            request.EmployeeId
                    });
                }

                var success =
                    Convert.ToInt32(
                        reader["success"]);

                var message =
                    reader["message"]?.ToString();

                if (success == 0)
                {
                    return NotFound(new
                    {
                        message =
                            message,

                        employeeId =
                            request.EmployeeId
                    });
                }

                return Ok(new
                {
                    message =
                        message,

                    id =
                        reader["id"],

                    employeeId =
                        reader["employee_id"],

                    actionName =
                        reader["action_name"],

                    attemptCount =
                        reader["attempt_count"],

                    insertedOn =
                        reader["inserted_on"],

                    updatedOn =
                        reader["updated_on"],

                    status =
                        reader["status"]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = ex.Message,
                        innerException =
                            ex.InnerException?.Message
                    });
            }
        }


        // =====================================================
        // GET ALL EMPLOYEE ACTION ATTEMPTS
        // =====================================================

        [HttpGet("GetAllAttempts")]
        public async Task<IActionResult> GetAllAttempts()
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection");

                await using var connection =
                    new MySqlConnection(connectionString);

                await using var command =
                    new MySqlCommand(
                        "sp_get_all_employee_action_attempts",
                        connection);

                command.CommandType =
                    CommandType.StoredProcedure;

                await connection.OpenAsync();

                await using var reader =
                    await command.ExecuteReaderAsync();

                var attempts =
                    new List<object>();

                while (await reader.ReadAsync())
                {
                    attempts.Add(
                        new
                        {
                            id =
                                reader["id"],

                            employeeId =
                                reader["employee_id"],

                            actionName =
                                reader["action_name"],

                            attemptCount =
                                reader["attempt_count"],

                            insertedOn =
                                reader["inserted_on"],

                            updatedOn =
                                reader["updated_on"],

                            status =
                                reader["status"]
                        });
                }

                return Ok(attempts);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = ex.Message,
                        innerException =
                            ex.InnerException?.Message
                    });
            }
        }


        // =====================================================
        // GET EMPLOYEE ACTION ATTEMPTS
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
                    .Select(
                        x =>
                            new
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
                                    x.UpdatedOn,

                                status =
                                    x.Status
                            })
                    .ToListAsync();

            if (attempts.Count == 0)
            {
                return NotFound(
                    new
                    {
                        message =
                            "No action attempts found.",

                        employeeId =
                            employeeId
                    });
            }

            return Ok(attempts);
        }


        // =====================================================
        // EDIT EMPLOYEE
        // =====================================================

        [HttpPut("EditEmployee")]
        public async Task<IActionResult> EditEmployee(
            [FromBody] UpdateEmployeeRequest request)
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection");

                await using var connection =
                    new MySqlConnection(connectionString);

                await using var command =
                    new MySqlCommand(
                        "UpdateEmployee",
                        connection);

                command.CommandType =
                    CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "p_EmployeeId",
                    request.EmployeeId);

                command.Parameters.AddWithValue(
                    "p_Email",
                    request.Email);

                command.Parameters.AddWithValue(
                    "p_Phone",
                    request.Phone);

                command.Parameters.AddWithValue(
                    "p_City",
                    request.City);

                command.Parameters.AddWithValue(
                    "p_Designation",
                    request.Designation);

                command.Parameters.AddWithValue(
                    "p_Department",
                    request.Department);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                return Ok(
                    new
                    {
                        success = true,

                        message =
                            "Employee updated successfully.",

                        employeeId =
                            request.EmployeeId
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = ex.Message,
                        innerException =
                            ex.InnerException?.Message
                    });
            }
        }


        // =====================================================
        // DELETE EMPLOYEE
        // =====================================================

        [HttpDelete("DeleteEmployee/{employeeId:int}")]
        public async Task<IActionResult> DeleteEmployee(
            int employeeId)
        {
            try
            {
                var connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection");

                await using var connection =
                    new MySqlConnection(connectionString);

                await using var command =
                    new MySqlCommand(
                        "DeleteEmployee",
                        connection);

                command.CommandType =
                    CommandType.StoredProcedure;

                command.Parameters.AddWithValue(
                    "p_EmployeeId",
                    employeeId);

                await connection.OpenAsync();

                await using var reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return StatusCode(
                        500,
                        new
                        {
                            success = false,

                            message =
                                "No response received from DeleteEmployee procedure."
                        });
                }

                var success =
                    Convert.ToInt32(
                        reader["success"]);

                var message =
                    reader["message"]?.ToString();

                if (success == 0)
                {
                    return BadRequest(
                        new
                        {
                            success = false,

                            message =
                                message,

                            employeeId =
                                employeeId
                        });
                }

                return Ok(
                    new
                    {
                        success = true,

                        message =
                            message,

                        employeeId =
                            employeeId
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = ex.Message,
                        innerException =
                            ex.InnerException?.Message
                    });
            }
        }


        // =====================================================
        // OPEN PORTAL REQUEST
        // =====================================================

        public class OpenPortalRequest
        {
            public int EmployeeId { get; set; }
        }


        // =====================================================
        // UPDATE EMPLOYEE REQUEST
        // =====================================================

        public class UpdateEmployeeRequest
        {
            public int EmployeeId { get; set; }

            public string Email { get; set; } =
                string.Empty;

            public string Phone { get; set; } =
                string.Empty;

            public string City { get; set; } =
                string.Empty;

            public string Designation { get; set; } =
                string.Empty;

            public string Department { get; set; } =
                string.Empty;
        }
    }
}