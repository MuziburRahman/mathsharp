using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class ExponentTerm : TermBase
    {
        public override ReadOnlyCollection<Variable> Variables { get; }

        public override ExpressionType Type { get; }

        public TermBase Base { get; }

        public TermBase Degree { get; }


        public ExponentTerm(TermBase _base, TermBase degree)
        {
            Base = _base;
            Degree = degree;

            Type = ExpressionType.Exponential;
            Variables = new ReadOnlyCollection<Variable>(Base.Variables.Union(Degree.Variables).ToList());
        }



        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        public override double EvaluateFor(params Variable[] valuePairs)
        {
            return Math.Pow(Base.EvaluateFor(valuePairs), Degree.EvaluateFor(valuePairs));
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            throw new NotImplementedException();
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }

        public override bool CanAdd(TermBase other)
        {
            throw new NotImplementedException();
        }
    }
}
