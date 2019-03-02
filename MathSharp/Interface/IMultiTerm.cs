using MathSharp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MathSharp.Interface
{
    internal interface IMultiTerm
    {
        ReadOnlyCollection<(TermBase Term, char Operator)> Children { get;}
        TermBase Inflate();
    }
}
