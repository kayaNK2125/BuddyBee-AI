using BuddyBee.Api.Services;
using BuddyBee.Api.Tools;
using Xunit;

namespace BuddyBee.Tests;

public class CalculatorToolTests
{
    private readonly CalculatorTool _tool;

    public CalculatorToolTests()
    {
        _tool = new CalculatorTool(new MathExpressionParser());
    }

    [Fact]
    public void ToolMetadata_ReturnsExpectedNameAndDescription()
    {
        Assert.Equal("calculate", _tool.Name);
        Assert.False(string.IsNullOrWhiteSpace(_tool.Description));
    }

    #region Basic Arithmetic & Precedence & Parentheses

    [Theory]
    [InlineData("10 + 5", "15")]
    [InlineData("20 - 8", "12")]
    [InlineData("6 * 7", "42")]
    [InlineData("9 / 3", "3")]
    [InlineData("1 / 4", "1/4")]
    [InlineData("2 + 3 * 4", "14")]
    [InlineData("(2 + 3) * 4", "20")]
    [InlineData("2 ^ 3 ^ 2", "512")]
    [InlineData("((10 + 2) / 3) * 2", "8")]
    public async Task ExecuteAsync_ValidExpressions_ReturnsSuccessAndResult(string expression, string expectedOutput)
    {
        var args = new Dictionary<string, object>
        {
            ["expression"] = expression
        };

        var result = await _tool.ExecuteAsync(args);

        Assert.True(result.Success);
        Assert.Equal(expectedOutput, result.Output);
        Assert.Equal(string.Empty, result.Error);
    }

    #endregion

    #region Division by Zero

    [Theory]
    [InlineData("10 / 0")]
    [InlineData("5 / (4 - 4)")]
    public async Task ExecuteAsync_DivisionByZero_ReturnsControlledError(string expression)
    {
        var args = new Dictionary<string, object>
        {
            ["expression"] = expression
        };

        var result = await _tool.ExecuteAsync(args);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal("Cannot divide by zero.", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_ZeroWithNegativeExponent_ReturnsControlledError()
    {
        var args = new Dictionary<string, object>
        {
            ["expression"] = "0 ^ (-2)"
        };

        var result = await _tool.ExecuteAsync(args);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal("Zero cannot have a negative exponent.", result.Error);
    }

    #endregion

    #region Invalid Input

    [Fact]
    public async Task ExecuteAsync_MissingExpressionKey_ReturnsError()
    {
        var args = new Dictionary<string, object>();

        var result = await _tool.ExecuteAsync(args);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal("Missing expression.", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_EmptyOrWhitespaceExpression_ReturnsError(string expression)
    {
        var args = new Dictionary<string, object>
        {
            ["expression"] = expression
        };

        var result = await _tool.ExecuteAsync(args);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal("Expression cannot be empty.", result.Error);
    }

    [Fact]
    public async Task ExecuteAsync_NullExpressionValue_ReturnsError()
    {
        var args = new Dictionary<string, object?>
        {
            ["expression"] = null
        };

        var result = await _tool.ExecuteAsync(args!);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal("Expression cannot be empty.", result.Error);
    }

    [Theory]
    [InlineData("not a valid expression")]
    [InlineData("10 +")]
    [InlineData("(10 + 5")]
    [InlineData("12.")]
    public async Task ExecuteAsync_MalformedExpression_ReturnsFailureWithErrorMessage(string expression)
    {
        var args = new Dictionary<string, object>
        {
            ["expression"] = expression
        };

        var result = await _tool.ExecuteAsync(args);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public async Task ExecuteAsync_ExponentTooLarge_ReturnsFailureWithErrorMessage()
    {
        var args = new Dictionary<string, object>
        {
            ["expression"] = "2 ^ 100001"
        };

        var result = await _tool.ExecuteAsync(args);

        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.Output);
        Assert.Equal("Exponent is too large.", result.Error);
    }

    #endregion
}
