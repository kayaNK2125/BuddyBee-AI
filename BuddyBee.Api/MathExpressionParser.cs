using System.Numerics;

namespace BuddyBee.Api.Services
{
    public class MathExpressionParser
    {
        private string _expression = "";
        private int _position;

        private const int MaxExpressionLength = 10_000;
        private const int MaxExponent = 100_000;

        
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
            if (expression.Length > MaxExpressionLength)
            {
                throw new ArgumentException(
                    "Expression is too long.");
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
            var result = ParseUnary();

            while (true)
            {
                SkipWhitespace();

                if (Match('*'))
                {
                    var right = ParseUnary();
                    result = result.Multiply(right);
                }
                else if (Match('/'))
                {
                    var right = ParseUnary();
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
            if (exponent > MaxExponent || exponent < -MaxExponent)
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
        private BigRational ParsePrimary()
        {
            SkipWhitespace();

            // Parentheses.
            //
            // (10 + 5)
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

            // Otherwise we expect a number.
            return ParseNumber();
        }

        private BigRational ParsePower()
        {
            // Parse the base first.
            //
            // This handles the actual number/parentheses,
            // but NOT the unary +/- sign.
            var baseValue = ParsePrimary();

            SkipWhitespace();

            if (Match('^'))
            {
                // Power is right-associative.
                //
                // 2^3^2
                //
                // becomes:
                //
                // 2^(3^2)
                var exponentValue = ParsePower();

                if (exponentValue.Denominator != 1)
                {
                    throw new FormatException(
                        "Power exponent must be an integer.");
                }

                return Power(
                    baseValue,
                    exponentValue.Numerator);
            }

            return baseValue;
        }



        private BigRational ParseUnary()
        {
            SkipWhitespace();

            // Unary minus.
            //
            // We parse the power FIRST.
            //
            // Therefore:
            //
            // -2^2
            //
            // becomes:
            //
            // -(2^2)
            //
            // = -4
            if (Match('-'))
            {
                var value = ParseUnary();

                return new BigRational(0, 1)
                    .Subtract(value);
            }

            // Unary plus.
            if (Match('+'))
            {
                return ParseUnary();
            }

            return ParsePower();
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

            // =========================================================
            // 1. READ THE MAIN NUMBER
            // =========================================================

            // Read digits before decimal point.
            //
            // Example:
            //
            // 123
            // 123.45
            // 6.022e23
            while (_position < _expression.Length &&
                   char.IsDigit(_expression[_position]))
            {
                _position++;
            }

            // Read decimal part if present.
            if (_position < _expression.Length &&
                _expression[_position] == '.')
            {
                _position++;

                int decimalStart = _position;

                while (_position < _expression.Length &&
                       char.IsDigit(_expression[_position]))
                {
                    _position++;
                }

                // Reject:
                //
                // 12.
                //
                // We require digits after '.'.
                if (decimalStart == _position)
                {
                    throw new FormatException(
                        $"Expected digits after decimal point at position {_position}.");
                }
            }

            // We didn't read any number.
            if (start == _position)
            {
                throw new FormatException(
                    $"Expected a number at position {_position}.");
            }


            // =========================================================
            // 2. CONVERT THE MAIN NUMBER INTO BigRational
            // =========================================================

            string numberText =
                _expression[start.._position];

            BigRational result;


            if (!numberText.Contains('.'))
            {
                // Integer:
                //
                // 123
                //
                // becomes:
                //
                // 123/1

                result = new BigRational(
                    BigInteger.Parse(numberText),
                    1);
            }
            else
            {
                // Decimal:
                //
                // 12.34
                //
                // becomes:
                //
                // 1234/100
                //
                // BigRational reduces it automatically.

                string[] parts =
                    numberText.Split('.');

                string wholePart =
                    parts[0];

                string decimalPart =
                    parts[1];

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
                    (whole * denominator) +
                    decimalDigits;

                result = new BigRational(
                    numerator,
                    denominator);
            }


            // =========================================================
            // 3. SCIENTIFIC NOTATION
            // =========================================================

            // Check for:
            //
            // e
            //
            // or:
            //
            // E
            //
            // Examples:
            //
            // 6.022e23
            // 1.5e-10
            // 9.99E+100

            if (_position < _expression.Length &&
                (_expression[_position] == 'e' ||
                 _expression[_position] == 'E'))
            {
                _position++;

                // Scientific notation must have an exponent.
                if (_position >= _expression.Length)
                {
                    throw new FormatException(
                        "Expected exponent after 'e'.");
                }

                // Optional exponent sign.
                bool negativeExponent = false;

                if (Match('-'))
                {
                    negativeExponent = true;
                }
                else
                {
                    Match('+');
                }

                // Remember where exponent digits begin.
                int exponentStart = _position;

                // Read exponent digits.
                while (_position < _expression.Length &&
                       char.IsDigit(_expression[_position]))
                {
                    _position++;
                }

                // Reject:
                //
                // 1.5e
                // 1.5e-
                // 1.5e+
                if (exponentStart == _position)
                {
                    throw new FormatException(
                        "Expected digits after scientific notation exponent.");
                }

                string exponentText =
                    _expression[
                        exponentStart.._position];

                BigInteger exponent =
                    BigInteger.Parse(exponentText);

                if (negativeExponent)
                {
                    exponent = -exponent;
                }

                // Protect against absurd calculations.
                //
                // 1e1000000 would create enormous numbers.
                if (exponent > 100000 ||
                    exponent < -100000)
                {
                    throw new ArgumentException(
                        "Scientific notation exponent is too large.");
                }

                int exponentValue =
                    (int)exponent;

                if (exponentValue > 0)
                {
                    // Example:
                    //
                    // 1.5e3
                    //
                    // 1.5 × 1000
                    //
                    // = 1500

                    result = result.Multiply(
                        new BigRational(
                            BigInteger.Pow(
                                10,
                                exponentValue),
                            1));
                }
                else if (exponentValue < 0)
                {
                    // Example:
                    //
                    // 1.5e-3
                    //
                    // 1.5 / 1000
                    //
                    // = 0.0015

                    result = result.Divide(
                        new BigRational(
                            BigInteger.Pow(
                                10,
                                -exponentValue),
                            1));
                }
            }


            // =========================================================
            // 4. PERCENTAGE
            // =========================================================

            // 20%
            //
            // becomes:
            //
            // 20 / 100
            //
            // This also works with scientific notation:
            //
            // 2e2%
            //
            // = 200%
            //
            // = 2

            SkipWhitespace();

            if (Match('%'))
            {
                result = result.Divide(
                    new BigRational(100, 1));
            }


            return result;
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