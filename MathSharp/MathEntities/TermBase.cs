using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.MathEntities;
using System;
using System.Collections.Generic;

namespace MathSharp.Interface
{
    public abstract class TermBase
    {
        public const int UnknownDegree = int.MaxValue;

        public bool IsNegative { get; set; } = false;
        public abstract IList<Variable> Variables { get; protected set; }

        public abstract ExpressionType Type { get; protected set; }

        public abstract double EvaluateFor(params Variable[] valuePairs);

        public abstract TermBase Derivative(int degree = 1, char RegardsTo = '\0');

        public abstract TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN);

        public abstract TermBase Clone();

        public static TermBase operator + (TermBase a, TermBase b)
        {
            if (a is TermPool ta && b is TermPool tb)
                return ta + tb;
            if (a is Entity ea && b is Entity eb)
                return ea + eb;

            if (a is Expression exa)
                return exa + b;
            if (b is Expression exb)
                return exb + a;

            return Entity.Zero;
        }

        public static TermBase operator -(TermBase a, TermBase b)
        {
            if (a is TermPool ta && b is TermPool tb)
                return ta - tb;
            if (a is Entity ea && b is Entity eb)
                return ea - eb;

            if (a is Expression exa)
                return exa - b;
            if (b is Expression exb)
                return exb - a;

            return Entity.Zero;
        }

        public static TermBase operator * (TermBase a, TermBase b)
        {
            if (a.Type == ExpressionType.Constant && b.Type == ExpressionType.Constant)
                return a.EvaluateFor(null) * b.EvaluateFor(null);

            if(a is TermPool ta)
                return ta * b;
            if (b is TermPool tb)
                return tb * a;

            if (a is ExponentTerm ea)
                return ea * b;
            if (a is ExponentTerm eb)
                return eb * a;

            return new TermPool((a, '*'), (b, '*'));
        }

        public static TermBase operator / (TermBase a, TermBase b)
        {
            if (a.Type == ExpressionType.Constant && b.Type == ExpressionType.Constant)
                return a.EvaluateFor(null) / b.EvaluateFor(null);

            if (a is TermPool ta)
                return ta / b;
            if (b is TermPool tb)
                return tb / a;

            if (a is ExponentTerm ea)
                return ea / b;
            if (a is ExponentTerm eb)
                return eb / a;

            return new TermPool((a, '*'), (b, '/'));
        }

        public static bool operator ==(TermBase a, TermBase b)
        {
            if (a is Entity ea && b is Entity eb)
                return ea == eb;
            if (a is TermPool ta && b is TermPool tb)
                return ta == tb;
            if (a is ExponentTerm eta && b is ExponentTerm etb)
                return eta == etb;
            if (a is OperatoredTerm oa && b is OperatoredTerm ob)
                return oa == ob;
            return false;
        }
        public static bool operator !=(TermBase a, TermBase b)
        {
            return !(a == b);
        }


        public static TermBase operator -(TermBase a)
        {
            a.IsNegative = !a.IsNegative;
            return a;
        }

        public static implicit operator TermBase(double a)
        {
            return new Entity(a);
        }

        public static implicit operator TermBase(char c)
        {
            return new Entity(c);
        }

        public static implicit operator TermBase(string str)
        {
            var children = new List<(TermBase Term, char Operator)>();
            int index = 0;
            char oprtr = '\0';
            bool eqn = false;

            while (index < str.Length)
            {
                if (str[index].IsEndingBracket())
                    break;

                if(str[index] == '=')
                {
                    if (eqn)
                        throw new Exception("Found two '=' sign");
                    index++;
                    return new Equation(new Expression(children.ToArray()).Inflate(), new Expression(str, ref index).Inflate());
                }

                else if (str[index].IsAdditiveOperator())
                {
                    oprtr = str[index];
                    index++;
                    continue;
                }

                children.Add((str.NextTerm(ref index), oprtr == '\0' ? '+' : oprtr));
                oprtr = '\0';
            }
            return new Expression(children.ToArray()).Inflate();
        }
    }
}
