using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sheleni_API.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sheleni_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public ServicesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("all_services")]
        public async Task<IActionResult> GetAllServiceNames()
        {
            try
            {
                var serviceNames = await _dbContext.Services
                    .Select(s => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ServiceName.ToLower()))
                    .ToListAsync();

                if (serviceNames == null || serviceNames.Count == 0)
                {
                    return NotFound("No service names found.");
                }

                return Ok(serviceNames);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
