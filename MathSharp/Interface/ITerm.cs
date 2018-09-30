
using MathSharp.Enum;
using MathSharp.Expression;
using System;
using System.Collections.Generic;

namespace MathSharp.Interface
{
    public interface ITerm : IComparable<ITerm>
    {
        List<Variable> Variables { get; }
        double Degree { get; }
        Extent Range { get; }
        ExpressionType Type { get; }
        bool IsExponential { get; }

        double EvaluateFor((char variable, double value)[] valuePairs);
        
    }
}
