using BuddyBee.Api.Interfaces;
using Google.GenAI;

namespace BuddyBee.Api.Services
{
    public class AIService : IAIService
    {
        private readonly Client _client;

        public AIService(IConfiguration configuration)
        {
            var apiKey = configuration["GEMINI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API key is missing.");
            }

            _client = new Client(apiKey: apiKey);
        }

        public async Task<string> GenerateReply(string message)
        {
            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.5-flash",
                contents: message
            );

            return response.Text ?? "Gemini returned no response.";
        }
    }
}