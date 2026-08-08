using BuddyBee.Api.DTOs;
using BuddyBee.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAIService _aiService; 

        public ChatController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ChatRequestDto request)
        {
            var reply = await _aiService.GenerateReply(request.Message);

            return Ok(new
            {
                reply = reply
            });
        }
    }
}


