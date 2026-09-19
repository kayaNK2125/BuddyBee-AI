using System;
using System.Linq;
using System.Threading.Tasks;
using BuddyBee.Api.DTOs;
using BuddyBee.Api.Exceptions;
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
        private readonly IMemoryService _memoryService;

        public ChatController(
            IAIService aiService,
            IConversationStore conversationStore,
            IMemoryService memoryService)
        {
            _aiService = aiService;
            _conversationStore = conversationStore;
            _memoryService = memoryService;
        }

        private static (int StatusCode, string Code, string UserMessage) ClassifyError(AIProviderException ex)
        {
            var innerMsg = ex.InnerException?.Message?.ToLowerInvariant() ?? "";
            var exMsg = ex.Message.ToLowerInvariant();
            var combined = $"{exMsg} {innerMsg}";

            // Missing credentials / configuration errors → 400 Bad Request
            if (combined.Contains("user_key_missing") ||
                combined.Contains("server_key_missing") ||
                combined.Contains("is missing") ||
                combined.Contains("not configured") ||
                combined.Contains("not provided"))
            {
                return (
                    400,
                    "API_KEY_REQUIRED",
                    "No AI provider key configured. Please enter your Gemini or OpenAI API key in Settings."
                );
            }

            // Invalid / Unauthorized credentials → 401 Unauthorized
            if (combined.Contains("unauthorized") ||
                combined.Contains("api key") ||
                combined.Contains("api_key_invalid") ||
                combined.Contains("forbidden") ||
                combined.Contains("401") ||
                combined.Contains("403"))
            {
                return (
                    401,
                    "INVALID_API_KEY",
                    $"The provided {ex.Provider} API key is invalid or unauthorized. Please verify your key in Settings."
                );
            }

            // Quota / Rate limit errors → 429 Too Many Requests
            if (combined.Contains("quota") ||
                combined.Contains("rate limit") ||
                combined.Contains("resource_exhausted") ||
                combined.Contains("429"))
            {
                return (
                    429,
                    "RATE_LIMITED",
                    $"{ex.Provider} quota exceeded or rate limit reached. Please try again shortly or configure another provider in Settings."
                );
            }

            // Transient / provider outage errors → 503 Service Unavailable
            return (
                503,
                "PROVIDER_UNAVAILABLE",
                $"{ex.Provider} is currently unavailable. Please try again in a few moments."
            );
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

            var memories = await _memoryService.GetMemories(
                request.UserId
            );

            var memoryContext = string.Join(
                "\n",
                memories.Select(memory => $"- {memory.Text}")
            );

            AIResponseDto response;
            try
            {
                response = await _aiService.GenerateReply(
                    request.Message,
                    history.SkipLast(1).ToList(),
                    memoryContext
                );
            }
            catch (AIProviderException ex)
            {
                var errorInfo = ClassifyError(ex);
                return StatusCode(errorInfo.StatusCode, new
                {
                    error = errorInfo.Code,
                    message = errorInfo.UserMessage,
                    provider = ex.Provider,
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "INTERNAL_ERROR",
                    message = "An unexpected error occurred while generating a response.",
                    details = ex.Message
                });
            }

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
