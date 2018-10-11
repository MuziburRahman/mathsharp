using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class ExponentTerm : ITerm
    {
        public ReadOnlyCollection<Variable> Variables { get; }

        public ExpressionType Type { get; }

        public ITerm Base { get; }

        public ITerm Degree { get; }


        public ExponentTerm(ITerm _base, ITerm degree)
        {
            Base = _base;
            Degree = degree;

            Type = _base.Type.CombineWith(degree.Type);
            Variables = new ReadOnlyCollection<Variable>(Base.Variables.Union(Degree.Variables).ToList());
        }



        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public double EvaluateFor(IList<Variable> valuePairs)
        {
            return Math.Pow(Base.EvaluateFor(valuePairs), Degree.EvaluateFor(valuePairs));
        }
    }
}
