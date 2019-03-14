using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class Entity : TermBase, IDegreeOf
    {
        Variable _var;
        public Variable Variable
        {
            get => _var;
            private set
            {
                Variables.Clear();
                Variables.Add(value);

                _var = value;
            }
        }

        public override ExpressionType Type { get; protected set; }

        public override IList<Variable> Variables { get; protected set; }

        public int Degree => double.IsNaN(Variable.Value) ? 1 : 0;

        public Entity(char c)
        {
            Variables = new List<Variable>();
            if (!char.IsDigit(c))
            {
                Variable = new Variable(c, double.NaN);
                Type = ExpressionType.Polynomial;
            }
            else
            {
                Variable = new Variable('\0', c - '0');
                Type = ExpressionType.Polynomial;
            }
        }

        public Entity(double value)
        {
            Variables = new List<Variable>();
            Variable = new Variable('\0', value);
            Type = ExpressionType.Constant;
        }

        public Entity(Variable variable)
        {
            Variables = new List<Variable>();
            Variable = variable;
            Type = variable.Character == '\0' ? ExpressionType.Constant : ExpressionType.Polynomial;
        }

        public Entity(string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            if (str[index].IsStartingBracket())
                throw new Exception("Awww.. didn't expect a bracket in front of a single entity!");

            int start = index;

            Variables = new List<Variable>();

            if (str[index].IsDigit() || str[index] == '.')
            {
                for (; index < str.Length && (str[index].IsDigit() || str[index] == '.'); index++)
                    ;
                Type = ExpressionType.Constant;
                double tmp = double.Parse(str.Substring(start, index - start));
                Variable = new Variable('\0', tmp);
            }
            else if (str[index].IsAlphabet())
            {
                /// Note: Entity contructor won't check for operator

                Variable = new Variable(str[index], double.NaN);
                index++;
                Type = ExpressionType.Polynomial;
            }
        }


        public static Entity Zero;
        public static Entity One;

        static Entity()
        {
            Zero = new Entity(0);
            One = new Entity(1);
        }

        public override double EvaluateFor(params Variable[] valuePairs)
        {
            if (Type == ExpressionType.Constant)
                return IsNegative ? -Variable.Value : Variable.Value;

            if (Type == ExpressionType.Polynomial)
            {
                if (valuePairs is null)
                {
                    return IsNegative ? -Variable.Value : Variable.Value;
                }


                var selected_pair = valuePairs.FirstOrDefault(pair => pair.Character == Variable.Character);

                if (selected_pair == default)
                    throw new Exception("Couldn't evaluate entity");
                return IsNegative ? -selected_pair.Value : selected_pair.Value;
            }

            throw new Exception("Couldn't evaluate entity");
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            if (Variable == default)
                return Zero;

            if (RegardsTo == '\0' || RegardsTo == Variable.Character)
            {
                if (Type == ExpressionType.Constant)
                    return Zero;
                else if (Type == ExpressionType.Polynomial)
                    return One;
            }
            return Zero;
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            if (Type == ExpressionType.Constant)
            {
                if (RegardsTo == '\0')
                    throw new Exception("Specify regards to which variable this expression should be integrated");
                TermBase ent = new Entity(RegardsTo) * this;
                if (double.IsNaN(upperbound) || double.IsNaN(lowerbound))
                    return ent;
                return ent.EvaluateFor(new Variable(RegardsTo, upperbound)) - ent.EvaluateFor(new Variable(RegardsTo, lowerbound));
            }

            else if (Type == ExpressionType.Polynomial)
            {
                if (RegardsTo == Variable.Character)
                {
                    return new TermPool((new ExponentTerm(new Entity(RegardsTo), new Entity(2)), '*'),
                                        (new Entity(2), '/'));
                }
                return new TermPool((new Entity(Variable.Character), '*'),
                                     (new Entity(RegardsTo), '*'));
            }
            return Zero;
        }

        public override string ToString()
        {
            return Variable.Character == '\0' ? Variable.Value.ToString() : Variable.Character.ToString();
        }

        public override TermBase Clone()
        {
            return new Entity(this.Variable);
        }

        public int DegreeOf(TermBase term)
        {
            if (term == this)
                return 1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TermBase tb))
                return false;

            if (obj is Entity b)
            {
                return Variable == b.Variable;
            }

            if (tb is IMultiTerm multi)
            {
                if (multi.Count != 1)
                    return false;
                return Equals(multi[0]);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Variable.GetHashCode();
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b)
        {
            return !(a == b);
        }

        public static TermBase operator +(Entity a, Entity b)
        {
            ///4+5=9
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return a.EvaluateFor(null) + b.EvaluateFor(null);

            /// b+b=2b
            else if (a.Type == ExpressionType.Polynomial && a.Type == b.Type && a.Variable == b.Variable)
                return new TermPool((2, '*'), (a, '*'));

            /// b + 9 = b+9
            else if (a.Type == ExpressionType.Polynomial && a.Type == b.Type && a.Variable == b.Variable)
                return new Expression((a, '+'), (b, '+'));

            return Zero;
        }

        public static TermBase operator -(Entity a, Entity b)
        {
            ///4-5=-1
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return a.EvaluateFor(null) - b.EvaluateFor(null);

            /// b - 9 = b-9
            else if (a.Type == ExpressionType.Polynomial && a.Type == b.Type && a.Variable == b.Variable)
                return new Expression((a, '+'), (b, '-'));

            return Zero;
        }




        public static implicit operator Entity(double a)
        {
            if (a == 0)
                return Zero;
            if (a == 1)
                return One;
            return new Entity(a);
        }
    }
}
