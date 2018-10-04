
using MathSharp.Entities;
using MathSharp.Enum;
using System.Collections.ObjectModel;

namespace MathSharp.Interface
{
    public interface IEntity
    {
        Extent Range { get; }
        string Body { get; }
        ReadOnlyCollection<Variable> Variables { get; }

        ExpressionType Type { get; }
        bool IsExponential { get; }

        double EvaluateFor(ReadOnlyCollection<Variable> valuePairs);
    }
}
