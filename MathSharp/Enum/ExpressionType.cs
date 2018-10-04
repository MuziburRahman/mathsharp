
namespace MathSharp.Enum
{
    public enum ExpressionType
    {
        Constant,
        Polynomial,
        Trigonometric,
        Logarithmic,
        Differential,
        Integral,
        Exponential,
        Complex /// contains imaginary number
    }

    public static class ExpressionTypeHelper
    {
        public static ExpressionType CombineWith(this ExpressionType thisType, ExpressionType type)
        {
            return ExpressionType.Complex;
        }
    }
}
