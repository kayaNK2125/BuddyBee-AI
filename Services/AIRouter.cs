// Which AI provider should use its job (trafic controler)

using BuddyBee.Api.DTOs;
using BuddyBee.Api.Exceptions;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using BuddyBee.Api.Provider.Services;

namespace BuddyBee.Api.Services
{
    public class AIRouter : IAIService
    {
        private readonly GeminiProvider _gemini;
        private readonly OpenAIProvider _openAI;

        private bool ShouldFallback(AIProviderException ex)
        {
            if (ex.InnerException == null)
                return true;

            var message = ex.InnerException.Message.ToLower();

            // Temporary problems → fallback
            if (message.Contains("quota") ||
                message.Contains("rate limit") ||
                message.Contains("timeout") ||
                message.Contains("timed out") ||
                message.Contains("connection") ||
                message.Contains("network"))
            {
                return true;
            }

            // Configuration / request problems → don't hide them
            if (message.Contains("api key") ||
                message.Contains("unauthorized") ||
                message.Contains("invalid argument") ||
                message.Contains("bad request"))
            {
                return false;
            }

            // Unknown provider failure → fallback for now
            return true;
        }

        public AIRouter(
            GeminiProvider gemini,
            OpenAIProvider openAI)
        {
            _gemini = gemini;
            _openAI = openAI;
        }

        public async Task<AIResponseDto> GenerateReply(
            string message,
            List<Message> history)
        {
            try
            {
                var reply = await _gemini.GenerateReply(message, history);

                return new AIResponseDto
                {
                    Reply = reply,
                    Provider = "Gemini"
                };
            }
            catch (AIProviderException ex)
            {
                Console.WriteLine(
                    $"[AI Router] {ex.Provider} failed: {ex.Message}"
                );

                if (!ShouldFallback(ex))
                {
                    throw;
                }

                var reply = await _openAI.GenerateReply(message, history);

                return new AIResponseDto
                {
                    Reply = reply,
                    Provider = "OpenAI"
                };
            }


        }
    }
}