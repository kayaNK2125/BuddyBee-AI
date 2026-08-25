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
        private readonly CalculatorTool _calculator;

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

        public GeminiProvider(
            IConfiguration configuration,
            CalculatorTool calculator)
        {
            var apiKey = configuration["GEMINI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("Gemini API key is missing.");
            }

            _client = new Client(apiKey: apiKey);
            _calculator = calculator;
        }

        public async Task<string> GenerateReply(
            string message,
            List<Message> history)
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
            // ADD CURRENT USER MESSAGE
            // =====================================================
            //
            // GenerateReply receives the current message separately
            // from history, so we must explicitly add it.
            //
            // Without this, Gemini may never see the user's
            // current request.
            //

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
            // CALCULATOR TOOL DECLARATION
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
                                "The complete mathematical expression to calculate. " +
                                "Example: ((847293 * 928374) - 192837) / 17"
                        }
                    },

                    Required = new List<string>
                    {
                        "expression"
                    }
                }
            };

            try
            {
                // =================================================
                // SEND REQUEST TO GEMINI
                // =================================================

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

                            Tools = new List<Tool>
    {
        new Tool
        {
            FunctionDeclarations = new List<FunctionDeclaration>
            {
                calculatorDeclaration
            }
        }
    },

                            ToolConfig = new ToolConfig
                            {
                                FunctionCallingConfig = new FunctionCallingConfig
                                {
                                    Mode = FunctionCallingConfigMode.Any,

                                    AllowedFunctionNames = new List<string>
            {
                "calculate"
            }
                                }
                            }
                        });

                Console.WriteLine(">>> AFTER GEMINI API CALL");

                // =================================================
                // DEBUG GEMINI RESPONSE
                // =================================================

                Console.WriteLine("===== GEMINI RESPONSE =====");
                Console.WriteLine(response);
                Console.WriteLine("==========================");

                // =================================================
                // CHECK FOR FUNCTION CALL
                // =================================================

                var functionCalls = response.FunctionCalls;

                if (functionCalls != null && functionCalls.Count > 0)
                {
                    foreach (var functionCall in functionCalls)
                    {
                        Console.WriteLine(
                            $"Gemini requested tool: {functionCall.Name}");

                        // ---------------------------------------------
                        // Make sure this is our calculator tool
                        // ---------------------------------------------

                        if (functionCall.Name != "calculate")
                        {
                            continue;
                        }

                        // ---------------------------------------------
                        // Read the expression Gemini provided
                        // ---------------------------------------------

                        if (functionCall.Args == null ||
                            !functionCall.Args.TryGetValue(
                                "expression",
                                out var expressionValue))
                        {
                            return "Calculator tool call did not contain an expression.";
                        }

                        string expression =
                            expressionValue?.ToString() ?? "";

                        Console.WriteLine(
                            $"Calculator expression: {expression}");

                        // ---------------------------------------------
                        // Prepare arguments for CalculatorTool
                        // ---------------------------------------------

                        var arguments = new Dictionary<string, object>
                        {
                            ["expression"] = expression
                        };

                        // ---------------------------------------------
                        // EXECUTE OUR REAL C# TOOL
                        // ---------------------------------------------

                        var toolResult =
                            await _calculator.ExecuteAsync(arguments);

                        Console.WriteLine(
                            $"Calculator result: {toolResult.Output}");

                        // ---------------------------------------------
                        // Handle calculator failure
                        // ---------------------------------------------

                        if (!toolResult.Success)
                        {
                            return $"Calculator error: {toolResult.Error}";
                        }

                        // =================================================
                        // IMPORTANT:
                        //
                        // We now have:
                        //
                        // Gemini FunctionCall
                        //          ↓
                        // CalculatorTool
                        //          ↓
                        // Exact result
                        //
                        // But Gemini still doesn't know the result.
                        //
                        // We must send the result BACK to Gemini.
                        // =================================================


                        // ---------------------------------------------
                        // Add Gemini's function-call message to history
                        // ---------------------------------------------
                        //
                        // Gemini needs to see its own previous
                        // function call before receiving the result.
                        //

                        if (response.Parts != null)
                        {
                            contents.Add(new Content
                            {
                                Role = "model",
                                Parts = response.Parts
                            });
                        }

                        // ---------------------------------------------
                        // Create FunctionResponse
                        // ---------------------------------------------

                        var functionResponse = new FunctionResponse
                        {
                            Name = functionCall.Name,

                            Id = functionCall.Id,

                            Response = new Dictionary<string, object>
                            {
                                ["output"] = toolResult.Output
                            }
                        };

                        // ---------------------------------------------
                        // Send the tool result as a user/tool response
                        // ---------------------------------------------

                        contents.Add(new Content
                        {
                            Role = "user",

                            Parts = new List<Part>
            {
                new Part
                {
                    FunctionResponse = functionResponse
                }
            }
                        });

                        Console.WriteLine(
                            ">>> SENDING FUNCTION RESPONSE BACK TO GEMINI");

                        // =================================================
                        // SECOND GEMINI REQUEST
                        // =================================================
                        //
                        // This time we DON'T force a function call.
                        //
                        // Gemini already has the calculator result.
                        // Now it should produce the normal BuddyBee answer.
                        //

                        var finalResponse =
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

                                    Tools = new List<Tool>
                                    {
                        new Tool
                        {
                            FunctionDeclarations =
                                new List<FunctionDeclaration>
                                {
                                    calculatorDeclaration
                                }
                        }
                                    },

                                    // IMPORTANT:
                                    //
                                    // The first request forced Gemini to call
                                    // the calculator.
                                    //
                                    // The second request must NOT force it.
                                    //
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

                        Console.WriteLine(
                            "===== FINAL GEMINI RESPONSE =====");

                        Console.WriteLine(finalResponse);

                        Console.WriteLine(
                            "=================================");

                        return finalResponse.Text
                            ?? $"The calculation result is {toolResult.Output}.";
                    }
                }

                return response.Text
                    ?? "Gemini returned no response.";

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