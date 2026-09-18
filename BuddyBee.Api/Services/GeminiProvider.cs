using BuddyBee.Api.Exceptions;
using BuddyBee.Api.Interfaces;
using BuddyBee.Api.Models;
using BuddyBee.Api.Tools;
using Google.GenAI;
using Google.GenAI.Types;

namespace BuddyBee.Api.Services
{
    public class GeminiProvider : IAIProvider
    {
        private readonly Client _client;
        private readonly ToolRegistry _toolRegistry;

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
When a tool is available and it is required, use it.

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

TOOL USAGE:

- Use the calculate tool for mathematical calculations that require reliable or exact arithmetic.
- Do not use a tool when it is unnecessary.

You are BuddyBee, not merely a generic chatbot.
Your job is to help the user think better, build better, and make better decisions.
""";

        public GeminiProvider(
            IConfiguration configuration,
            ToolRegistry toolRegistry)
        {
            var apiKey = configuration["GEMINI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API key is missing.");
            }

            _client = new Client(apiKey: apiKey);
            _toolRegistry = toolRegistry;
        }

        public async Task<string> GenerateReply( // Implement the IAIProvider interface
        string message,
        List<Message> history,
        string memoryContext)
        {
            var contents = new List<Content>();

            // =====================================================
            // ADD PREVIOUS CONVERSATION HISTORY
            // =====================================================

            foreach (var item in history)
            {
                contents.Add(new Content
                {
                    Role = item.Sender == "User"
                        ? "user"
                        : "model",

                    Parts = new List<Part>
                    {
                        new Part
                        {
                            Text = item.Text
                        }
                    }
                });
            }



            // =====================================================
            // ADD LONG-TERM MEMORY
            // =====================================================

            //we are adding the memory context as a user message in the conversation, but we are instructing Gemini to use it only when relevant and not to expose it unless it is naturally relevant to the conversation.
            if (!string.IsNullOrWhiteSpace(memoryContext))
            {
                contents.Add(new Content
                {
                    Role = "user",
                    Parts = new List<Part>
        {
            new Part
            {
                Text = $"""
                The following are long-term memories about the user.
                Use them when relevant, but do not mention or expose this memory context unless it is naturally relevant to the conversation.

