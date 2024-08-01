using System;
using AchievementLib.Pack.V1.Models;

namespace Flyga.AdditionalAchievements.Solve.Handler.V1.Api
{
    /// <summary>
    /// Provides utility functions for <see cref="Comparison"/>s.
    /// </summary>
    public static class ComparisonUtil
    {
        /// <summary>
        /// Compares the <paramref name="value"/> with the <paramref name="expected"/> according to the 
        /// <paramref name="comparison"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <param name="comparison"></param>
        /// <returns>The evaluation according to the <paramref name="comparison"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> or <paramref name="expected"/> is 
        /// <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="expected"/> is empty or whitespace.</exception>
        /// <exception cref="NotImplementedException">If a <paramref name="comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="value"/> is something other than 
        /// <see cref="int"/> or <see cref="float"/> and a <paramref name="comparison"/> other than 
        /// <see cref="Comparison.Equal"/> or <see cref="Comparison.NotEqual"/> is provided. Also, if 
        /// <paramref name="expected"/> can't be parsed to the same <see cref="Type"/> as <paramref name="value"/>.</exception>
        public static bool Compare(object value, string expected, Comparison comparison)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            if (string.IsNullOrWhiteSpace(expected))
            {
                throw new ArgumentException($"{nameof(expected)} can't be empty or whitespace.", nameof(expected));
            }

            if (value is int intValue)
            {
                if (!int.TryParse(expected, out int intExpected))
                {
                    throw new InvalidOperationException($"{nameof(expected)} value can't be parsed to the same type as " +
                        $"{nameof(value)} ({value.GetType()}).");
                }

                return CompareInt(intValue, intExpected, comparison);
            }

            if (value is float floatValue)
            {
                if (!float.TryParse(expected, out float floatExpected))
                {
                    throw new InvalidOperationException($"{nameof(expected)} value can't be parsed to the same type as " +
                        $"{nameof(value)} ({value.GetType()}).");
                }

                return CompareFloat(floatValue, floatExpected, comparison);
            }

            if (value is Guid guidValue)
            {
                if (!Guid.TryParse(expected, out Guid guidExpected))
                {
                    throw new InvalidOperationException($"{nameof(expected)} value can't be parsed to the same type as " +
                        $"{nameof(value)} ({value.GetType()}).");
                }

                return CompareGuid(guidValue, guidExpected, comparison);
            }

            return CompareString(value.ToString(), expected, comparison);
        }

        /// <summary>
        /// Compares the <paramref name="value"/> with the <paramref name="expected"/> according to the 
        /// <paramref name="comparison"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <param name="comparison"></param>
        /// <returns>The evaluation according to the <paramref name="comparison"/>.</returns>
        /// <exception cref="NotImplementedException">If a <paramref name="comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        public static bool CompareInt(int value, int expected, Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    {
                        return value == expected;
                    }
                case Comparison.NotEqual:
                    {
                        return value != expected;
                    }
                case Comparison.LessThan:
                    {
                        return value < expected;
                    }
                case Comparison.GreaterThan:
                    {
                        return value > expected;
                    }
                case Comparison.GreaterThanOrEqual:
                    {
                        return value >= expected;
                    }
                case Comparison.LessThanOrEqual:
                    {
                        return value <= expected;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        /// <summary>
        /// Compares the <paramref name="value"/> with the <paramref name="expected"/> according to the 
        /// <paramref name="comparison"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <param name="comparison"></param>
        /// <returns>The evaluation according to the <paramref name="comparison"/>.</returns>
        /// <exception cref="NotImplementedException">If a <paramref name="comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        public static bool CompareFloat(float value, float expected, Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    {
                        return value == expected;
                    }
                case Comparison.NotEqual:
                    {
                        return value != expected;
                    }
                case Comparison.LessThan:
                    {
                        return value < expected;
                    }
                case Comparison.GreaterThan:
                    {
                        return value > expected;
                    }
                case Comparison.GreaterThanOrEqual:
                    {
                        return value >= expected;
                    }
                case Comparison.LessThanOrEqual:
                    {
                        return value <= expected;
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        /// <summary>
        /// Compares the <paramref name="value"/> with the <paramref name="expected"/> according to the 
        /// <paramref name="comparison"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <param name="comparison"></param>
        /// <returns>The evaluation according to the <paramref name="comparison"/>.</returns>
        /// <exception cref="NotImplementedException">If a <paramref name="comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        /// <exception cref="InvalidOperationException">If a <paramref name="comparison"/> other than 
        /// <see cref="Comparison.Equal"/> or <see cref="Comparison.NotEqual"/> is provided.</exception>
        public static bool CompareString(string value, string expected, Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    {
                        return value == expected;
                    }
                case Comparison.NotEqual:
                    {
                        return value != expected;
                    }
                case Comparison.LessThan:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(string)}.");
                    }
                case Comparison.GreaterThan:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(string)}.");
                    }
                case Comparison.GreaterThanOrEqual:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(string)}.");
                    }
                case Comparison.LessThanOrEqual:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(string)}.");
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        /// <summary>
        /// Compares the <paramref name="value"/> with the <paramref name="expected"/> according to the 
        /// <paramref name="comparison"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <param name="comparison"></param>
        /// <returns>The evaluation according to the <paramref name="comparison"/>.</returns>
        /// <exception cref="NotImplementedException">If a <paramref name="comparison"/> is used, that has not been 
        /// implemented yet.</exception>
        /// <exception cref="InvalidOperationException">If a <paramref name="comparison"/> other than 
        /// <see cref="Comparison.Equal"/> or <see cref="Comparison.NotEqual"/> is provided.</exception>
        public static bool CompareGuid(Guid value, Guid expected, Comparison comparison)
        {
            switch (comparison)
            {
                case Comparison.Equal:
                    {
                        return value == expected;
                    }
                case Comparison.NotEqual:
                    {
                        return value != expected;
                    }
                case Comparison.LessThan:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(Guid)}.");
                    }
                case Comparison.GreaterThan:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(Guid)}.");
                    }
                case Comparison.GreaterThanOrEqual:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(Guid)}.");
                    }
                case Comparison.LessThanOrEqual:
                    {
                        throw new InvalidOperationException($"Can't use comparison {comparison} for {typeof(Guid)}.");
                    }
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
    }
}
