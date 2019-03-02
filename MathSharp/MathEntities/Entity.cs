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
    public class Entity : TermBase, IEntity
    {
        public Variable Variable { get; }

        public override ExpressionType Type { get; }

        public string Body { get; }

        public override ReadOnlyCollection<Variable> Variables { get; }

        public Entity(char c)
        {
            if (!char.IsDigit(c))
            {
                Variables = new ReadOnlyCollection<Variable>(new[] { Variable = (c, double.NaN) });
            }
            Body = c.ToString();
        }

        public Entity(double value)
        {
            Body = value.ToString();
            Variables = new ReadOnlyCollection<Variable>(new List<Variable>());
        }

        public Entity(string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            if (str[index].IsStartingBracket())
                throw new Exception("Awww.. didn't expect a bracket in front of a single entity!");
            
            int start = index;

            if (str[index].IsDigit() || str[index] == '.')
            {
                for (; index < str.Length && (str[index].IsDigit() || str[index] == '.') ; index++) ;
                Type = ExpressionType.Constant;
                Variables = new ReadOnlyCollection<Variable>(new List<Variable>());
            }
            else if (str[index].IsAlphabet())
            {
                /// Note: Entity contructor won't check for operator

                Variables = new ReadOnlyCollection<Variable>(new[] { Variable = (str[index], double.NaN) });
                index++;
                Type = ExpressionType.Polynomial;
            }

            Body = str.Substring(start, index - start);
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
            if (Type == ExpressionType.Constant && double.TryParse(Body, out double ret_value))
                return ret_value;
            
            if(Type == ExpressionType.Polynomial)
            {
                if (valuePairs is null)
                {
                    if (Variable == default || double.IsNaN(Variable.Value))
                        throw new Exception("Variable(s) found null");

                    return Variable.Value;
                }

                   
                var selected_pair = valuePairs.FirstOrDefault(pair => pair.Sign == Variable.Sign);

                if (selected_pair == default)
                    throw new Exception("Couldn't evaluate entity");
                return selected_pair.Value;
            }

            throw new Exception("Couldn't evaluate entity");
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            if (Variable == default)
                return Zero;

            if (RegardsTo == '\0' || RegardsTo == Variable.Sign)
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
                return ent.EvaluateFor(new Variable(RegardsTo, upperbound) ) - ent.EvaluateFor(new Variable(RegardsTo, lowerbound));
            }

            else if (Type == ExpressionType.Polynomial)
            {
                if (RegardsTo == Variable.Sign)
                {
                    return new TermPool((new ExponentTerm(new Entity(RegardsTo), new Entity(2)),'*'), 
                                        (new Entity(2), '/'));
                }
                return new TermPool((new Entity(Variable.Sign), '*'), 
                                     (new Entity(RegardsTo), '*'));
            }
            return Zero;
        }

        public override string ToString()
        {
            return Body;
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return a.Body == b.Body;
            if (a.Type == ExpressionType.Polynomial && a.Type == b.Type)
                return a.Variable.Value == b.Variable.Value;

            return false;
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
                return new TermPool(( 2, '*'), (a, '*'));

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
                return new Expression((a, '+'),  (b, '-'));

            return Zero;
        }




        public static implicit operator Entity(double a)
        {
            return new Entity(a);
        }
    }
}
