using BuddyBee.Api.Services;

namespace BuddyBee.Api.Tools
{
    public class CalculatorTool : ITool
    {
        private readonly MathExpressionParser _parser;

        public CalculatorTool(MathExpressionParser parser)
        {
            _parser = parser;
        }

        public string Name => "calculate";

        public string Description =>
            "Evaluates mathematical expressions exactly, including "
            + "large numbers, fractions, decimals, percentages, "
            + "powers, and scientific notation.";

        public Task<ToolResult> ExecuteAsync(
            Dictionary<string, object> arguments)
        {
            // =====================================================
            // 1. GET THE EXPRESSION
            // =====================================================

            // The AI should now send:
            //
            // {
            //     "expression": "938472 * 827"
            // }
            //
            // instead of:
            //
            // operation = multiply
            // a = 938472
            // b = 827

            if (!arguments.TryGetValue(
                    "expression",
                    out var expressionValue))
            {
                return Task.FromResult(
                    new ToolResult
                    {
                        Success = false,
                        Output = "",
                        Error = "Missing expression."
                    });
            }

            string expression =
                expressionValue?.ToString() ?? "";


            // =====================================================
            // 2. VALIDATE THE EXPRESSION
            // =====================================================

            if (string.IsNullOrWhiteSpace(expression))
            {
                return Task.FromResult(
                    new ToolResult
                    {
                        Success = false,
                        Output = "",
                        Error = "Expression cannot be empty."
                    });
            }


            // =====================================================
            // 3. SEND EXPRESSION TO OUR MATH ENGINE
            // =====================================================

            try
            {
                // IMPORTANT:
                //
                // The LLM does NOT calculate the answer here.
                //
                // Our MathExpressionParser does it.
                //
                // Example:
                //
                // "2^100"
                //
                // goes into:
                //
                // MathExpressionParser
                //
                // which uses BigInteger / BigRational
                // to calculate the exact result.

                var result =
                    _parser.Evaluate(expression);


                // =================================================
                // 4. RETURN EXACT RESULT
                // =================================================

                return Task.FromResult(
                    new ToolResult
                    {
                        Success = true,
                        Output = result.ToString(),
                        Error = ""
                    });
            }
            catch (Exception ex)
            {
                // If the expression is invalid, division by zero
                // occurs, the exponent is too large, etc.,
                // return a controlled tool error instead of
                // crashing BuddyBee.

                return Task.FromResult(
                    new ToolResult
                    {
                        Success = false,
                        Output = "",
                        Error = ex.Message
                    });
            }
        }
    }
}