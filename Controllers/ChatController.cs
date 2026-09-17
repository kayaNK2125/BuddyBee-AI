using System.Numerics;
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
        private readonly IConversationStore _conversationStore;
        private readonly ToolRegistry _toolRegistry;
        private readonly IMemoryService _memoryService; //memory service for saving and retrieving memories

        public ChatController(
    IAIService aiService,
    IConversationStore conversationStore,
    ToolRegistry toolRegistry,
    IMemoryService memoryService) //memory service injected into the controller
        {
            _aiService = aiService;
            _conversationStore = conversationStore;
            _toolRegistry = toolRegistry;
            _memoryService = memoryService; //memory service initialized
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

        [HttpGet("test-big-number")]
        public IActionResult TestBigNumber(
    [FromServices] MathEngine mathEngine)
        {
            var a = BigInteger.Parse(
                "999999999999999999999999999999999999999999999999999999");

            var b = BigInteger.Parse(
                "888888888888888888888888888888888888888888888888888888");

            var result = mathEngine.Multiply(a, b);

            return Ok(result.ToString());
        }

        [HttpGet("test-rational")]
        public IActionResult TestRational()
        {
            var first = new BigRational(2, 3);
            var second = new BigRational(5, 7);
                
            var result = first.Divide(second);

            return Ok(result.ToString());
        }

        [HttpGet("test-expression")]
        public IActionResult TestExpression(
    [FromServices] MathExpressionParser parser)
        {
            var result = parser.Evaluate(
                "1e-3 * 1e3"
            );

            return Ok(result.ToString());
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

            await _conversationStore.SaveMessage(userMessage);

            var history = await _conversationStore.GetConversationMessages(
            request.ConversationId
            );

            var memories = await _memoryService.GetMemories( //this will Retrieve memories for the user
            request.UserId
             );

            var memoryContext = string.Join( //this will create a string representation of the memories to be included in the AI prompt
             "\n",
             memories.Select(memory => $"- {memory.Text}")
             );

            var response = await _aiService.GenerateReply( // this will generate a reply from the AI service using the user's message, conversation history, and memory context
    request.Message,
    history.SkipLast(1).ToList(),   
    memoryContext);

            var botMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = request.ConversationId,
                Text = response.Reply,
                Sender = "BuddyBee",
                Time = DateTime.UtcNow
            };

            await _conversationStore.SaveMessage(botMessage);


            return Ok(new
{
    reply = response.Reply,
    provider = response.Provider
});
        }
    }
}


