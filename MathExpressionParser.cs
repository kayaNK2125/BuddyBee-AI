using System.Numerics;

namespace BuddyBee.Api.Services
{
    public class MathExpressionParser
    {
        private string _expression = "";
        private int _position;


        // Public entry point.
        //
        // Example:
        //
        // Evaluate("10 + 5 * 2")
        //
        // returns:
        //
        // 20
        public BigRational Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentException(
                    "Expression cannot be empty.");
            }

            _expression = expression;
            _position = 0;

            var result = ParseExpression();

            SkipWhitespace();

            // If there are still characters left,
            // something wasn't understood by the parser.
            if (_position < _expression.Length)
            {
                throw new FormatException(
                    $"Unexpected character '{_expression[_position]}' " +
                    $"at position {_position}.");
            }

            return result;
        }


        // Handles + and -
        //
        // Example:
        //
        // 10 + 5 - 2
        //
        // First calculate 10 + 5,
        // then subtract 2.
        private BigRational ParseExpression()
        {
            var result = ParseTerm();

            while (true)
            {
                SkipWhitespace();

                if (Match('+'))
                {
                    var right = ParseTerm();
                    result = result.Add(right);
                }
                else if (Match('-'))
                {
                    var right = ParseTerm();
                    result = result.Subtract(right);
                }
                else
                {
                    break;
                }
            }

            return result;
        }


        // Handles * and /
        //
        // This is called from ParseExpression(),
        // which gives multiplication/division
        // higher precedence than + and -.
        private BigRational ParseTerm()
        {
            var result = ParsePower();

            while (true)
            {
                SkipWhitespace();

                if (Match('*'))
                {
                    var right = ParsePower();
                    result = result.Multiply(right);
                }
                else if (Match('/'))
                {
                    var right = ParsePower();
                    result = result.Divide(right);
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private BigRational Power(
    BigRational baseValue,
    BigInteger exponent)
        {
            // Prevent absurdly large calculations from
            // consuming unlimited CPU/memory.
            if (exponent > 100000 || exponent < -100000)
            {
                throw new ArgumentException(
                    "Exponent is too large.");
            }

            // x^0 = 1
            if (exponent == 0)
            {
                return new BigRational(1, 1);
            }

            // Negative exponent:
            //
            // 2^-3 = 1 / 2^3 = 1/8
            if (exponent < 0)
            {
                if (baseValue.Numerator == 0)
                {
                    throw new DivideByZeroException(
                        "Zero cannot have a negative exponent.");
                }

                int positiveExponent = (int)(-exponent);

                return new BigRational(
                    BigInteger.Pow(
                        baseValue.Denominator,
                        positiveExponent),

                    BigInteger.Pow(
                        baseValue.Numerator,
                        positiveExponent));
            }

            int exponentValue = (int)exponent;

            return new BigRational(
                BigInteger.Pow(
                    baseValue.Numerator,
                    exponentValue),

                BigInteger.Pow(
                    baseValue.Denominator,
                    exponentValue));
        }


        // Handles:
        //
        // numbers
        // parentheses
        //
        // Example:
        //
        // (10 + 5)
        //
        // The parser recursively evaluates
        // what's inside the parentheses.
        private BigRational ParseFactor()
        {
            SkipWhitespace();

            // Handle a negative number or negative expression.
            //
            // Example:
            //
            // -5
            //
            // We consume the '-' first,
            // then parse the value after it.
            //
            // Mathematically:
            //
            // -5 = 0 - 5
            if (Match('-'))
            {
                var value = ParseFactor();

                return new BigRational(0, 1)
                    .Subtract(value);
            }

            // Handle a positive sign.
            //
            // Example:
            //
            // +5
            //
            // The '+' doesn't change the value,
            // so we simply consume it and parse the value.
            if (Match('+'))
            {
                return ParseFactor();
            }

            // Handle parentheses.
            //
            // Example:
            //
            // -(10 + 5)
            //
            // After consuming '-', ParseFactor()
            // sees '(' and evaluates the expression inside.
            if (Match('('))
            {
                var result = ParseExpression();

                SkipWhitespace();

                if (!Match(')'))
                {
                    throw new FormatException(
                        "Missing closing parenthesis.");
                }

                return result;
            }

            // Otherwise, it must be a number.
            return ParseNumber();
        }

        private BigRational ParsePower()
        {
            var baseValue = ParseFactor();

            SkipWhitespace();

            if (Match('^'))
            {
                SkipWhitespace();

                // For now, powers must use an integer exponent.
                var exponentValue = ParseFactor();

                if (exponentValue.Denominator != 1)
                {
                    throw new FormatException(
                        "Power exponent must be an integer.");
                }

                BigInteger exponent =
                    exponentValue.Numerator;

                return Power(baseValue, exponent);
            }

            return baseValue;
        }


        // Reads an integer number.
        //
        // Example:
        //
        // "12345"
        //
        // becomes:
        //
        // new BigRational(12345, 1)
        private BigRational ParseNumber()
        {
            SkipWhitespace();

            int start = _position;

            // Read the whole-number part.
            while (_position < _expression.Length &&
                   char.IsDigit(_expression[_position]))
            {
                _position++;
            }

            // Check whether the number has a decimal point.
            if (_position < _expression.Length &&
                _expression[_position] == '.')
            {
                _position++;

                // There must be at least one digit after the decimal.
                int decimalStart = _position;

                while (_position < _expression.Length &&
                       char.IsDigit(_expression[_position]))
                {
                    _position++;
                }

                if (decimalStart == _position)
                {
                    throw new FormatException(
                        $"Expected digits after decimal point at position {_position}.");
                }
            }

            // We didn't read anything.
            if (start == _position)
            {
                throw new FormatException(
                    $"Expected a number at position {_position}.");
            }

            string numberText =
                _expression[start.._position];

            // No decimal point:
            //
            // 123
            //
            // becomes:
            //
            // 123/1
            if (!numberText.Contains('.'))
            {
                return new BigRational(
                    BigInteger.Parse(numberText),
                    1);
            }




            // Decimal:
            //
            // 12.34
            //
            // becomes:
            //
            // 1234/100
            //
            // BigRational will automatically reduce it.
            string[] parts = numberText.Split('.');

            string wholePart = parts[0];
            string decimalPart = parts[1];

            BigInteger whole =
                string.IsNullOrEmpty(wholePart)
                    ? BigInteger.Zero
                    : BigInteger.Parse(wholePart);

            BigInteger decimalDigits =
                BigInteger.Parse(decimalPart);

            BigInteger denominator =
                BigInteger.Pow(
                    10,
                    decimalPart.Length);

            BigInteger numerator =
                (whole * denominator) + decimalDigits;

            return new BigRational(
                numerator,
                denominator);
        }


        // Checks whether the current character
        // matches the character we are looking for.
        private bool Match(char expected)
        {
            if (_position < _expression.Length &&
                _expression[_position] == expected)
            {
                _position++;
                return true;
            }

            return false;
        }


        // Ignore spaces in the expression.
        //
        // This allows:
        //
        // 10+5
        //
        // and:
        //
        // 10 + 5
        //
        // to behave the same way.
        private void SkipWhitespace()
        {
            while (_position < _expression.Length &&
                   char.IsWhiteSpace(_expression[_position]))
            {
                _position++;
            }
        }
    }
}