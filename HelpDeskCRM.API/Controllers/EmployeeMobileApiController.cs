using HelpDeskCRM.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HelpDeskCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeMobileApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmployeeMobileApiController(
            ApplicationDbContext db,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // =====================================================
        // FIND EMPLOYEE BY MOBILE NUMBER
        // =====================================================

        [HttpPost("SaveMobile")]
        public async Task<IActionResult> SaveMobile(
            [FromBody] SaveMobileRequest request)
        {
            try
            {
                // =================================================
                // 1. VALIDATE MOBILE NUMBER
                // =================================================

                if (string.IsNullOrWhiteSpace(request.NewMobileNo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mobile number is required."
                    });
                }


                string mobileNo =
                    request.NewMobileNo.Trim();


                if (mobileNo.Length != 10 ||
                    !mobileNo.All(char.IsDigit) ||
                    mobileNo[0] < '6' ||
                    mobileNo[0] > '9')
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Mobile number must be exactly 10 digits and start with 6-9."
                    });
                }


                // =================================================
                // 2. GET MANAGER API CONFIGURATION
                // =================================================

                string? managerApiUrl =
                    _configuration[
                        "ManagerApi:MobileLookupUrl"];


                string? authorization =
                    _configuration[
                        "ManagerApi:Authorization"];


                if (string.IsNullOrWhiteSpace(managerApiUrl))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Manager API URL is not configured."
                    });
                }


                if (string.IsNullOrWhiteSpace(authorization))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Manager API authorization is not configured."
                    });
                }


                // =================================================
                // 3. CALL MANAGER API
                // =================================================

                var httpClient =
                    _httpClientFactory.CreateClient();


                httpClient.DefaultRequestHeaders.Clear();


                httpClient.DefaultRequestHeaders
                    .TryAddWithoutValidation(
                        "Authorization",
                        authorization);


                var managerRequest =
                    new ManagerMobileRequest
                    {
                        MobileNo =
                            mobileNo
                    };


                string requestJson =
                    JsonSerializer.Serialize(
                        managerRequest);


                using var content =
                    new StringContent(
                        requestJson,
                        Encoding.UTF8,
                        "application/json");


                HttpResponseMessage managerResponse;


                try
                {
                    managerResponse =
                        await httpClient.PostAsync(
                            managerApiUrl,
                            content);
                }
                catch (HttpRequestException ex)
                {
                    return StatusCode(502, new
                    {
                        success = false,
                        message =
                            "Unable to connect to Manager API.",
                        error =
                            ex.Message
                    });
                }


                string managerResponseJson =
                    await managerResponse.Content
                        .ReadAsStringAsync();


                // =================================================
                // 4. READ MANAGER API RESPONSE
                // =================================================

                ManagerMobileResponse?
                    managerApiResponse;


                try
                {
                    managerApiResponse =
                        JsonSerializer.Deserialize
                            <ManagerMobileResponse>(
                                managerResponseJson,
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive =
                                        true
                                });
                }
                catch
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Unable to read Manager API response."
                    });
                }


                if (managerApiResponse == null)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Manager API returned an empty response."
                    });
                }


                // =================================================
                // 5. GET ACTUAL PERSON NAME
                // =================================================

                string personName =
                    managerApiResponse
                        .Data?
                        .PersonName
                    ?? string.Empty;


                string responseMessage =
                    managerApiResponse.Message
                    ?? string.Empty;


                string responseStatus =
                    managerApiResponse.Success
                        ? "Success"
                        : "Failed";


                // =================================================
                // 6. SAVE MANAGER API RESPONSE
                //    THROUGH STORED PROCEDURE
                // =================================================

                var connection =
                    _db.Database.GetDbConnection();


                if (connection.State !=
                    ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }


                using var command =
                    new MySqlCommand(
                        "SaveEmployeeMobileApiResponse",
                        (MySqlConnection)connection);


                command.CommandType =
                    CommandType.StoredProcedure;


                command.Parameters.AddWithValue(
                    "p_mobile_no",
                    mobileNo);


                command.Parameters.AddWithValue(
                    "p_employee_name",
                    string.IsNullOrWhiteSpace(personName)
                        ? DBNull.Value
                        : personName);


                command.Parameters.AddWithValue(
                    "p_response_message",
                    string.IsNullOrWhiteSpace(responseMessage)
                        ? DBNull.Value
                        : responseMessage);


                command.Parameters.AddWithValue(
                    "p_response_status",
                    responseStatus);


                using var reader =
                    await command.ExecuteReaderAsync();


                // =================================================
                // 7. READ SAVED DATABASE RESPONSE
                // =================================================

                if (!await reader.ReadAsync())
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message =
                            "Stored procedure did not return any result."
                    });
                }


                int logId =
                    Convert.ToInt32(
                        reader["id"]);


                string savedMobileNo =
                    reader["mobile_no"]?
                        .ToString()
                    ?? mobileNo;


                string savedEmployeeName =
                    reader["employee_name"]?
                        .ToString()
                    ?? string.Empty;


                string savedResponseMessage =
                    reader["response_message"]?
                        .ToString()
                    ?? string.Empty;


                string savedResponseStatus =
                    reader["response_status"]?
                        .ToString()
                    ?? string.Empty;


                DateTime createdAt =
                    Convert.ToDateTime(
                        reader["created_at"]);


                // =================================================
                // 8. MANAGER API FAILED
                // =================================================

                if (!managerApiResponse.Success)
                {
                    return BadRequest(new
                    {
                        success = false,

                        message =
                            savedResponseMessage,

                        mobileNo =
                            savedMobileNo,

                        responseStatus =
                            savedResponseStatus,

                        logId =
                            logId,

                        savedAt =
                            createdAt
                    });
                }


                // =================================================
                // 9. PERSON NAME NOT RETURNED
                // =================================================

                if (string.IsNullOrWhiteSpace(savedEmployeeName))
                {
                    return BadRequest(new
                    {
                        success = false,

                        message =
                            "Manager API did not return a person name.",

                        mobileNo =
                            savedMobileNo,

                        responseStatus =
                            savedResponseStatus,

                        logId =
                            logId,

                        savedAt =
                            createdAt,

                        managerApiRawResponse =
                            managerResponseJson
                    });
                }


                // =================================================
                // 10. RETURN SUCCESSFUL RESULT
                // =================================================

                return Ok(new
                {
                    success = true,

                    message =
                        savedResponseMessage,

                    employee = new
                    {
                        mobileNo =
                            savedMobileNo,

                        name =
                            savedEmployeeName
                    },

                    responseStatus =
                        savedResponseStatus,

                    managerApi = new
                    {
                        success =
                            managerApiResponse.Success,

                        message =
                            managerApiResponse.Message,

                        personName =
                            personName
                    },

                    logId =
                        logId,

                    savedAt =
                        createdAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,

                    message =
                        "An error occurred while processing the mobile number.",

                    error =
                        ex.Message
                });
            }
        }


        // =====================================================
        // REQUEST MODEL
        // =====================================================

        public class SaveMobileRequest
        {
            public string NewMobileNo { get; set; }
                = string.Empty;
        }


        // =====================================================
        // MANAGER API REQUEST
        // =====================================================

        public class ManagerMobileRequest
        {
            [JsonPropertyName("MobileNo")]
            public string MobileNo { get; set; }
                = string.Empty;
        }


        // =====================================================
        // MANAGER API RESPONSE
        // =====================================================

        public class ManagerMobileResponse
        {
            public string? Message { get; set; }

            public object? ErrorMessages { get; set; }

            public object? Exception { get; set; }

            public ManagerMobileData? Data { get; set; }

            public bool Success { get; set; }
        }


        public class ManagerMobileData
        {
            public string? PersonName { get; set; }
        }
    }
}