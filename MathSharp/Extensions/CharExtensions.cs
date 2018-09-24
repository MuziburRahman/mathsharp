using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Extensions
{
    public static class CharExtensions
    {
        public static bool IsBracket(this in char c) =>
            IsStartingBracket(c) || IsEndingBracket(c);
        public static bool IsDigit(this in char c) =>
            char.IsDigit(c);

        public static bool IsStartingBracket(this in char c) =>
            c == '{' ||
            c == '[' ||
            c == '(';
        public static bool IsEndingBracket(this in char c) =>
            c == '}' ||
            c == ']' ||
            c == ')';
        public static bool IsAdditiveOperator(this in char c) =>
            c == '+' ||
            c == '-';
        public static bool IsMutiplicativeOperator(this in char c) =>
            c == '*' ||
            c == '/';
    }
}
