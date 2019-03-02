using System.Linq;
using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.MathEntities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MathSharp.Interface
{
    public abstract class TermBase
    {
        public abstract ReadOnlyCollection<Variable> Variables { get; }

        public abstract ExpressionType Type { get; }

        public abstract double EvaluateFor(params Variable[] valuePairs);

        public abstract TermBase Derivative(int degree = 1, char RegardsTo = '\0');

        public abstract TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN);

        public static TermBase operator + (TermBase a, TermBase b)
        {
            if (a is TermPool ta && b is TermPool tb)
            {
                return ta + tb;
            }
            if (a is Entity && b is Entity)
            {
                return a + b;
            }

            return Entity.Zero;
        }

        public static TermBase operator - (TermBase a, TermBase b)
        {
            if (a is TermPool ta && b is TermPool tb)
            {
                return ta - tb;
            }
            if (a is Entity && b is Entity)
            {
                return  a - b ;
            }

            return Entity.Zero;
        }

        public static TermBase operator * (TermBase a, TermBase b)
        {
            if(a is TermPool ta && b is TermPool tb)
                return ta * tb;

            if(a is Entity ea && b is Entity eb)
            {
                return ea* eb;
            }

            return Entity.Zero;
        }

        public static TermBase operator / (TermBase a, TermBase b)
        {
            //if(a is TermPool ta && b is TermPool tb)
            //{
            //    List<TermBase> tl = new List<TermBase>();
            //    tl.AddRange(ta.Children);
            //    tl.AddRange(tb.Children);
            //    List<char> mops = new List<char>();
            //    mops.AddRange(ta.Operators);
            //    mops.Add('/');
            //    for (int i = 0; i < tb.Operators.Count; i++)
            //    {
            //        mops.Add(tb.Operators[i] == '*' ? '/' : '*');
            //    }
            //    return new TermPool(tl, mops);
            //}
            //if(a is Entity && b is Entity)
            //{
            //    if (a.Type == ExpressionType.Constant && b.Type == ExpressionType.Constant)
            //        return new Entity(a.EvaluateFor(null) / b.EvaluateFor(null));
            //    else
            //        return new TermPool(new List<TermBase> { a, b }, new List<char> { '/' });
            //}

            return Entity.Zero;
        }


        public static implicit operator TermBase(double a)
        {
            return new Entity(a);
        }
    }
}
