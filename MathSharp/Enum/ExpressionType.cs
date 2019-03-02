
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
            if ((thisType == ExpressionType.Constant && type == ExpressionType.Polynomial) ||
               (thisType == ExpressionType.Polynomial && type == ExpressionType.Polynomial) ||
               (thisType == ExpressionType.Polynomial && type == ExpressionType.Constant))
                return ExpressionType.Polynomial;

            else if (thisType == ExpressionType.Constant && type == ExpressionType.Constant)
                return ExpressionType.Constant;

            return ExpressionType.Complex;
        }
    }
}
