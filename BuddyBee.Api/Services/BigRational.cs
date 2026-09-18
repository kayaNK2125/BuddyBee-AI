using System.Numerics;

namespace BuddyBee.Api.Services
{
    // Represents an exact fraction:
    //
    //      Numerator
    //      ---------
    //      Denominator
    //
    // Example:
    //
    //      4 / 8
    //
    // will automatically become:
    //
    //      1 / 2
    //
    // We use BigInteger instead of int/long because
    // BuddyBee's calculator is supposed to handle
    // extremely large numbers.
    public class BigRational
    {
        // The top part of the fraction.
        public BigInteger Numerator { get; }

        // The bottom part of the fraction.
        public BigInteger Denominator { get; }


        // Constructor
        //
        // Example:
        //
        // new BigRational(4, 8)
        //
        // Internally this will store:
        //
        // Numerator   = 1
        // Denominator = 2
        public BigRational(
            BigInteger numerator,
            BigInteger denominator)
        {
            // A fraction cannot have zero as its denominator.
            //
            // 5 / 0 is mathematically undefined.
            if (denominator == 0)
            {
                throw new DivideByZeroException(
                    "The denominator cannot be zero.");
            }


            // We always want the denominator to be positive.
            //
            // Instead of:
            //
            //     1 / -2
            //
            // we store:
            //
            //    -1 / 2
            //
            // This gives us one consistent representation.
            if (denominator < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }


            // Find the Greatest Common Divisor (GCD).
            //
            // Example:
            //
            // 4 and 8
            //
            // GCD = 4
            //
            // Therefore:
            //
            // 4 / 8
            // ↓
            // 1 / 2
            BigInteger gcd =
                BigInteger.GreatestCommonDivisor(
                    BigInteger.Abs(numerator),
                    denominator);


            // Divide both sides by the GCD
            // to reduce the fraction.
            Numerator = numerator / gcd;
            Denominator = denominator / gcd;
        }


        // Adds two fractions.
        //
        // Example:
        //
        //     1/3 + 1/7
        //
        // Common denominator:
        //
        //     7 + 3
        //     -------
        //       21
        //
        //     = 10/21
        public BigRational Add(BigRational other)
        {
            BigInteger newNumerator =
                (Numerator * other.Denominator)
                +
                (other.Numerator * Denominator);

            BigInteger newDenominator =
                Denominator * other.Denominator;

            return new BigRational(
                newNumerator,
                newDenominator);
        }


        // Subtracts one fraction from another.
        //
        // Example:
        //
        //     5/6 - 1/3
        //
        // =   5/6 - 2/6
        //
        // =   3/6
        //
        // =   1/2
        public BigRational Subtract(BigRational other)
        {
            BigInteger newNumerator =
                (Numerator * other.Denominator)
                -
                (other.Numerator * Denominator);

            BigInteger newDenominator =
                Denominator * other.Denominator;

            return new BigRational(
                newNumerator,
                newDenominator);
        }


        // Multiplies two fractions.
        //
        // Example:
        //
        //     2/3 × 5/7
        //
        // =   10/21
        public BigRational Multiply(BigRational other)
        {
            BigInteger newNumerator =
                Numerator * other.Numerator;

            BigInteger newDenominator =
                Denominator * other.Denominator;

            return new BigRational(
                newNumerator,
                newDenominator);
        }


        // Divides one fraction by another.
        //
        // Example:
        //
        //     2/3 ÷ 5/7
        //
        // Division by a fraction means
        // multiplying by its reciprocal:
        //
        //     2/3 × 7/5
        //
        // =   14/15
        public BigRational Divide(BigRational other)
        {
            // You cannot divide by zero.
            //
            // A fraction is zero when its numerator is zero.
            if (other.Numerator == 0)
            {
                throw new DivideByZeroException(
                    "Cannot divide by zero.");
            }

            BigInteger newNumerator =
                Numerator * other.Denominator;

            BigInteger newDenominator =
                Denominator * other.Numerator;

            return new BigRational(
                newNumerator,
                newDenominator);
        }


        // Converts the fraction into a readable string.
        //
        // Example:
        //
        //     new BigRational(4, 8)
        //
        // becomes:
        //
        //     "1/2"
        //
        // If the denominator is 1, we just return
        // the integer.
        //
        //     10/1 → "10"
        public override string ToString()
        {
            if (Denominator == 1)
            {
                return Numerator.ToString();
            }

            return $"{Numerator}/{Denominator}";
        }
    }
}