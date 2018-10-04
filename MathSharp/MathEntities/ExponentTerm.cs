using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class ExponentTerm : ITerm
    {
        public Extent Range { get ;}

        public List<Variable> Variables { get; }

        public ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public ITerm Base { get; }

        public ITerm Degree { get; }

        public string Body { get; }


        public ExponentTerm(ITerm _base, ITerm degree)
        {
            Base = _base;
            Degree = degree;
            Body = _base.Body;
            Range = new Extent(_base.Range.Start, degree.Range.End);
            Type = _base.Type.CombineWith(degree.Type);
            Variables = Base.Variables.Union(Degree.Variables).ToList();
        }



        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public double EvaluateFor(List<Variable> valuePairs)
        {
            return Math.Pow(Base.EvaluateFor(valuePairs), Degree.EvaluateFor(valuePairs));
        }

        public void SetVariableValue(char x, double val)
        {
            Base?.SetVariableValue(x, val);
            Degree?.SetVariableValue(x, val);
        }
    }
}
