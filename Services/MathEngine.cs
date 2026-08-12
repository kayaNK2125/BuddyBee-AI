using System.Numerics;

namespace BuddyBee.Api.Services
{
    public class MathEngine
    {
        public BigInteger Add(
            BigInteger a,
            BigInteger b)
        {
            return a + b;
        }

        public BigInteger Subtract(
            BigInteger a,
            BigInteger b)
        {
            return a - b;
        }

        public BigInteger Multiply(
            BigInteger a,
            BigInteger b)
        {
            return a * b;
        }

        public BigInteger Divide(
            BigInteger a,
            BigInteger b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException(
                    "Cannot divide by zero.");
            }

            return a / b;
        }

        public BigInteger Power(
            BigInteger a,
            int exponent)
        {
            if (exponent < 0)
            {
                throw new ArgumentException(
                    "Negative exponents are not supported for integer results.");
            }

            return BigInteger.Pow(a, exponent);
        }
    }
}