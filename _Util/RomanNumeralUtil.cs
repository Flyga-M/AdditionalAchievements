using System;
using System.Text;

namespace Flyga.AdditionalAchievements
{
    public static class RomanNumeralUtil
    {
        /// <summary>
        /// Returns the Roman Numeral String of a given value. 
        /// https://stackoverflow.com/a/23303475
        /// </summary>
        public static string ToRomanNumeral(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be greater than zero.");
            }

            StringBuilder stringBuilder = new StringBuilder();
            int remainder = value;
            while (remainder > 0)
            {
                if (remainder >= 1000) { stringBuilder.Append("M"); remainder -= 1000; }
                else if (remainder >= 900) { stringBuilder.Append("CM"); remainder -= 900; }
                else if (remainder >= 500) { stringBuilder.Append("D"); remainder -= 500; }
                else if (remainder >= 400) { stringBuilder.Append("CD"); remainder -= 400; }
                else if (remainder >= 100) { stringBuilder.Append("C"); remainder -= 100; }
                else if (remainder >= 90) { stringBuilder.Append("XC"); remainder -= 90; }
                else if (remainder >= 50) { stringBuilder.Append("L"); remainder -= 50; }
                else if (remainder >= 40) { stringBuilder.Append("XL"); remainder -= 40; }
                else if (remainder >= 10) { stringBuilder.Append("X"); remainder -= 10; }
                else if (remainder >= 9) { stringBuilder.Append("IX"); remainder -= 9; }
                else if (remainder >= 5) { stringBuilder.Append("V"); remainder -= 5; }
                else if (remainder >= 4) { stringBuilder.Append("IV"); remainder -= 4; }
                else if (remainder >= 1) { stringBuilder.Append("I"); remainder -= 1; }
            }

            return stringBuilder.ToString();
        }
    }
}
