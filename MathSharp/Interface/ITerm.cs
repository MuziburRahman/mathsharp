using MathSharp.Entities;
using MathSharp.Enum;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MathSharp.Interface
{
    public interface ITerm
    {
        ReadOnlyCollection<Variable> Variables { get; }

        ExpressionType Type { get; }

        double EvaluateFor(IList<Variable> valuePairs);

        //ITerm Derivative(int degree = 1, ITerm RegardsTo = null);

        //ITerm Integral(int degree = 1);
    }
}
