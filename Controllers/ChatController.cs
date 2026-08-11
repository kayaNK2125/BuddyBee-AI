using BuddyBee.Api.DTOs;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using BuddyBee.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static Google.Apis.Requests.BatchRequest;


namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly MongoDbService _mongoDbService;
        private readonly ToolRegistry _toolRegistry;

        public ChatController(
     IAIService aiService,
     MongoDbService mongoDbService,
     ToolRegistry toolRegistry)
        {
            _aiService = aiService;
            _mongoDbService = mongoDbService;
            _toolRegistry = toolRegistry;
        }

        [HttpGet("test-tool")]
        public async Task<IActionResult> TestTool()
        {
            var tool = _toolRegistry.GetTool("get_time");

            if (tool == null)
            {
                return NotFound("Tool not found.");
            }

            var result = await tool.ExecuteAsync(
                new Dictionary<string, object>());

            return Ok(result);
        }
        [HttpGet("test-calculator-tool")]
        public async Task<IActionResult> TestCalculator()
        {

            var tool = _toolRegistry.GetTool("calculate");

            var arguments = new Dictionary<string, object>
            {
                ["operation"] = "multiply",
                ["a"] = 12,
                ["b"] = 8
            };

            var result = await tool.ExecuteAsync(arguments);

            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post(ChatRequestDto request)
        {
            var userMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = request.ConversationId,
                Text = request.Message,
                Sender = "User",
                Time = DateTime.UtcNow
            };

            await _mongoDbService.SaveMessage(userMessage);
            
            var history = await _mongoDbService.GetConversationMessages(
            request.ConversationId
             );

            var response = await _aiService.GenerateReply(
    request.Message,
    history);

            var botMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = request.ConversationId,
                Text = response.Reply,
                Sender = "BuddyBee",
                Time = DateTime.UtcNow
            };

            await _mongoDbService.SaveMessage(botMessage);


            return Ok(new
{
    reply = response.Reply,
    provider = response.Provider
});
        }
    }
}


