using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using HelpDeskCRM.Data;
using HelpDeskCRM.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmployeesController(
            ApplicationDbContext db,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _db = db;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }


        // =====================================================
        // EMPLOYEE LIST / MAIN PAGE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string section = "employeeList")
        {
            var model = new EmployeeManagementViewModel
            {
                Employees = await _db.Employees
                    .AsNoTracking()
                    .ToListAsync(),

                ActiveSection = section
            };

            return View(model);
        }


        // =====================================================
        // CREATE EMPLOYEE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EmployeeManagementViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLists(model);

                model.ActiveSection = "addEmployee";

                return View("Index", model);
            }

            string email =
                model.NewEmployee.Email.Trim();

            bool emailExists =
                await _db.Employees.AnyAsync(
                    x =>
                        x.Email.ToLower() ==
                        email.ToLower());

            if (emailExists)
            {
                ModelState.AddModelError(
                    "NewEmployee.Email",
                    "An employee with this email already exists."
                );

                await LoadLists(model);

                model.ActiveSection = "addEmployee";

                return View("Index", model);
            }

            var employee = new Employee
            {
                FirstName =
                    model.NewEmployee.FirstName,

                LastName =
                    model.NewEmployee.LastName,

                Email =
                    email,

                Phone =
                    model.NewEmployee.Phone,

                Department =
                    model.NewEmployee.Department,

                Designation =
                    model.NewEmployee.Designation,

                City =
                    model.NewEmployee.City,

                Status =
                    string.IsNullOrWhiteSpace(
                        model.NewEmployee.Status)
                        ? "Active"
                        : model.NewEmployee.Status,

                LastUpdated =
                    DateTime.Now
            };

            _db.Employees.Add(employee);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Employee created successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section = "employeeList"
                });
        }


        // =====================================================
        // EMPLOYEE DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int employeeId)
        {
            var employee =
                await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId);

            var employees =
                await _db.Employees
                    .AsNoTracking()
                    .ToListAsync();

            var assignedTickets =
                await _db.Tickets
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.EmployeeId ==
                            employeeId)
                    .OrderByDescending(
                        x =>
                            x.LastUpdated)
                    .ToListAsync();

            var model =
                new EmployeeManagementViewModel
                {
                    Employees =
                        employees,

                    SelectedEmployee =
                        employee,

                    AssignedTasks =
                        assignedTickets,

                    ActiveSection =
                        "employeeDetails"
                };

            if (employee == null)
            {
                model.Message =
                    $"Employee with ID {employeeId} was not found.";

                model.AssignedTasks =
                    new List<Tickets>();
            }

            return View(
                "Index",
                model);
        }


        // =====================================================
        // OPEN EMPLOYEE PORTAL
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenPortal(
            int employeeId)
        {
            try
            {
                var apiResponse =
                    await _httpClient.PostAsJsonAsync(
                        "https://localhost:7215/api/EmployeeActionAttemptsApi/OpenPortal",
                        new
                        {
                            employeeId =
                                employeeId
                        });

                if (!apiResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        "Unable to record Open Portal action through API.";

                    return RedirectToAction(
                        nameof(Index),
                        new
                        {
                            section =
                                "employeeDetails"
                        });
                }

                return RedirectToAction(
                    nameof(Portal),
                    new
                    {
                        employeeId =
                            employeeId
                    });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to HelpDeskCRM API.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "employeeDetails"
                    });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An unexpected error occurred while opening the employee portal.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "employeeDetails"
                    });
            }
        }


        // =====================================================
        // EMPLOYEE PORTAL
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Portal(
            int employeeId)
        {
            var employee =
                await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId);

            if (employee == null)
            {
                var errorModel =
                    new EmployeeManagementViewModel
                    {
                        Employees =
                            await _db.Employees
                                .AsNoTracking()
                                .ToListAsync(),

                        ActiveSection =
                            "employeePortal",

                        Message =
                            $"Employee with ID {employeeId} was not found.",

                        AssignedTasks =
                            new List<Tickets>()
                    };

                return View(
                    "Index",
                    errorModel);
            }

            var assignedTickets =
                await _db.Tickets
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.EmployeeId ==
                            employeeId)
                    .OrderByDescending(
                        x =>
                            x.LastUpdated)
                    .ToListAsync();

            var model =
                new EmployeeManagementViewModel
                {
                    Employees =
                        await _db.Employees
                            .AsNoTracking()
                            .ToListAsync(),

                    SelectedEmployee =
                        employee,

                    AssignedTasks =
                        assignedTickets,

                    ActiveSection =
                        "employeePortal"
                };

            return View(
                "Index",
                model);
        }


        // =====================================================
        // SOLVE TICKET
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("SolveTicket")]
        public async Task<IActionResult> SolveTicket(
            int employeeId,
            int ticketId)
        {
            var employee =
                await _db.Employees
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] =
                    $"Employee with ID {employeeId} was not found.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "employeePortal"
                    });
            }

            var ticket =
                await _db.Tickets
                    .FirstOrDefaultAsync(
                        x =>
                            x.TicketId ==
                            ticketId);

            if (ticket == null)
            {
                TempData["ErrorMessage"] =
                    $"Ticket with ID {ticketId} was not found.";

                return RedirectToAction(
                    nameof(Portal),
                    new
                    {
                        employeeId =
                            employeeId
                    });
            }

            if (ticket.EmployeeId != employeeId)
            {
                TempData["ErrorMessage"] =
                    "This ticket is not assigned to this employee.";

                return RedirectToAction(
                    nameof(Portal),
                    new
                    {
                        employeeId =
                            employeeId
                    });
            }

            ticket.Status =
                "Solved";

            ticket.LastUpdated =
                DateTime.Now;

            await TrackEmployeeAction(
                employeeId,
                "Problem Solved");

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Ticket {ticketId} has been marked as solved.";

            return RedirectToAction(
                nameof(Portal),
                new
                {
                    employeeId =
                        employeeId
                });
        }


        // =====================================================
        // FIND EMPLOYEE FOR EDIT
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> FindForEdit(
            int employeeId)
        {
            var employee =
                await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId);

            var model =
                new EmployeeManagementViewModel
                {
                    Employees =
                        await _db.Employees
                            .AsNoTracking()
                            .ToListAsync(),

                    SelectedEmployee =
                        employee,

                    ActiveSection =
                        "editEmployee"
                };

            if (employee == null)
            {
                model.Message =
                    $"Employee with ID {employeeId} was not found.";
            }

            return View(
                "Index",
                model);
        }


        // =====================================================
        // UPDATE EMPLOYEE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            Employee model)
        {
            if (!ModelState.IsValid)
            {
                var viewModel =
                    new EmployeeManagementViewModel
                    {
                        Employees =
                            await _db.Employees
                                .AsNoTracking()
                                .ToListAsync(),

                        SelectedEmployee =
                            model,

                        ActiveSection =
                            "editEmployee"
                    };

                return View(
                    "Index",
                    viewModel);
            }

            var employee =
                await _db.Employees
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            model.EmployeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] =
                    $"Employee with ID {model.EmployeeId} was not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            string email =
                model.Email.Trim();

            bool emailExists =
                await _db.Employees.AnyAsync(
                    x =>
                        x.EmployeeId !=
                        employee.EmployeeId
                        &&
                        x.Email.ToLower() ==
                        email.ToLower());

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "Another employee already uses this email."
                );

                var viewModel =
                    new EmployeeManagementViewModel
                    {
                        Employees =
                            await _db.Employees
                                .AsNoTracking()
                                .ToListAsync(),

                        SelectedEmployee =
                            model,

                        ActiveSection =
                            "editEmployee"
                    };

                return View(
                    "Index",
                    viewModel);
            }

            employee.FirstName =
                model.FirstName;

            employee.LastName =
                model.LastName;

            employee.Email =
                email;

            employee.Phone =
                model.Phone;

            employee.Department =
                model.Department;

            employee.Designation =
                model.Designation;

            employee.City =
                model.City;

            employee.Status =
                string.IsNullOrWhiteSpace(
                    model.Status)
                    ? "Active"
                    : model.Status;

            employee.LastUpdated =
                DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Employee updated successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section =
                        "employeeList"
                });
        }


        // =====================================================
        // FETCH EMPLOYEE BY MOBILE NUMBER
        // =====================================================
        //
        // IMPORTANT:
        // This feature is completely separate from Edit Employee.
        //
        // FETCH:
        // 1. Validates mobile number
        // 2. Calls Manager API directly from the Main CRM
        // 3. Reads the Manager API response
        // 4. Displays the response in the CRM
        // 5. DOES NOT save anything to the database
        //
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchMobile(
            string mobileNo)
        {
            mobileNo =
                mobileNo?.Trim() ??
                string.Empty;

            // -------------------------------------------------
            // VALIDATE MOBILE NUMBER
            // -------------------------------------------------

            if (string.IsNullOrWhiteSpace(mobileNo) ||
                mobileNo.Length != 10 ||
                !mobileNo.All(char.IsDigit) ||
                mobileNo[0] < '6' ||
                mobileNo[0] > '9')
            {
                TempData["ErrorMessage"] =
                    "Mobile number must be exactly 10 digits and start with 6-9.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }

            try
            {
                // -------------------------------------------------
                // MANAGER API URL FROM CONFIGURATION
                // -------------------------------------------------

                var managerApiUrl =
                    _configuration["ManagerApi:MobileLookupUrl"];

                if (string.IsNullOrWhiteSpace(managerApiUrl))
                {
                    TempData["ErrorMessage"] =
                        "Manager API URL is not configured.";

                    return RedirectToAction(
                        nameof(Index),
                        new
                        {
                            section =
                                "mobileLookup"
                        });
                }

                // -------------------------------------------------
                // REQUEST BODY
                // -------------------------------------------------

                var requestBody =
                    new
                    {
                        MobileNo = mobileNo
                    };

                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Post,
                        managerApiUrl);

                request.Content =
                    new StringContent(
                        JsonSerializer.Serialize(
                            requestBody,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = null
                            }),
                        Encoding.UTF8,
                        "application/json");

                // -------------------------------------------------
                // BASIC AUTH FROM CONFIGURATION
                // -------------------------------------------------

                var authorization =
                    _configuration["ManagerApi:Authorization"];

                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    request.Headers.TryAddWithoutValidation(
                        "Authorization",
                        authorization);
                }

                // -------------------------------------------------
                // CALL MANAGER API DIRECTLY
                // -------------------------------------------------

                var apiResponse =
                    await _httpClient.SendAsync(request);

                var responseContent =
                    await apiResponse.Content
                        .ReadAsStringAsync();

                if (!apiResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] =
                        $"Manager API request failed. HTTP Status: {(int)apiResponse.StatusCode}.";

                    return RedirectToAction(
                        nameof(Index),
                        new
                        {
                            section =
                                "mobileLookup"
                        });
                }

                // -------------------------------------------------
                // PARSE THE EXACT MANAGER API RESPONSE
                // -------------------------------------------------

                using var jsonDocument =
                    JsonDocument.Parse(responseContent);

                var root =
                    jsonDocument.RootElement;

                bool success =
                    root.TryGetProperty(
                        "Success",
                        out var successProperty)
                    &&
                    successProperty.ValueKind ==
                        JsonValueKind.True;

                string message =
                    root.TryGetProperty(
                        "Message",
                        out var messageProperty)
                        ? messageProperty.GetString() ?? string.Empty
                        : string.Empty;

                string errorMessages =
                    root.TryGetProperty(
                        "ErrorMessages",
                        out var errorMessagesProperty)
                        ? errorMessagesProperty.ValueKind == JsonValueKind.Null
                            ? "null"
                            : errorMessagesProperty.GetRawText()
                        : "null";

                string exception =
                    root.TryGetProperty(
                        "Exception",
                        out var exceptionProperty)
                        ? exceptionProperty.ValueKind == JsonValueKind.Null
                            ? "null"
                            : exceptionProperty.GetRawText()
                        : "null";

                string dataJson = "null";
                string personName = string.Empty;
                bool hasEmployeeData = false;

                if (root.TryGetProperty(
                        "Data",
                        out var dataProperty) &&
                    dataProperty.ValueKind != JsonValueKind.Null &&
                    dataProperty.ValueKind != JsonValueKind.Undefined)
                {
                    dataJson =
                        dataProperty.GetRawText();

                    if (dataProperty.ValueKind == JsonValueKind.Object &&
                        dataProperty.TryGetProperty(
                            "PersonName",
                            out var personNameProperty))
                    {
                        personName =
                            personNameProperty.GetString() ?? string.Empty;

                        hasEmployeeData =
                            !string.IsNullOrWhiteSpace(personName);
                    }
                }

                // -------------------------------------------------
                // CREATE RESPONSE FOR UI ONLY
                //
                // IMPORTANT:
                // We preserve the actual API response instead of
                // converting Data=null into a fake employee name.
                // NO DATABASE OPERATION HERE.
                // -------------------------------------------------

                var response =
                    new MobileApiResponseViewModel
                    {
                        Success =
                            success,

                        Message =
                            message,

                        ErrorMessages =
                            errorMessages,

                        Exception =
                            exception,

                        Data =
                            dataJson,

                        PersonName =
                            string.IsNullOrWhiteSpace(personName)
                                ? "null"
                                : personName,

                        HasEmployeeData =
                            hasEmployeeData,

                        Employee =
                            new MobileEmployeeResponseViewModel
                            {
                                MobileNo =
                                    mobileNo,

                                Name =
                                    personName
                            },

                        ResponseStatus =
                            apiResponse.IsSuccessStatusCode
                                ? "Success"
                                : "Failed",

                        SavedAt =
                            null
                    };

                var model =
                    new EmployeeManagementViewModel
                    {
                        Employees =
                            await _db.Employees
                                .AsNoTracking()
                                .ToListAsync(),

                        ActiveSection =
                            "mobileLookup",

                        MobileApiResponse =
                            response
                    };

                return View(
                    "Index",
                    model);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Unable to connect to Manager API.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }
            catch (JsonException)
            {
                TempData["ErrorMessage"] =
                    "Manager API returned an invalid response.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "An unexpected error occurred while fetching employee information.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }
        }


        // =====================================================
        // SAVE FETCHED MANAGER API RESPONSE
        // =====================================================
        //
        // IMPORTANT:
        // This is the ONLY action in this feature that writes
        // the Manager API response to the database.
        //
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMobileResponse(
            string mobileNo,
            string employeeName,
            string responseMessage,
            string responseStatus)
        {
            mobileNo =
                mobileNo?.Trim() ??
                string.Empty;

            employeeName =
                employeeName?.Trim() ??
                string.Empty;

            responseMessage =
                responseMessage?.Trim() ??
                string.Empty;

            responseStatus =
                responseStatus?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(mobileNo) ||
                mobileNo.Length != 10 ||
                !mobileNo.All(char.IsDigit) ||
                mobileNo[0] < '6' ||
                mobileNo[0] > '9')
            {
                TempData["ErrorMessage"] =
                    "Mobile number must be exactly 10 digits and start with 6-9.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }

            try
            {
                // -------------------------------------------------
                // EXECUTE EXISTING MYSQL STORED PROCEDURE
                // -------------------------------------------------

                var connection =
                    _db.Database.GetDbConnection();

                await _db.Database.OpenConnectionAsync();

                try
                {
                    await using var command =
                        connection.CreateCommand();

                    command.CommandText =
                        "CALL SaveEmployeeMobileApiResponse(@mobileNo, @employeeName, @responseMessage, @responseStatus);";

                    var mobileParameter =
                        command.CreateParameter();
                    mobileParameter.ParameterName =
                        "@mobileNo";
                    mobileParameter.Value =
                        mobileNo;

                    var employeeNameParameter =
                        command.CreateParameter();
                    employeeNameParameter.ParameterName =
                        "@employeeName";
                    employeeNameParameter.Value =
                        string.IsNullOrWhiteSpace(employeeName)
                            ? DBNull.Value
                            : employeeName;

                    var responseMessageParameter =
                        command.CreateParameter();
                    responseMessageParameter.ParameterName =
                        "@responseMessage";
                    responseMessageParameter.Value =
                        responseMessage;

                    var responseStatusParameter =
                        command.CreateParameter();
                    responseStatusParameter.ParameterName =
                        "@responseStatus";
                    responseStatusParameter.Value =
                        responseStatus;

                    command.Parameters.Add(
                        mobileParameter);
                    command.Parameters.Add(
                        employeeNameParameter);
                    command.Parameters.Add(
                        responseMessageParameter);
                    command.Parameters.Add(
                        responseStatusParameter);

                    await command.ExecuteNonQueryAsync();
                }
                finally
                {
                    await _db.Database.CloseConnectionAsync();
                }

                TempData["SuccessMessage"] =
                    "Manager API response has been saved successfully to the database.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] =
                    "Unable to save the Manager API response to the database.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "mobileLookup"
                    });
            }
        }


        // =====================================================
        // FIND EMPLOYEE FOR DELETE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> FindForDelete(
            int employeeId)
        {
            var employee =
                await _db.Employees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId);

            var model =
                new EmployeeManagementViewModel
                {
                    Employees =
                        await _db.Employees
                            .AsNoTracking()
                            .ToListAsync(),

                    SelectedEmployee =
                        employee,

                    ActiveSection =
                        "deleteEmployee"
                };

            if (employee == null)
            {
                model.Message =
                    $"Employee with ID {employeeId} was not found.";
            }

            return View(
                "Index",
                model);
        }


        // =====================================================
        // DELETE EMPLOYEE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int employeeId)
        {
            var employee =
                await _db.Employees
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] =
                    $"Employee with ID {employeeId} was not found.";

                return RedirectToAction(
                    nameof(Index));
            }

            bool hasTickets =
                await _db.Tickets.AnyAsync(
                    x =>
                        x.EmployeeId ==
                        employeeId);

            if (hasTickets)
            {
                TempData["ErrorMessage"] =
                    "This employee cannot be deleted because tickets are assigned to them.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        section =
                            "deleteEmployee"
                    });
            }

            _db.Employees.Remove(employee);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Employee {employeeId} deleted successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    section =
                        "employeeList"
                });
        }


        // =====================================================
        // EMPLOYEE ACTION TRACKING
        // =====================================================

        private async Task TrackEmployeeAction(
            int employeeId,
            string actionName)
        {
            var now =
                DateTime.Now;

            var actionAttempt =
                await _db.EmployeeActionAttempts
                    .FirstOrDefaultAsync(
                        x =>
                            x.EmployeeId ==
                            employeeId
                            &&
                            x.ActionName ==
                            actionName);

            if (actionAttempt == null)
            {
                actionAttempt =
                    new EmployeeActionAttempt
                    {
                        EmployeeId =
                            employeeId,

                        ActionName =
                            actionName,

                        AttemptCount =
                            1,

                        InsertedOn =
                            now,

                        UpdatedOn =
                            now,

                        Status =
                            0
                    };

                _db.EmployeeActionAttempts
                    .Add(actionAttempt);
            }
            else
            {
                actionAttempt.AttemptCount++;

                actionAttempt.UpdatedOn =
                    now;

                actionAttempt.Status =
                    0;
            }
        }


        // =====================================================
        // HELPER
        // =====================================================

        private async Task LoadLists(
            EmployeeManagementViewModel model)
        {
            model.Employees =
                await _db.Employees
                    .AsNoTracking()
                    .ToListAsync();

            model.AssignedTasks =
                await _db.Tickets
                    .AsNoTracking()
                    .ToListAsync();
        }
    }
}