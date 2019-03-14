using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathSharp.MathEntities
{
    public class TermPool : TermBase, IMultiTerm, IExpandable, IInflatenable, IDegreeOf
    {
        public override IList<Variable> Variables { get; protected set; }

        public override ExpressionType Type { get; protected set; }

        IList<(TermBase Term, char Operator)> NonConstantChildren { get; set; }

        public double ConstantPart { get; private set; } = 1;
        public int Degree
        {
            get
            {
                int _degree = 0;
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] is ExponentTerm et)
                    {
                        _degree += (int)et.Degree.EvaluateFor(null);
                    }
                    else if (this[i] is Entity && this[i].Type == ExpressionType.Polynomial)
                        _degree++;
                }
                return _degree;
            }
        }


        public TermPool(params (TermBase, char)[] terms)
        {
            NonConstantChildren = new List<(TermBase Term, char Operator)>();
            for (int i = 0; i < terms.Length; i++)
            {
                add_impl(terms[i].Item1, terms[i].Item2);
            }
            InvalidateTypeAndVariables();
        }

        public char Operator(int i)
        {
            return NonConstantChildren[i].Operator;
        }

        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        public override double EvaluateFor(params Variable[] valuePairs)
        {
            if (valuePairs is null)
                return ConstantPart;

            double ret_value = NonConstantChildren[0].Term.EvaluateFor(valuePairs);

            for (int i = 1; i < NonConstantChildren.Count; i++)
            {
                double val = NonConstantChildren[i].Term.EvaluateFor(valuePairs);
                if (NonConstantChildren[i].Operator == '*')
                    ret_value *= val;
                else if (val == 0)
                    throw new Exception("Math error");
                else
                    ret_value /= val;
            }
            return ret_value;
        }

        public TermBase Inflate()
        {
            if (NonConstantChildren.Count == 0)
                return ConstantPart;
            if (NonConstantChildren.Count == 1 && ConstantPart == 1)
                return NonConstantChildren[0].Term;
            return this;
        }


        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {

            var NonCnstTerms = NonConstantChildren
                                .Where(child => child.Term.Type != ExpressionType.Constant)
                                .ToList();

            if (NonCnstTerms.Count == 0)
                return Entity.Zero;

            if (degree > 1)
                return Derivative(degree - 1, RegardsTo)
                      .Derivative(1, RegardsTo);

            if (NonConstantChildren.Count == 1)
                return NonConstantChildren[0].Term.Derivative(degree, RegardsTo);

            /// so now, children > 1, degree = 1, at least 1 non-constant term
            var ConstantTerm = NonConstantChildren.Except(NonCnstTerms).ToList();

            List<(TermBase, char)> lst = new List<(TermBase, char)>();

            if (NonConstantChildren.Count == 2 && NonCnstTerms.Count == 1)
            {
                if (NonCnstTerms[0].Operator == '*')
                {
                    return ConstantTerm[0].Operator == '*' ?
                        ConstantTerm[0].Term * NonCnstTerms[0].Term.Derivative(degree, RegardsTo) :
                        NonCnstTerms[0].Term.Derivative(degree, RegardsTo) / ConstantTerm[0].Term;
                }

                return ConstantTerm[0].Term * -new ExponentTerm(NonCnstTerms[0].Term, 2) * NonCnstTerms[0].Term.Derivative(degree, RegardsTo);
            }

            /// for terms like uvw, d/dx(uvw) = uv * d/dx(w) + vw * d/dx(u) + wu * d/dx(v)
            /// so, the final result should be an expression

            List<(TermBase, char)> final_expression = new List<(TermBase, char)>();
            for (int i = 0; i < NonCnstTerms.Count; i++)
            {
                /// first, the term without the one to be dealt with in a group:
                List<(TermBase, char)> tlist = NonCnstTerms.Where(term => term != NonCnstTerms[i]).ToList();

                /// now insert the derivative of the current term in that list
                tlist.Add((NonCnstTerms[i].Term.Derivative(degree, RegardsTo), NonCnstTerms[i].Operator));

                /// construct a termpool with those terms
                TermPool termPool = new TermPool(tlist.ToArray());

                final_expression.Add((termPool, '+'));
            }

            return new Expression(final_expression.ToArray()).Inflate();
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }


        public double CoEfficientOf(TermBase term)
        {
            if (term.Type == ExpressionType.Constant || Type == ExpressionType.Constant)
                return 0;

            if (this == term)
                return 1;

            if (NonConstantChildren.Count == 2 && ConstantPart > 1 && term == NonConstantChildren[0].Term)
                return ConstantPart;

            TermBase non_cnst_clone = (Clone() as TermPool).WithoutConstantPart();
            if (non_cnst_clone == term)
                return ConstantPart;
            else if (non_cnst_clone is IMultiTerm multiTerm)
            {
                for (int i = 0; i < multiTerm.Count; i++)
                {

                }
            }
            return 0;
        }

        public TermBase Expand() /// (a+x)(b+y) = ab + ay + bx + xy
        {
            /// (8x + 5y) * 5 = 40x + 25y 
            if (ConstantPart != 1)
            {
                bool constant_multiplied = false;
                for (int i = 0; i < NonConstantChildren.Count; i++)
                {
                    if (NonConstantChildren[i].Term is ExponentTerm exp)
                    {
                        var tmp = exp.Expand();
                        if (tmp is TermPool || tmp is Expression)
                        {
                            char oprtr = NonConstantChildren[i].Operator;
                            RemoveAt(i);

                            add_impl(tmp, oprtr);
                            return Expand();
                        }
                    }

                    else if (!constant_multiplied && NonConstantChildren[i].Term is Expression e0)
                    {
                        (TermBase, char)[] list = new (TermBase, char)[e0.Count];

                        for (int j = 0; j < e0.Count; j++)
                        {
                            list[j] = (e0[j] * ConstantPart, e0.Operator(j));
                        }

                        var tmp = new Expression(list);
                        tmp.Add(e0.ConstantPart * ConstantPart);
                        NonConstantChildren[i] = (tmp, NonConstantChildren[i].Operator);
                        ConstantPart = 1;
                        break;
                    }
                }
            }


            for (int i = 0; i < NonConstantChildren.Count - 1; i++)
            {
                if (NonConstantChildren[i].Term is Expression e1)
                {
                    for (int j = i + 1; j < NonConstantChildren.Count; j++)
                    {
                        if (NonConstantChildren[j].Term is Expression e2)
                        {
                            List<(TermBase, char)> list = new List<(TermBase, char)>();
                            for (int k = 0; k < e1.Count; k++)
                            {
                                for (int l = 0; l < e2.Count; l++)
                                {
                                    list.Add((e1[k] * e2[l], e1.Operator(k) == '+' ? e2.Operator(l) :
                                                                       e2.Operator(l) == '+' ? '-' : '+'));
                                }
                            }
                            NonConstantChildren.RemoveAt(j);
                            NonConstantChildren[i] = (new TermPool(list.ToArray()), NonConstantChildren[i].Operator);
                        }
                    }
                }

            }

            return Inflate();
        }

        public override TermBase Clone()
        {
            (TermBase, char)[] lst = new (TermBase, char)[NonConstantChildren.Count];
            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                lst[i] = NonConstantChildren[i];
            }
            return new TermPool(lst) { ConstantPart = ConstantPart };
        }

        public TermBase this[int i]
        {
            get => NonConstantChildren[i].Term;
        }

        private void InvalidateTypeAndVariables()
        {
            Type = ExpressionType.Constant;
            var vars = Enumerable.Empty<Variable>();

            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                Type = Type.CombineWith(NonConstantChildren[i].Term.Type);
                vars = vars.Union(NonConstantChildren[i].Term.Variables);
            }

            Variables = vars.ToList();
        }

        private void InvalidateDegree()
        {

        }

        public void RemoveAt(int i)
        {
            if (i >= Count)
                return;

            if (NonConstantChildren[i].Term.Type == ExpressionType.Constant)
            {
                if (NonConstantChildren[i].Operator == '*')
                    ConstantPart /= NonConstantChildren[i].Term.EvaluateFor(null);
                else
                    ConstantPart *= NonConstantChildren[i].Term.EvaluateFor(null);
            }
            else
            {
                NonConstantChildren.RemoveAt(i);
                InvalidateTypeAndVariables();
            }
        }

        public bool Remove(TermBase child)
        {
            if (child.Type == ExpressionType.Constant && child == ConstantPart)
            {
                ConstantPart = 1;
                return true;
            }

            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                if (NonConstantChildren[i].Term == child)
                {
                    NonConstantChildren.RemoveAt(i);
                    InvalidateTypeAndVariables();
                    return true;
                }
            }

            return false;
        }

        private bool add_impl(TermBase term, char oprtr)
        {
            if (term == Entity.Zero)
            {
                if (oprtr == '*')
                {
                    NonConstantChildren.Clear();
                    NonConstantChildren.Add((Entity.Zero, '*'));
                    ConstantPart = 0;
                }
                else if (oprtr == '/')
                {
                    NonConstantChildren.Clear();
                    NonConstantChildren.Add((double.NaN, '*'));
                    ConstantPart = double.NaN;
                }
                Type = ExpressionType.Constant;
                Variables?.Clear();
            }

            else if (term == Entity.One)
            {

            }

            else if (term.Type == ExpressionType.Constant)
            {
                if (oprtr == '*')
                    ConstantPart *= term.EvaluateFor(null);
                else
                    ConstantPart /= term.EvaluateFor(null);
            }

            else if (term is ExponentTerm et1)
            {
                bool flag = true;
                for (int i = 0; i < NonConstantChildren.Count; i++)
                {
                    if (NonConstantChildren[i].Term is ExponentTerm et2 && et1.Base == et2.Base)
                    {
                        if (oprtr == '*')
                        {
                            et2.Degree += et1.Degree;
                        }
                        else if (oprtr == '/')
                        {
                            et2.Degree -= et1.Degree;
                        }
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    NonConstantChildren.Add((term, oprtr));
                    return true;
                }
            }

            else if (term is TermPool tp)
            {
                for (int i = 0; i < tp.Count; i++)
                {
                    Add(tp[i], oprtr);
                }
                if (tp.ConstantPart != 1)
                    ConstantPart *= tp.ConstantPart;
            }

            else
            {
                for (int i = 0; i < NonConstantChildren.Count; i++)
                {
                    if (NonConstantChildren[i].Term is ExponentTerm et && et.Base == term)
                    {
                        et.Degree += 1;
                        return false;
                    }
                    if (NonConstantChildren[i].Term == term)
                    {
                        NonConstantChildren[i] = (new ExponentTerm(term, 2), NonConstantChildren[i].Operator);
                        return false;
                    }
                }

                NonConstantChildren.Add((term, oprtr));
                return true;
            }
            return false;
        }

        public void Add(TermBase term, char oprtr = '*')
        {
            if (add_impl(term, oprtr))
                InvalidateTypeAndVariables();
        }

        public TermBase WithoutConstantPart()
        {
            return new TermPool(NonConstantChildren.ToArray()).Inflate();
        }

        public int Count => NonConstantChildren.Count;


        public bool EqualsToNonConstantPart(TermBase term)
        {
            if (term is null)
                return false;

            if (NonConstantChildren.Count == 0)
                return false;

            if (NonConstantChildren.Count == 1/* && (term is ExponentTerm || term is Entity)*/)
            {
                return term == NonConstantChildren[0].Term;
            }

            if (term is TermPool tp)
                return CanAddWith(tp);
            return false;
        }

        public bool CanAddWith(TermPool term)
        {
            if (Count != term.Count)
                return false;

            bool all_match = true;
            for (int i = 0; i < Count; i++)
            {
                bool no_match = true;
                for (int j = 0; j < term.Count; j++)
                {
                    if (this[i] == term[i])
                    {
                        no_match = false;
                        break;
                    }
                }
                if (no_match)
                {
                    all_match = false;
                    break;
                }
            }
            return all_match;
        }

        public static bool operator ==(TermPool a, TermPool b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return a.ConstantPart == b.ConstantPart;

            /// 4x5y3z = 60xyz
            if (a.Type == ExpressionType.Polynomial && a.Type == b.Type)
            {
                if (a.ConstantPart != b.ConstantPart)
                    return false;

                if (a.Count != b.Count)
                    return false;

                bool all_match = true;
                for (int i = 0; i < a.Count; i++)
                {
                    bool no_match = true;
                    for (int j = 0; j < b.Count; j++)
                    {
                        if (a[i] == b[i])
                        {
                            no_match = false;
                            break;
                        }
                    }
                    if (no_match)
                    {
                        all_match = false;
                        break;
                    }
                }
                return all_match;
            }

            return false;
        }

        public static bool operator !=(TermPool a, TermPool b)
        {
            return !(a == b);
        }


        //public static TermBase operator +(TermPool a, TermPool b)
        //{
        //    ///4*1 + 1*5 = 9
        //    if (a.Type == ExpressionType.Constant && a.Type == b.Type)
        //        return new Entity(a.ConstantPart + b.ConstantPart);

        //    /// 4xz + 6zx
        //    if (a.Type == ExpressionType.Polynomial && a.Type == b.Type &&
        //        !a.Variables.OrderBy(vrbl => vrbl.Character)
        //                    .Except(a.Variables.OrderBy(vrbl => vrbl.Character))
        //                    .Any())
        //    {
        //        var lst = new List<(TermBase, char)>
        //        {
        //            (new Entity(a.ConstantPart + b.ConstantPart), '*')
        //        };
        //        var mops = new List<char>();
        //        a.Variables.ToList().ForEach(vrbl =>
        //        {
        //            lst.Add((new Entity(vrbl.Character), '*'));
        //        });
        //        return new TermPool(lst.ToArray());
        //    }

        //    return Entity.Zero;
        //}

        //public static TermBase operator -(TermPool a, TermPool b)
        //{
        //    ///4*1 + 1*5 = 9
        //    if (a.Type == ExpressionType.Constant && a.Type == b.Type)
        //        return new Entity(a.EvaluateFor(null) - b.EvaluateFor(null));

        //    if (a.Type == ExpressionType.Polynomial && a.Type == b.Type &&
        //        !a.Variables.OrderBy(varbl => varbl.Character).Except(a.Variables.OrderBy(vrbl => vrbl.Character)).Any())
        //    {
        //        var lst = new List<(TermBase, char)>
        //        {
        //            (new Entity(a.ConstantPart - b.ConstantPart), '*')
        //        };
        //        var mops = new List<char>();
        //        a.Variables.ToList().ForEach(vrbl =>
        //        {
        //            lst.Add((new Entity(vrbl.Character), '*'));
        //        });
        //        return new TermPool(lst.ToArray());
        //    }

        //    return Entity.Zero;
        //}

        public static TermBase operator *(TermPool a, TermBase b)
        {
            var ret = a.Clone() as TermPool;
            ret.Add(b);
            return ret;
        }

        public static TermBase operator /(TermPool a, TermBase b)
        {
            var ret = a.Clone() as TermPool;
            ret.Add(b, '/');
            return ret;
        }



        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (ConstantPart != 1 || NonConstantChildren.Count == 0)
                stringBuilder.Append(ConstantPart);
            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                //if (i > 0)
                //    stringBuilder.Append(" ")
                //                 .Append(NonConstantChildren[i].Operator)
                //                 .Append(" ");
                stringBuilder.Append(NonConstantChildren[i].Term);
            }
            return stringBuilder.ToString();
        }

        //public int DegreeOf(TermBase term)
        //{
        //    if (term.Type == ExpressionType.Constant)
        //        return 0;

        //    for (int i = 0; i < Count; i++)
        //    {
        //        if (this[i] == term)
        //            return 1;
        //        if (this[i] is IDegreeOf degrred_term)
        //        {
        //            var tmp_degree = degrred_term.DegreeOf(term);
        //            if (tmp_degree != 0 || tmp_degree != UnknownDegree)
        //                return tmp_degree;
        //        }
        //    }

        //    return 0;
        //}
    }
}
