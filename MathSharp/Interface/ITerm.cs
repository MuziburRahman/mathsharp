using System;
using System.Collections.Generic;

namespace MathSharp.Interface
{
    public interface ITerm : IComparable<ITerm> , IEntity
    {
        void SetVariableValue(char x, double val);
    }
}
