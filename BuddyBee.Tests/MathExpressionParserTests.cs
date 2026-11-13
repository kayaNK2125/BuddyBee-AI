using BuddyBee.Api.Services;
using Xunit;

namespace BuddyBee.Tests;

public class MathExpressionParserTests
{
    private readonly MathExpressionParser _parser = new();

    #region Basic Arithmetic

    [Theory]
    [InlineData("1 + 2", "3")]
    [InlineData("10 - 4", "6")]
    [InlineData("6 * 7", "42")]
    [InlineData("8 / 2", "4")]
    [InlineData("1 / 2", "1/2")]
    [InlineData("0 + 5", "5")]
    [InlineData("5 - 5", "0")]
    [InlineData("0 * 100", "0")]
    [InlineData("0 / 5", "0")]
    public void Evaluate_BasicArithmetic_ReturnsExpectedResult(string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("2.5 + 1.5", "4")]
    [InlineData("0.25 * 4", "1")]
    [InlineData("1.2 / 0.4", "3")]
    [InlineData("0.1 + 0.2", "3/10")]
    [InlineData(".5", "1/2")]
    [InlineData(".75 * 2", "3/2")]
    public void Evaluate_Decimals_ReturnsExactFractionOrInteger(string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("-5 + 3", "-2")]
    [InlineData("+5 + 3", "8")]
    [InlineData("-(5 + 3)", "-8")]
    [InlineData("- - 5", "5")]
    public void Evaluate_UnaryOperators_ReturnsExpectedResult(string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("50%", "1/2")]
    [InlineData("100% * 50", "50")]
    [InlineData("20% + 30%", "1/2")]
    public void Evaluate_Percentages_ReturnsExpectedResult(string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("1e3", "1000")]
    [InlineData("1.5e2", "150")]
    [InlineData("2E-2", "1/50")]
    [InlineData("1e+2", "100")]
    public void Evaluate_ScientificNotation_ReturnsExpectedResult(string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    #endregion

    #region Precedence

    [Theory]
    [InlineData("2 + 3 * 4", "14")]
    [InlineData("10 - 6 / 2", "7")]
    [InlineData("10 - 2 * 3 + 4 / 2", "6")]
    [InlineData("2 * 3 + 4 * 5", "26")]
    [InlineData("10 / 2 * 3", "15")]
    public void Evaluate_OperatorPrecedence_MultiplicationAndDivisionBeforeAdditionAndSubtraction(
        string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    [Theory]
    [InlineData("2 * 3 ^ 2", "18")]
    [InlineData("2 + 3 ^ 2", "11")]
    [InlineData("2 ^ 3 * 2", "16")]
    public void Evaluate_PowerPrecedence_HigherThanMultiplicationAndAddition(
        string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    [Fact]
    public void Evaluate_Power_IsRightAssociative()
    {
        // 2 ^ 3 ^ 2 = 2 ^ (3 ^ 2) = 2 ^ 9 = 512, NOT (2^3)^2 = 64
        var result = _parser.Evaluate("2 ^ 3 ^ 2");
        Assert.Equal("512", result.ToString());
    }

    [Fact]
    public void Evaluate_UnaryMinus_HasLowerPrecedenceThanPower()
    {
        // -2^2 = -(2^2) = -4
        var result = _parser.Evaluate("-2 ^ 2");
        Assert.Equal("-4", result.ToString());
    }

    #endregion

    #region Parentheses

    [Theory]
    [InlineData("(2 + 3) * 4", "20")]
    [InlineData("(10 - 6) / 2", "2")]
    [InlineData("2 * (3 + 4) * 5", "70")]
    [InlineData("((1 + 2) * (3 + 4))", "21")]
    [InlineData("(-2) ^ 2", "4")]
    [InlineData("(2 ^ 3) ^ 2", "64")]
    [InlineData("  (  10  +  ( 2 * 3 )  )  ", "16")]
    public void Evaluate_Parentheses_OverridesPrecedence(string expression, string expected)
    {
        var result = _parser.Evaluate(expression);
        Assert.Equal(expected, result.ToString());
    }

    #endregion

    #region Division by Zero

    [Theory]
    [InlineData("10 / 0")]
    [InlineData("0 / 0")]
    [InlineData("5 / (3 - 3)")]
    [InlineData("1 / (2 * 0)")]
    public void Evaluate_DivisionByZero_ThrowsDivideByZeroException(string expression)
    {
        var ex = Assert.Throws<DivideByZeroException>(() => _parser.Evaluate(expression));
        Assert.Equal("Cannot divide by zero.", ex.Message);
    }

    [Fact]
    public void Evaluate_ZeroWithNegativeExponent_ThrowsDivideByZeroException()
    {
        var ex = Assert.Throws<DivideByZeroException>(() => _parser.Evaluate("0 ^ (-2)"));
        Assert.Equal("Zero cannot have a negative exponent.", ex.Message);
    }

    #endregion

    #region Invalid Input

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Evaluate_EmptyOrWhitespace_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10 + x")]
    [InlineData("@!")]
    public void Evaluate_InvalidCharacters_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
    }

    [Theory]
    [InlineData("(1 + 2")]
    [InlineData("((5 * 2) + 1")]
    public void Evaluate_UnclosedParentheses_ThrowsFormatException(string expression)
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
        Assert.Equal("Missing closing parenthesis.", ex.Message);
    }

    [Theory]
    [InlineData("1 + 2)")]
    [InlineData("()")]
    public void Evaluate_MismatchedOrEmptyParentheses_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
    }

    [Theory]
    [InlineData("10 +")]
    [InlineData("10 *")]
    [InlineData("10 /")]
    [InlineData("2 ^")]
    public void Evaluate_TrailingOperator_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
    }

    [Theory]
    [InlineData("12.")]
    [InlineData(".")]
    public void Evaluate_MalformedDecimal_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
    }

    [Theory]
    [InlineData("1e")]
    [InlineData("1e+")]
    [InlineData("1e-")]
    public void Evaluate_MalformedScientificNotation_ThrowsFormatException(string expression)
    {
        Assert.Throws<FormatException>(() => _parser.Evaluate(expression));
    }

    [Fact]
    public void Evaluate_ScientificNotationExponentTooLarge_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => _parser.Evaluate("1e100001"));
        Assert.Equal("Scientific notation exponent is too large.", ex.Message);
    }

    [Fact]
    public void Evaluate_NonIntegerExponent_ThrowsFormatException()
    {
        var ex = Assert.Throws<FormatException>(() => _parser.Evaluate("2 ^ 1.5"));
        Assert.Equal("Power exponent must be an integer.", ex.Message);
    }

    [Fact]
    public void Evaluate_ExpressionTooLong_ThrowsArgumentException()
    {
        var longExpression = new string('1', 10_001);
        var ex = Assert.Throws<ArgumentException>(() => _parser.Evaluate(longExpression));
        Assert.Equal("Expression is too long.", ex.Message);
    }

    [Fact]
    public void Evaluate_ExponentTooLarge_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => _parser.Evaluate("2 ^ 100001"));
        Assert.Equal("Exponent is too large.", ex.Message);
    }

    #endregion
}
