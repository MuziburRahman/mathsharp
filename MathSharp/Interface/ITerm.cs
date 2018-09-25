
using MathSharp.Enum;
using System.Collections.Generic;

namespace MathSharp.Interface
{
    public interface ITerm
    {
        Dictionary<char, double?> Variables { get; }
        int Degree { get; }
        Extent Range { get; }
        ExpressionType Type { get; }

        double EvaluateFor((char variable, double value)[] valuePairs);
    }
}
