using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class TermPool : TermBase, IMultiTerm
    {
        public override ReadOnlyCollection<Variable> Variables { get; }

        public override ExpressionType Type { get; }
        
        //public ReadOnlyCollection<TermBase> Children { get; internal set; }

        public ReadOnlyCollection<(TermBase, char)> Children { get; internal set; }

        //public ReadOnlyCollection<char> Operators { get; internal set; }


        public TermPool(params (TermBase, char)[] terms)
        {
            Children = new ReadOnlyCollection<(TermBase, char)>(terms);
            
            foreach(var term in terms)
            {
                Type = Type.CombineWith(term.Item1.Type);
            }
            Variables = new ReadOnlyCollection<Variable>(terms
                                                            .SelectMany(term=> term.Item1.Variables)
                                                            .Distinct()
                                                            .ToList()
                                                            );
        }


        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        public override double EvaluateFor(params Variable[] valuePairs)
        {
            double ret_value = Children[0].Item1.EvaluateFor(valuePairs);

            for (int i = 1; i< Children.Count; i++)
            {
                double val = Children[i].Item1.EvaluateFor(valuePairs);
                if (Children[i].Item2 == '*')
                    ret_value *= val;
                else if (val == 0)
                    throw new Exception("Math error");
                else ret_value /= val;
            }
            return ret_value;
        }

        public TermBase Inflate()
        {
            if (Children.Count == 1)
                return Children[0].Item1;
            return this;
        }



        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {

            var NonCnstTerms = Children
                                .Where(child => child.Item1.Type != ExpressionType.Constant)
                                .ToList();

            if (NonCnstTerms.Count == 0)
                return Entity.Zero;

            if (degree > 1)
                return Derivative(degree - 1, RegardsTo)
                      .Derivative(1, RegardsTo);

            if (Children.Count == 1)
                return Children[0].Item1.Derivative(degree, RegardsTo);

            /// so now, children > 1, degree = 1, at least 1 non-constant term
            var ConstantTerm = Children.Except(NonCnstTerms).ToList();

            List<(TermBase, char)> lst = new List<(TermBase, char)>(); 

            if(Children.Count == 2 && NonCnstTerms.Count == 1)
            {
                if(NonCnstTerms.Count == 1)
                    return new TermPool((ConstantTerm[0].Item1, '*'),
                                    (NonCnstTerms[0].Item1.Derivative(degree, RegardsTo), '*'));

            }

            /// for terms like uvw, d/dx(uvw) = uv * d/dx(w) + vw * d/dx(u) + wu * d/dx(v)
            /// so, the final result should be an expression

            List<(TermBase, char)> final_expression = new List<(TermBase, char)>();
            for (int i = 0; i < NonCnstTerms.Count; i++)
            {
                /// first, the term without the one to be dealt with in a group:
                List<(TermBase, char)> tlist = NonCnstTerms.Where(term => term != NonCnstTerms[i]).ToList();

                /// now insert the derivative of the current term in that list
                tlist.Add((NonCnstTerms[i].Item1.Derivative(degree, RegardsTo), NonCnstTerms[i].Item2));

                /// construct a termpool with those terms
                TermPool termPool = new TermPool(tlist.ToArray());

                final_expression.Add((termPool, '+'));
            }

            return new Expression(final_expression.ToArray());
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            string str = string.Empty;
            for (int i = 0; i < Children.Count; i++)
            {
                str += Children[i].ToString();
                if (i < Children.Count - 1)
                    str += Children[i].Item2;
            }
            return str;
        }

        public override bool CanAdd(TermBase other)
        {
            throw new NotImplementedException();
        }


        public static bool operator ==(TermPool a, TermPool b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return a.EvaluateFor(null) == b.EvaluateFor(null);

            /// 4x5y3z = 60xyz
            if (a.Type == ExpressionType.Polynomial && a.Type == b.Type)
            {
                double constantA = 1;
                double constantB = 1;

                for (int i = 0; i < a.Children.Count; i++)
                {
                    if (a.Children[i].Item1.Type == ExpressionType.Constant)
                        constantA *= a.Children[i].Item1.EvaluateFor(null);
                }
                for (int i = 0; i < b.Children.Count; i++)
                {
                    if (b.Children[i].Item1.Type == ExpressionType.Constant)
                        constantB *= b.Children[i].Item1.EvaluateFor(null);
                }
                if (constantA != constantB)
                    return false;
                return !a.Variables.OrderBy(varbl => varbl.Sign).Except(a.Variables.OrderBy(vrbl => vrbl.Sign)).Any();
            }

            return false;
        }

        public static bool operator !=(TermPool a, TermPool b)
        {
            return !(a == b);
        }


        public static TermBase operator +(TermPool a, TermPool b)
        {
            ///4*1 + 1*5 = 9
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return new Entity(a.EvaluateFor(null) + b.EvaluateFor(null));

            if (a.Type == ExpressionType.Polynomial && a.Type == b.Type &&
                !a.Variables.OrderBy(varbl => varbl.Sign).Except(a.Variables.OrderBy(vrbl => vrbl.Sign)).Any())
            {
                double constantA = 1;
                double constantB = 1;

                for (int i = 0; i < a.Children.Count; i++)
                {
                    if (a.Children[i].Item1.Type == ExpressionType.Constant)
                    {
                        if(i > 0 && a.Children[i].Item2 == '/')
                            constantA /= a.Children[i].Item1.EvaluateFor(null);
                        else constantA *= a.Children[i].Item1.EvaluateFor(null);
                    }
                }
                for (int i = 0; i < b.Children.Count; i++)
                {
                    if (b.Children[i].Item1.Type == ExpressionType.Constant)
                    {
                        if (i > 0 && b.Children[i].Item2 == '/')
                            constantB /= b.Children[i].Item1.EvaluateFor(null);
                        else constantB *= b.Children[i].Item1.EvaluateFor(null);
                    }
                }

                var lst = new List<(TermBase, char)>
                {
                    (new Entity(constantA + constantB), '*')
                };
                var mops = new List<char>();
                a.Variables.ToList().ForEach(vrbl =>
                {
                    lst.Add((new Entity(vrbl.Sign), '*'));
                });
                return new TermPool(lst.ToArray());
            }

            return Entity.Zero;
        }

        public static TermBase operator -(TermPool a, TermPool b)
        {
            ///4*1 + 1*5 = 9
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return new Entity(a.EvaluateFor(null) - b.EvaluateFor(null));

            if (a.Type == ExpressionType.Polynomial && a.Type == b.Type &&
                !a.Variables.OrderBy(varbl => varbl.Sign).Except(a.Variables.OrderBy(vrbl => vrbl.Sign)).Any())
            {
                double constantA = 1;
                double constantB = 1;

                for (int i = 0; i < a.Children.Count; i++)
                {
                    if (a.Children[i].Item1.Type == ExpressionType.Constant)
                    {
                        if (i > 0 && a.Children[i].Item2 == '/')
                            constantA /= a.Children[i].Item1.EvaluateFor(null);
                        else constantA *= a.Children[i].Item1.EvaluateFor(null);
                    }
                }
                for (int i = 0; i < b.Children.Count; i++)
                {
                    if (b.Children[i].Item1.Type == ExpressionType.Constant)
                    {
                        if (i > 0 && b.Children[i].Item2 == '/')
                            constantB /= b.Children[i].Item1.EvaluateFor(null);
                        else constantB *= b.Children[i].Item1.EvaluateFor(null);
                    }
                }

                var lst = new List<(TermBase, char)>
                {
                    (new Entity(constantA - constantB), '*')
                };
                var mops = new List<char>();
                a.Variables.ToList().ForEach(vrbl =>
                {
                    lst.Add((new Entity(vrbl.Sign), '*'));
                });
                return new TermPool(lst.ToArray());
            }

            return Entity.Zero;
        }

        public static TermBase operator *(TermPool a, TermPool b)
        {
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return new Entity(a.EvaluateFor(null)* b.EvaluateFor(null));

            double constantAB = 1;
            var lst = new List<(TermBase, char)> { (Entity.One, '*') };

            for (int i = 0; i < a.Children.Count; i++)
            {
                if (a.Children[i].Item1.Type == ExpressionType.Constant)
                {
                    if (i > 0 && a.Children[i].Item2 == '/')
                        constantAB /= a.Children[i].Item1.EvaluateFor(null);
                    else constantAB *= a.Children[i].Item1.EvaluateFor(null);
                }
                else
                {
                    var matched = b.Children.Where(child => child == a.Children[i]);
                    if (matched.Any())
                    {
                        var matched_list = matched.ToList();
                        lst.Add((new ExponentTerm(a.Children[i].Item1, matched_list.Count + 1), '*'));
                        b.Children = new ReadOnlyCollection<(TermBase, char)>( b.Children.Except(matched_list).ToList());
                    }
                }
            }
            for (int i = 0; i < b.Children.Count; i++)
            {
                if (b.Children[i].Item1.Type == ExpressionType.Constant)
                {
                    if (i > 0 && b.Children[i].Item2 == '/')
                        constantAB /= b.Children[i].Item1.EvaluateFor(null);
                    else constantAB *= b.Children[i].Item1.EvaluateFor(null);
                }
            }

            lst[0] = (constantAB, '*');
            a.Variables.ToList().ForEach(vrbl =>
            {
                lst.Add((new Entity(vrbl.Sign), '*'));
            });
            return new TermPool(lst.ToArray());
        }

    }
}
