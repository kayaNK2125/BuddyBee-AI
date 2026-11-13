using System.Numerics;
using BuddyBee.Api.Services;
using Xunit;

namespace BuddyBee.Tests;

public class BigRationalTests
{
    [Fact]
    public void Constructor_ReducesFractionsViaGcd()
    {
        var rational = new BigRational(4, 8);
        Assert.Equal(new BigInteger(1), rational.Numerator);
        Assert.Equal(new BigInteger(2), rational.Denominator);
        Assert.Equal("1/2", rational.ToString());
    }

    [Fact]
    public void Constructor_NormalizesNegativeDenominator()
    {
        var r1 = new BigRational(1, -2);
        Assert.Equal(new BigInteger(-1), r1.Numerator);
        Assert.Equal(new BigInteger(2), r1.Denominator);
        Assert.Equal("-1/2", r1.ToString());

        var r2 = new BigRational(-4, -8);
        Assert.Equal(new BigInteger(1), r2.Numerator);
        Assert.Equal(new BigInteger(2), r2.Denominator);
        Assert.Equal("1/2", r2.ToString());
    }

    [Fact]
    public void Constructor_ZeroDenominator_ThrowsDivideByZeroException()
    {
        var ex = Assert.Throws<DivideByZeroException>(() => new BigRational(5, 0));
        Assert.Equal("The denominator cannot be zero.", ex.Message);
    }

    [Fact]
    public void Constructor_ZeroNumerator_NormalizesToOneDenominator()
    {
        var rational = new BigRational(0, 5);
        Assert.Equal(BigInteger.Zero, rational.Numerator);
        Assert.Equal(BigInteger.One, rational.Denominator);
        Assert.Equal("0", rational.ToString());
    }

    [Fact]
    public void ToString_IntegerDenominator_ReturnsNumeratorOnly()
    {
        var rational = new BigRational(10, 1);
        Assert.Equal("10", rational.ToString());
    }

    [Theory]
    [InlineData(1, 3, 1, 7, 10, 21, "10/21")]
    [InlineData(1, 2, 1, 2, 1, 1, "1")]
    [InlineData(-1, 3, 2, 3, 1, 3, "1/3")]
    [InlineData(0, 1, 5, 2, 5, 2, "5/2")]
    public void Add_PerformsCorrectAddition(
        long n1, long d1, long n2, long d2, long expectedNum, long expectedDen, string expectedString)
    {
        var r1 = new BigRational(n1, d1);
        var r2 = new BigRational(n2, d2);
        var result = r1.Add(r2);

        Assert.Equal(new BigInteger(expectedNum), result.Numerator);
        Assert.Equal(new BigInteger(expectedDen), result.Denominator);
        Assert.Equal(expectedString, result.ToString());
    }

    [Theory]
    [InlineData(5, 6, 1, 3, 1, 2, "1/2")]
    [InlineData(1, 2, 1, 2, 0, 1, "0")]
    [InlineData(1, 3, 1, 2, -1, 6, "-1/6")]
    public void Subtract_PerformsCorrectSubtraction(
        long n1, long d1, long n2, long d2, long expectedNum, long expectedDen, string expectedString)
    {
        var r1 = new BigRational(n1, d1);
        var r2 = new BigRational(n2, d2);
        var result = r1.Subtract(r2);

        Assert.Equal(new BigInteger(expectedNum), result.Numerator);
        Assert.Equal(new BigInteger(expectedDen), result.Denominator);
        Assert.Equal(expectedString, result.ToString());
    }

    [Theory]
    [InlineData(2, 3, 5, 7, 10, 21, "10/21")]
    [InlineData(3, 4, 4, 3, 1, 1, "1")]
    [InlineData(-2, 3, 3, 4, -1, 2, "-1/2")]
    [InlineData(0, 1, 5, 7, 0, 1, "0")]
    public void Multiply_PerformsCorrectMultiplication(
        long n1, long d1, long n2, long d2, long expectedNum, long expectedDen, string expectedString)
    {
        var r1 = new BigRational(n1, d1);
        var r2 = new BigRational(n2, d2);
        var result = r1.Multiply(r2);

        Assert.Equal(new BigInteger(expectedNum), result.Numerator);
        Assert.Equal(new BigInteger(expectedDen), result.Denominator);
        Assert.Equal(expectedString, result.ToString());
    }

    [Theory]
    [InlineData(2, 3, 5, 7, 14, 15, "14/15")]
    [InlineData(1, 2, 1, 2, 1, 1, "1")]
    [InlineData(4, 1, 2, 1, 2, 1, "2")]
    [InlineData(1, 3, -1, 2, -2, 3, "-2/3")]
    public void Divide_PerformsCorrectDivision(
        long n1, long d1, long n2, long d2, long expectedNum, long expectedDen, string expectedString)
    {
        var r1 = new BigRational(n1, d1);
        var r2 = new BigRational(n2, d2);
        var result = r1.Divide(r2);

        Assert.Equal(new BigInteger(expectedNum), result.Numerator);
        Assert.Equal(new BigInteger(expectedDen), result.Denominator);
        Assert.Equal(expectedString, result.ToString());
    }

    [Fact]
    public void Divide_ByZeroFraction_ThrowsDivideByZeroException()
    {
        var r1 = new BigRational(5, 2);
        var zero = new BigRational(0, 1);

        var ex = Assert.Throws<DivideByZeroException>(() => r1.Divide(zero));
        Assert.Equal("Cannot divide by zero.", ex.Message);
    }

    [Fact]
    public void HandlesLargeNumbersAccurately()
    {
        var largeNum1 = BigInteger.Parse("1000000000000000000000000000000000000");
        var largeNum2 = BigInteger.Parse("2000000000000000000000000000000000000");

        var r1 = new BigRational(largeNum1, 1);
        var r2 = new BigRational(largeNum2, 1);

        var sum = r1.Add(r2);
        Assert.Equal(BigInteger.Parse("3000000000000000000000000000000000000"), sum.Numerator);
        Assert.Equal(BigInteger.One, sum.Denominator);

        var quotient = r1.Divide(r2);
        Assert.Equal(new BigInteger(1), quotient.Numerator);
        Assert.Equal(new BigInteger(2), quotient.Denominator);
    }
}

