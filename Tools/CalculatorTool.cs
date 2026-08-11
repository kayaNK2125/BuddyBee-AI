namespace BuddyBee.Api.Tools
{
    public class CalculatorTool : ITool
    {
        public string Name => "calculate";

        public string Description =>
            "Performs basic arithmetic: add, subtract, multiply, divide.";

        public Task<ToolResult> ExecuteAsync(
            Dictionary<string, object> arguments)
        {
            // 1. Get the operation
            if (!arguments.TryGetValue("operation", out var operationValue)) // get the operation from the arguments
            {
                return Task.FromResult(new ToolResult
                {
                    Success = false,
                    Error = "Missing operation."
                });
            }

            string operation = operationValue.ToString()!.ToLower(); // convert the operation to lowercase for consistency

            // 2. Get number A
            if (!arguments.TryGetValue("a", out var aValue))
            {
                return Task.FromResult(new ToolResult
                {
                    Success = false,
                    Error = "Missing first number."
                });
            }

            // 3. Get number B
            if (!arguments.TryGetValue("b", out var bValue))
            {
                return Task.FromResult(new ToolResult
                {
                    Success = false,
                    Error = "Missing second number."
                });
            }

            // 4. Convert them to numbers
            if (!double.TryParse(aValue.ToString(), out double a) ||
                !double.TryParse(bValue.ToString(), out double b))
            {
                return Task.FromResult(new ToolResult
                {
                    Success = false,
                    Error = "Invalid numbers."
                });
            }

            double result;

            // 5. Perform the operation
            switch (operation)
            {
                case "add":
                    result = a + b;
                    break;

                case "subtract":
                    result = a - b;
                    break;

                case "multiply":
                    result = a * b;
                    break;

                case "divide":

                    if (b == 0)
                    {
                        return Task.FromResult(new ToolResult
                        {
                            Success = false,
                            Error = "Cannot divide by zero."
                        });
                    }

                    result = a / b;
                    break;

                default:
                    return Task.FromResult(new ToolResult
                    {
                        Success = false,
                        Error = $"Unknown operation: {operation}"
                    });
            }

            // 6. Return successful result
            return Task.FromResult(new ToolResult
            {
                Success = true,
                Output = result.ToString(),
                Error = ""
            });
        }
    }
}