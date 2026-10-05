using HelpDeskCRM.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDeskCRM.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class ApiTestController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ApiTestController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> TestApi()
        {
            bool databaseConnected =
                await _db.Database.CanConnectAsync();

            return Ok(new
            {
                message = "API is working",
                databaseConnected = databaseConnected,
                time = DateTime.Now
            });
        }
    }
}