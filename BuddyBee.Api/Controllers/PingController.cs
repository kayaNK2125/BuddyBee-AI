using Microsoft.AspNetCore.Mvc;

namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "BuddyBee API is alive 🚀"
            });
        }
    }
}