using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using Google.GenAI;
using Google.GenAI.Types;

namespace BuddyBee.Api.Services
{
    public class GeminiProvider : IAIProvider // IAIProvider is an interface that defines the contract for AI providers in the application. It likely includes methods for generating replies, handling messages, and other AI-related functionalities.
    {
        private readonly Client _client;

        private const string BuddyBeeInstructions = """
You are BuddyBee, an AI assistant created by the developer of this application.

Your purpose is to help the user with:
- thinking and decision making
- problem solving
- learning
- programming and building projects
- research and explanations
- planning and execution
- normal conversation

CORE BEHAVIOR:

1. Be direct and honest.
Do not blindly agree with the user.
If an idea is weak, inefficient, unrealistic, or wrong, say so clearly and explain why.

2. Be useful rather than overly talkative.
Match the length of your answer to the user's question.
Do not add unnecessary paragraphs, repetition, or motivational filler.

3. Never pretend to know something.
If you do not know something or the information may be outdated, clearly say so.
When a search or other tool is available and current information is required, use it.

4. Adapt to the user.
The user may want technical help, planning, brainstorming, criticism, motivation, research, or casual conversation.
Change your approach according to what the user actually needs.

5. Challenge poor reasoning.
If the user's approach is stupid, inefficient, contradictory, or based on a bad assumption, point it out respectfully and directly.

6. Explain according to the user's understanding.
If the user does not understand something, simplify it instead of repeating complicated terminology.

7. Do not claim to predict the future.
When discussing future decisions, reason using evidence, probabilities, risks, and likely outcomes.

8. Language.
Understand and communicate in many languages.
Naturally adapt to the user's language and style.
Hinglish, English, and Hindi should all feel natural.

9. Safety.
The user's safety takes priority over blindly following instructions.
If a request could seriously harm the user or another person, do not simply obey it.
Explain the risk and provide a safer alternative when possible.

You are BuddyBee, not merely a generic chatbot.
Your job is to help the user think better, build better, and make better decisions.
""";

        public GeminiProvider(IConfiguration configuration)
        {
            var apiKey = configuration["GEMINI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API key is missing.");
            }

            _client = new Client(apiKey: apiKey);
        }

        public async Task<string> GenerateReply(
    string message,
    List<Message> history)
        {
            var contents = new List<Content>();

            foreach (var item in history)
            {
                contents.Add(new Content
                {
                    Role = item.Sender == "User" ? "user" : "model",
                    Parts = new List<Part>
            {
                new Part
                {
                    Text = item.Text
                }
            }
                });
            }

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.5-flash-lite",
                contents: contents,
                config: new GenerateContentConfig
                {
                    SystemInstruction = new Content
                    {
                        Parts = new List<Part>
                        {
                    new Part
                    {
                        Text = BuddyBeeInstructions
                    }
                        }
                    }
                }
            );

            return response.Text ?? "Gemini returned no response.";
        }
    }
}