                {memoryContext}
                """
            }
        }
                });
            }


            // =====================================================
            // ADD CURRENT USER MESSAGE
            // =====================================================

            contents.Add(new Content
            {
                Role = "user",

                Parts = new List<Part>
                {
                    new Part
                    {
                        Text = message
                    }
                }
            });

            // =====================================================
            // CALCULATOR FUNCTION DECLARATION
            // =====================================================

            var calculatorDeclaration = new FunctionDeclaration
            {
                Name = "calculate",

                Description =
                    "Evaluates a mathematical expression exactly. " +
                    "Use this for arithmetic, large numbers, fractions, " +
                    "decimals, percentages, powers, and scientific notation.",

                Parameters = new Schema
                {
                    Type = Google.GenAI.Types.Type.Object,

                    Properties = new Dictionary<string, Schema>
                    {
                        ["expression"] = new Schema
                        {
                            Type = Google.GenAI.Types.Type.String,

                            Description =
                                "The complete mathematical expression to calculate."
                        }
                    },

                    Required = new List<string>
                    {
                        "expression"
                    }
                }
            };


            // =====================================================
            // TIME FUNCTION DECLARATION
            // =====================================================

            var timeDeclaration = new FunctionDeclaration
            {
                Name = "get_time",

                Description =
                    "Returns the current time. Optionally specify an IANA timezone (e.g. 'Asia/Kolkata' for India, 'America/New_York' for US Eastern).",

                Parameters = new Schema
                {
                    Type = Google.GenAI.Types.Type.Object,

                    Properties = new Dictionary<string, Schema>
                    {
                        ["timezone"] = new Schema
                        {
                            Type = Google.GenAI.Types.Type.String,

                            Description =
                                "Optional IANA timezone identifier. If omitted, returns UTC."
                        }
                    },

                    Required = new List<string>
                    {
                    }
                }
            };

            // =====================================================
            // REGISTERED GEMINI TOOLS
            // =====================================================

            var tools = new List<Tool>
            {
                new Tool
                {
                    FunctionDeclarations =
                        new List<FunctionDeclaration>
                        {
                            calculatorDeclaration,
                            timeDeclaration
                        }
                }
            };

            try
            {
                // =================================================
                // FIRST GEMINI REQUEST
                // =================================================
                //
                // AUTO means Gemini decides whether it needs a tool.
                //
                // Normal question → no tool
                // Math            → calculate
                //

                Console.WriteLine(">>> BEFORE GEMINI API CALL");

             var response =
    await _client.Models.GenerateContentAsync(
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
            },

            Tools = tools,

            ToolConfig = new ToolConfig
            {
                FunctionCallingConfig =
                    new FunctionCallingConfig
                    {
                        Mode =
                            FunctionCallingConfigMode.Auto
                    }
            }
        });

                Console.WriteLine(">>> AFTER GEMINI API CALL");

                // =================================================
                // TOOL LOOP
                // =================================================
                //
                // Gemini may:
                //
                // 1. Answer directly
                //
                // OR
                //
                // 2. Request one or more tools.
                //
                // We execute the requested tools through ToolRegistry.
                //

                const int maxToolRounds = 5;

                for (int round = 0;
                     round < maxToolRounds;
                     round++)
                {
                    var functionCalls = response.FunctionCalls;

                    // -------------------------------------------------
                    // No tool requested.
                    // Gemini has produced the final answer.
                    // -------------------------------------------------

                    if (functionCalls == null ||
                        functionCalls.Count == 0)
                    {
                        return response.Text
                            ?? "Gemini returned no response.";
                    }

                    Console.WriteLine(
                        $"Gemini requested {functionCalls.Count} tool call(s).");

                    // -------------------------------------------------
                    // Add Gemini's function-call message.
                    // -------------------------------------------------

                    if (response.Parts != null)
                    {
                        contents.Add(new Content
                        {
                            Role = "model",
                            Parts = response.Parts
                        });
                    }

                    // -------------------------------------------------
                    // Execute every requested tool.
                    // -------------------------------------------------

                    var functionResponseParts = new List<Part>();

                    foreach (var functionCall in functionCalls)
                    {
                        Console.WriteLine(
                            $"Gemini requested tool: {functionCall.Name}");

                        // ---------------------------------------------
                        // Find the tool in ToolRegistry.
                        // ---------------------------------------------

                        if (string.IsNullOrWhiteSpace(functionCall.Name))
                        {
                            throw new InvalidOperationException(
                                "Gemini returned a function call without a tool name.");
                        }

                        var tool =
                            _toolRegistry.GetTool(functionCall.Name);

                        if (tool == null)
                        {
                            Console.WriteLine(
                                $"Tool not found: {functionCall.Name}");

                            var errorResponse =
                                new FunctionResponse
                                {
                                    Name = functionCall.Name,

                                    Id = functionCall.Id,

                                    Response =
                                        new Dictionary<string, object>
                                        {
                                            ["error"] =
                                                $"Tool '{functionCall.Name}' was not found."
                                        }
                                };

                            functionResponseParts.Add(
                                new Part
                                {
                                    FunctionResponse =
                                        errorResponse
                                });

                            continue;
                        }

                        // ---------------------------------------------
                        // Convert Gemini arguments to our tool format.
                        // ---------------------------------------------

                        var arguments =
                            new Dictionary<string, object>();

                        if (functionCall.Args != null)
                        {
                            foreach (var argument in functionCall.Args)
                            {
                                arguments[argument.Key] =
                                    argument.Value;
                            }
                        }

                        // ---------------------------------------------
                        // Execute the actual BuddyBee tool.
                        // ---------------------------------------------

                        var toolResult =
                            await tool.ExecuteAsync(arguments);

                        Console.WriteLine(
                            $"Tool result: {toolResult.Output}");

                        // ---------------------------------------------
                        // Build FunctionResponse.
                        // ---------------------------------------------

                        var responseData =
                            new Dictionary<string, object>();

                        if (toolResult.Success)
                        {
                            responseData["output"] =
                                toolResult.Output ?? "";
                        }
                        else
                        {
                            responseData["error"] =
                                toolResult.Error ?? "Tool failed.";
                        }

                        var functionResponse =
                            new FunctionResponse
                            {
                                Name = functionCall.Name,

                                Id = functionCall.Id,

                                Response = responseData
                            };

                        functionResponseParts.Add(
                            new Part
                            {
                                FunctionResponse =
                                    functionResponse
                            });
                    }

                    // =================================================
                    // SEND ALL TOOL RESULTS BACK TO GEMINI
                    // =================================================

                    contents.Add(new Content
                    {
                        Role = "user",

                        Parts = functionResponseParts
                    });

                    Console.WriteLine(
                        ">>> SENDING FUNCTION RESPONSES BACK TO GEMINI");

                    // =================================================
                    // SECOND / NEXT GEMINI REQUEST
                    // =================================================
                    //
                    // Auto again.
                    //
                    // Usually Gemini now produces the final answer.
                    // If it needs another tool, the loop handles it.
                    //

                    response =
    await _client.Models.GenerateContentAsync(
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
            },

            Tools = tools,

            ToolConfig = new ToolConfig
            {
                FunctionCallingConfig =
                    new FunctionCallingConfig
                    {
                        Mode =
                            FunctionCallingConfigMode.Auto
                    }
            }
        });
                }

                return "Tool execution limit reached.";

            }
            catch (Exception ex)
            {
                throw new AIProviderException(
                    "Gemini",
                    "Gemini failed to generate a response.",
                    ex
                );
            }
        }
    }
}