using BuddyBee.Api.DTOs;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using BuddyBee.Api.Provider.Services;

namespace BuddyBee.Api.Services
{
    public class AIRouter : IAIService
    {
        private readonly GeminiProvider _gemini;
        private readonly OpenAIProvider _openAI;

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
            catch
            {
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