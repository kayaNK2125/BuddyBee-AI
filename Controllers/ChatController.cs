using BuddyBee.Api.Models;
using Microsoft.AspNetCore.Mvc;
namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController:ControllerBase
    {
        [HttpPost]
        public IActionResult post()
        {
            return Ok(new
            {    
               reply= "Hello from BuddyBee"
            });
        }
    }
}


