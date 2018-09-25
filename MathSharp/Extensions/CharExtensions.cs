
namespace MathSharp.Extensions
{
    public static class CharExtensions
    {
        public static bool IsBracket(this char c) =>
            IsStartingBracket(c) || IsEndingBracket(c);
        public static bool IsDigit(this char c) =>
            char.IsDigit(c);

        public static bool IsStartingBracket(this char c) =>
            c == '{' ||
            c == '[' ||
            c == '(';
        public static bool IsEndingBracket(this char c) =>
            c == '}' ||
            c == ']' ||
            c == ')';
        public static bool IsAdditiveOperator(this char c) =>
            c == '+' ||
            c == '-';
        public static bool IsMutiplicativeOperator(this char c) =>
            c == '*' ||
            c == '/';
        public static bool TryGetInverse(this char c, out char x)
        {
            switch (c)
            {
                case '[': x = ']'; return true;
                case ']': x = '['; return true;
                case '(': x = ')'; return true;
                case ')': x = '('; return true;
                case '{': x = '}'; return true;
                case '}': x = '{'; return true;
                default: x = '?';  return false;
            }
        }
        public static bool IsAlphabet(this char c) =>
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}
