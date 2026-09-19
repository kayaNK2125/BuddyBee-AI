// Which AI provider should use its job (traffic controller)

using BuddyBee.Api.DTOs;
using BuddyBee.Api.Exceptions;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using Microsoft.Extensions.Configuration;

namespace BuddyBee.Api.Services
{
    public class AIRouter : IAIService
    {
        private readonly GeminiProvider _gemini;
        private readonly OpenAIProvider _openAI;
        private readonly ProviderKeyContext _keyContext;
        private readonly IConfiguration _configuration;

        public AIRouter(
            GeminiProvider gemini,
            OpenAIProvider openAI,
            ProviderKeyContext keyContext,
            IConfiguration configuration)
        {
            _gemini = gemini;
            _openAI = openAI;
            _keyContext = keyContext;
            _configuration = configuration;
        }

        private static bool ShouldFallback(AIProviderException ex)
        {
            if (ex.InnerException == null)
                return false;

            var message = ex.InnerException.Message.ToLower();

            // Configuration / authentication / key errors → NEVER fallback
            if (message.Contains("api key") ||
                message.Contains("unauthorized") ||
                message.Contains("forbidden") ||
                message.Contains("invalid argument") ||
                message.Contains("bad request") ||
                message.Contains("api_key_invalid") ||
                message.Contains("user_key_missing") ||
                message.Contains("server_key_missing") ||
                message.Contains("401") ||
                message.Contains("403"))
            {
                return false;
            }

            // Temporary / transient problems → fallback allowed
            if (message.Contains("quota") ||
                message.Contains("rate limit") ||
                message.Contains("resource_exhausted") ||
                message.Contains("429") ||
                message.Contains("timeout") ||
                message.Contains("timed out") ||
                message.Contains("connection") ||
                message.Contains("network") ||
                message.Contains("503") ||
                message.Contains("unavailable"))
            {
                return true;
            }

            return false;
        }

        public async Task<AIResponseDto> GenerateReply(
            string message,
            List<Message> history,
            string memoryContext)
        {
            bool hasUserGemini = !string.IsNullOrWhiteSpace(_keyContext.GetUserKey("Gemini"));
            bool hasUserOpenAI = !string.IsNullOrWhiteSpace(_keyContext.GetUserKey("OpenAI"));

            if (_keyContext.HasUserKeys)
            {
                // =====================================================================
                // USER-SUPPLIED KEY MODE
                // INVARIANT: Developer/server keys must NEVER be used as a substitute
                // for a missing user provider key.
                // =====================================================================

                if (hasUserGemini && !hasUserOpenAI)
                {
                    // User supplied Gemini ONLY → Gemini user key only; never developer OpenAI
                    var reply = await _gemini.GenerateReply(message, history, memoryContext);
                    return new AIResponseDto
                    {
                        Reply = reply,
                        Provider = "Gemini"
                    };
                }

                if (!hasUserGemini && hasUserOpenAI)
                {
                    // User supplied OpenAI ONLY → OpenAI user key only; never developer Gemini
                    var reply = await _openAI.GenerateReply(message, history, memoryContext);
                    return new AIResponseDto
                    {
                        Reply = reply,
                        Provider = "OpenAI"
                    };
                }

                if (hasUserGemini && hasUserOpenAI)
                {
                    // User supplied BOTH keys → Gemini first, fallback to user OpenAI on transient error
                    try
                    {
                        var reply = await _gemini.GenerateReply(message, history, memoryContext);
                        return new AIResponseDto
                        {
                            Reply = reply,
                            Provider = "Gemini"
                        };
                    }
                    catch (AIProviderException ex)
                    {
                        Console.WriteLine($"[AI Router] User Gemini failed: {ex.Message}");

                        if (!ShouldFallback(ex))
                        {
                            throw;
                        }

                        Console.WriteLine("[AI Router] Falling back to user OpenAI...");
                        var reply = await _openAI.GenerateReply(message, history, memoryContext);
                        return new AIResponseDto
                        {
                            Reply = reply,
                            Provider = "OpenAI"
                        };
                    }
                }

                throw new AIProviderException(
                    "AIRouter",
                    "No valid user API key provided.",
                    new InvalidOperationException("USER_KEY_MISSING")
                );
            }
            else
            {
                // =====================================================================
                // MANAGED / DEVELOPER MODE (no user keys supplied)
                // Existing developer/local configuration may be used.
                // =====================================================================

                bool hasDevGemini = !string.IsNullOrWhiteSpace(_configuration["GEMINI_API_KEY"]);
                bool hasDevOpenAI = !string.IsNullOrWhiteSpace(_configuration["OPENAI_API_KEY"]);

                if (!hasDevGemini && !hasDevOpenAI)
                {
                    throw new AIProviderException(
                        "AIRouter",
                        "No AI provider key is configured on the server. Please enter your API key in Settings.",
                        new InvalidOperationException("SERVER_KEY_MISSING")
                    );
                }

                if (hasDevGemini)
                {
                    try
                    {
                        var reply = await _gemini.GenerateReply(message, history, memoryContext);
                        return new AIResponseDto
                        {
                            Reply = reply,
                            Provider = "Gemini"
                        };
                    }
                    catch (AIProviderException ex)
                    {
                        Console.WriteLine($"[AI Router] Managed Gemini failed: {ex.Message}");

                        if (hasDevOpenAI && ShouldFallback(ex))
                        {
                            Console.WriteLine("[AI Router] Falling back to managed OpenAI...");
                            var reply = await _openAI.GenerateReply(message, history, memoryContext);
                            return new AIResponseDto
                            {
                                Reply = reply,
                                Provider = "OpenAI"
                            };
                        }

                        throw;
                    }
                }
                else
                {
                    var reply = await _openAI.GenerateReply(message, history, memoryContext);
                    return new AIResponseDto
                    {
                        Reply = reply,
                        Provider = "OpenAI"
                    };
                }
            }
        }
    }
}