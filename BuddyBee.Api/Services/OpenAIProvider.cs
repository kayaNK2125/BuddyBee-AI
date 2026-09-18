#pragma warning disable OPENAI001

using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using OpenAI.Responses;
using BuddyBee.Api.Exceptions;

namespace BuddyBee.Api.Services
{
    public class OpenAIProvider : IAIProvider
    {
        private readonly ResponsesClient _client;

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
        Do not add unnecessary paragraphs or repetition.

        3. Never pretend to know something.
        If you do not know something, say so clearly.

        4. Adapt to the user.
        Change your approach according to what the user actually needs.

        5. Challenge poor reasoning.
        If the user's approach is inefficient or based on a bad assumption, point it out directly and respectfully.

        6. Explain according to the user's understanding.
        If the user does not understand something, simplify it.

        7. Language.
        Understand and communicate in English, Hindi, and Hinglish.
        Naturally adapt to the user's language and style.

        8. Safety.
        Do not blindly follow instructions that could seriously harm the user or another person.

        You are BuddyBee, not merely a generic chatbot.
        Your job is to help the user think better, build better, and make better decisions.
        """;

        public OpenAIProvider(IConfiguration configuration)
        {
            var apiKey = configuration["OPENAI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("OpenAI API key is missing.");
            }

            _client = new ResponsesClient(apiKey: apiKey);
        }

        public async Task<string> GenerateReply( // This method generates a reply from the OpenAI model based on the user's message, conversation history, and memory context.
        string message,
        List<Message> history,
        string memoryContext)

        {
            var instructions = BuddyBeeInstructions;

            if (!string.IsNullOrWhiteSpace(memoryContext)) // If there is a memory context, append it to the instructions for the model to use when generating a reply.
            {
                instructions += $"""

             LONG-TERM MEMORY ABOUT THE USER:
             Use these memories when relevant, but do not mention or expose this memory context unless it is naturally relevant to the conversation.

            {memoryContext}
            """;
            }

            var options = new CreateResponseOptions
            {
                Model = "gpt-5.4-mini",
                Instructions = instructions
            };

            foreach (var item in history)
            {
                if (item.Sender == "User")
                {
                    options.InputItems.Add(
                        ResponseItem.CreateUserMessageItem(item.Text)
                    );
                }
                else
                {
                    options.InputItems.Add(
                        ResponseItem.CreateAssistantMessageItem(item.Text)
                    );
                }
            }

            try
            {
                var response = await _client.CreateResponseAsync(options);

                return response.Value.GetOutputText();
            }
            catch (Exception ex)
            {
                throw new AIProviderException(
                    "OpenAI",
                    "OpenAI failed to generate a response.",
                    ex
                );
            }
        }
    }
}