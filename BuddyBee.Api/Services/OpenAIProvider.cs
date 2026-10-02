#pragma warning disable OPENAI001

using System.Text.Json;
using BuddyBee.Api.Exceptions;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using OpenAI.Responses;

namespace BuddyBee.Api.Services
{
    public class OpenAIProvider : IAIProvider
    {
        private ResponsesClient? _client;
        private readonly IConfiguration _configuration;
        private readonly ProviderKeyContext _keyContext;
        private readonly ToolRegistry _toolRegistry;

        private static readonly FunctionTool CalculatorToolDefinition =
            ResponseTool.CreateFunctionTool(
                functionName: "calculate",
                functionDescription: "Evaluates mathematical expressions exactly, including large numbers, fractions, decimals, percentages, powers, and scientific notation.",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "expression": {
                            "type": "string",
                            "description": "The complete mathematical expression to calculate."
                        }
                    },
                    "required": ["expression"],
                    "additionalProperties": false
                }
                """),
                strictModeEnabled: true
            );

        private static readonly FunctionTool TimeToolDefinition =
            ResponseTool.CreateFunctionTool(
                functionName: "get_time",
                functionDescription: "Returns the current time. Optionally specify an IANA timezone (e.g. 'Asia/Kolkata' for India, 'America/New_York' for US Eastern).",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "timezone": {
                            "type": ["string", "null"],
                            "description": "Optional IANA timezone identifier such as Asia/Kolkata or America/New_York. If omitted or null, returns UTC."
                        }
                    },
                    "required": ["timezone"],
                    "additionalProperties": false
                }
                """),
                strictModeEnabled: true
            );

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

        TOOL USAGE:

        - Use the calculate tool for mathematical calculations that require reliable or exact arithmetic.
        - Use the get_time tool when the user asks for the current time or date for an optional timezone.
        - Do not use a tool when it is unnecessary.
        - After receiving a tool result, use that result to answer the user's request.
        - Never claim that you calculated something with a tool or checked the time with a tool if you did not actually use it.

        You are BuddyBee, not merely a generic chatbot.
        Your job is to help the user think better, build better, and make better decisions.
        """;

        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public OpenAIProvider(
            IConfiguration configuration,
            ProviderKeyContext keyContext,
            ToolRegistry toolRegistry)
            : this(configuration, keyContext, toolRegistry, null)
        {
        }

        public OpenAIProvider(
            IConfiguration configuration,
            ProviderKeyContext keyContext,
            ToolRegistry toolRegistry,
            ResponsesClient? client)
        {
            _configuration = configuration;
            _keyContext = keyContext;
            _toolRegistry = toolRegistry;
            _client = client;
        }

        private ResponsesClient GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            var userKey = _keyContext.GetUserKey("OpenAI");
            if (!string.IsNullOrWhiteSpace(userKey))
            {
                _client = new ResponsesClient(apiKey: userKey);
                return _client;
            }

            if (_keyContext.HasUserKeys)
            {
                throw new AIProviderException("OpenAI", "No user OpenAI API key was provided.", new InvalidOperationException("USER_KEY_MISSING"));
            }

            var serverKey = _configuration["OPENAI_API_KEY"];
            if (string.IsNullOrWhiteSpace(serverKey))
            {
                throw new AIProviderException("OpenAI", "OpenAI provider key is not configured on the server.", new InvalidOperationException("SERVER_KEY_MISSING"));
            }

            _client = new ResponsesClient(apiKey: serverKey);
            return _client;
        }

        public async Task<string> GenerateReply(
            string message,
            List<Message> history,
            string memoryContext)
        {
            var instructions = BuddyBeeInstructions;

            if (!string.IsNullOrWhiteSpace(memoryContext))
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
                Instructions = instructions,
                Tools =
                {
                    CalculatorToolDefinition,
                    TimeToolDefinition
                }
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

            options.InputItems.Add(
                ResponseItem.CreateUserMessageItem(message)
            );

            try
            {
                var client = GetClient();
                const int maxToolRounds = 5;

                for (int round = 0; round < maxToolRounds; round++)
                {
                    var response = await client.CreateResponseAsync(options);
                    var result = response.Value;

                    foreach (var outputItem in result.OutputItems)
                    {
                        options.InputItems.Add(outputItem);
                    }

                    bool toolCalled = false;

                    foreach (var outputItem in result.OutputItems)
                    {
                        if (outputItem is not FunctionCallResponseItem functionCall)
                        {
                            continue;
                        }

                        toolCalled = true;

                        var tool = _toolRegistry.GetTool(functionCall.FunctionName)
                            ?? (functionCall.FunctionName == "time" ? _toolRegistry.GetTool("get_time") : null)
                            ?? (functionCall.FunctionName == "calculator" ? _toolRegistry.GetTool("calculate") : null);

                        if (tool == null)
                        {
                            options.InputItems.Add(
                                new FunctionCallOutputResponseItem(
                                    functionCall.CallId,
                                    $"Tool '{functionCall.FunctionName}' was not found."
                                )
                            );

                            continue;
                        }

                        var arguments = new Dictionary<string, object>();

                        try
                        {
                            using var doc = JsonDocument.Parse(functionCall.FunctionArguments);
                            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var prop in doc.RootElement.EnumerateObject())
                                {
                                    if (prop.Value.ValueKind == JsonValueKind.String)
                                    {
                                        arguments[prop.Name] = prop.Value.GetString()!;
                                    }
                                    else if (prop.Value.ValueKind != JsonValueKind.Null && prop.Value.ValueKind != JsonValueKind.Undefined)
                                    {
                                        arguments[prop.Name] = prop.Value.ToString();
                                    }
                                }
                            }
                        }
                        catch (JsonException)
                        {
                            arguments = new Dictionary<string, object>();
                        }

                        ToolResult toolResult = await tool.ExecuteAsync(arguments);

                        string toolOutput = toolResult.Success
                            ? toolResult.Output ?? string.Empty
                            : $"Tool execution failed: {toolResult.Error ?? "Unknown error."}";

                        options.InputItems.Add(
                            new FunctionCallOutputResponseItem(
                                functionCall.CallId,
                                toolOutput
                            )
                        );
                    }

                    if (!toolCalled)
                    {
                        return result.GetOutputText() ?? string.Empty;
                    }
                }

                throw new AIProviderException(
                    "OpenAI",
                    "OpenAI exceeded the maximum number of tool-calling rounds.",
                    new InvalidOperationException("Maximum tool-calling rounds exceeded.")
                );
            }
            catch (AIProviderException)
            {
                throw;
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