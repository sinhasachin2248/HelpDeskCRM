using HelpDeskCRM.API.Data;
using HelpDeskCRM.API.Models;

namespace HelpDeskCRM.API.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _environment;

        public ApiLoggingMiddleware(
            RequestDelegate next,
            IWebHostEnvironment environment)
        {
            _next = next;
            _environment = environment;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ApplicationDbContext db)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            var processName = context.Request.Path.ToString();
            var method = context.Request.Method;
            var dateTime = DateTime.Now;
            var responseValue = "success";

            try
            {
                await _next(context);

                responseValue = GetResponseValue(
                    context.Response.StatusCode);
            }
            catch
            {
                responseValue = "exception";
                throw;
            }
            finally
            {
                var status = responseValue == "exception"
                    ? StatusCodes.Status500InternalServerError
                    : context.Response.StatusCode;

                var log = new ApiLog
                {
                    ProcessName = processName,
                    Method = method,
                    LoggedAt = dateTime,
                    Status = status,
                    ResponseValue = responseValue
                };

                try
                {
                    db.ApiLogs.Add(log);
                    await db.SaveChangesAsync();
                }
                catch
                {
                    // Logging errors must not stop the API.
                }

                try
                {
                    var logsFolder = Path.Combine(
                        _environment.ContentRootPath,
                        "Logs");

                    Directory.CreateDirectory(logsFolder);

                    var logFile = Path.Combine(
                        logsFolder,
                        $"api-{DateTime.Now:yyyy-MM-dd}.log");

                    var logMessage =
                        $"Id: {log.Id} | " +
                        $"Process Name: {processName} | " +
                        $"Method: {method} | " +
                        $"Date Time: {dateTime:yyyy-MM-dd HH:mm:ss} | " +
                        $"Status: {status} | " +
                        $"Response Value: {responseValue}";

                    await File.AppendAllTextAsync(
                        logFile,
                        logMessage + Environment.NewLine);
                }
                catch
                {
                    // File logging errors must not stop the API.
                }
            }
        }

        private static string GetResponseValue(int statusCode)
        {
            if (statusCode is >= 200 and < 300)
                return "success";

            if (statusCode is >= 400 and < 500)
                return "error";

            if (statusCode >= 500)
                return "failure";

            return "success";
        }
    }
}