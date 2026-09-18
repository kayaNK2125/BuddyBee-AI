using BuddyBee.Api.DTOs;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BuddyBee.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly IConversationStore _conversationStore;
        private readonly IMemoryService _memoryService; // memory service for saving and retrieving memories

        public ChatController(
            IAIService aiService,
            IConversationStore conversationStore,
            IMemoryService memoryService)
        {
            _aiService = aiService;
            _conversationStore = conversationStore;
            _memoryService = memoryService;
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


