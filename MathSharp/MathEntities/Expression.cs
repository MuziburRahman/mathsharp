
using System.Linq;
using System.Collections.Generic;
using MathSharp.Enum;
using MathSharp.Interface;
using MathSharp.Extensions;
using System;
using MathSharp.MathEntities;
using System.Collections.ObjectModel;
using System.Text;

namespace MathSharp.Entities
{
    public class ExpressionMember
    {
        public TermBase Term;
        public char Operator = '+';
        //public double CoEfficient = 1;
    }

    public class Expression : TermBase, IMultiTerm, IInflatenable, IExpandable, IDegreeOf
    {
        public override IList<Variable> Variables { get; protected set; }
        public override ExpressionType Type { get; protected set; }
        protected IList<TermBase> NonConstantChildren { get; private set; }
        public double ConstantPart { get; private set; }
        public int Dimension => Variables.Count;

        public Expression(string str)
        {
            NonConstantChildren = new List<TermBase>();

            if (str == string.Empty)
                return;

            int index = 0;
            char oprtr = '\0';

            str.PassWhiteSpace(ref index);
            if (str[index].IsStartingBracket())
            {
                index++;
            }
            while (index < str.Length)
            {
                if (str[index].IsEndingBracket() || str[index] == '=')
                    break;

                else if (str[index].IsAdditiveOperator())
                {
                    oprtr = str[index];
                    index++;
                    continue;
                }

                TermBase ex = str.NextTerm(ref index);
                add_impl(ex, oprtr == '\0' ? '+' : oprtr);
                oprtr = '\0';

            }
            InvalidateTypeAndVariables();
        }

        public Expression(string str, ref int index)
        {
            NonConstantChildren = new List<TermBase>();
            char oprtr = '\0';
            bool starting_bracket_found = false;

            str.PassWhiteSpace(ref index);
            if (str[index].IsStartingBracket())
            {
                starting_bracket_found = true;
                index++;
            }
            while (index < str.Length)
            {
                if (str[index].IsEndingBracket())
                {
                    if (starting_bracket_found)
                        index++;
                    break;
                }
                if (str[index] == '=')
                    break;

                else if (str[index].IsAdditiveOperator())
                {
                    oprtr = str[index];
                    index++;
                    continue;
                }

                TermBase ex = str.NextTerm(ref index);
                add_impl(ex, oprtr == '\0' ? '+' : oprtr);
                oprtr = '\0';
            }
            InvalidateTypeAndVariables();
        }

        public Expression(params (TermBase Term, char Operator)[] terms)
        {
            NonConstantChildren = new List<TermBase>();
            for (int i = 0; i < terms.Length; i++)
            {
                add_impl(terms[i].Term, terms[i].Operator);
            }
            InvalidateTypeAndVariables();
        }

        //public Polynomial TryGetPolynomial()
        //{
        //    return Polynomial.TryParse(this);
        //}

        public double DegreeOf(TermBase term)
        {
            if (Type != ExpressionType.Polynomial)
                throw new InvalidOperationException("request for degree of non-polynomial");

            double degree = 0;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == term)
                    degree = 1;
                //else if (this[i] is IDegreeOf degree_term)
                //{
                //    var tmp_degree = degree_term.DegreeOf(term);
                //    if (tmp_degree > degree)
                //        degree = tmp_degree;
                //}
            }
            return degree;
        }

        //public double CoEfficientOf(TermBase term)
        //{
        //    if (term.Type == ExpressionType.Constant || Type == ExpressionType.Constant)
        //        return 0;

        //    double co_efficient = 0;
        //    for (int i = 0; i < NonConstantChildren.Count; i++)
        //    {
        //        if (NonConstantChildren[i].Term == term)
        //            co_efficient += NonConstantChildren[i].CoEfficient;
        //    }
        //    return co_efficient;
        //}

        public int Degree
        {
            get
            {
                int _degree = 0;
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] is IDegreeOf degreed)
                    {
                        int tmp_degree = degreed.Degree;
                        if (tmp_degree > _degree)
                            _degree = tmp_degree;
                    }
                }

                return _degree;
            }
        }



        public double CoEfficientOf(TermBase term)
        {
            double _cof = 0;
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == term)
                    _cof++;
                else if (this[i] is TermPool tp)
                {
                    if (tp.EqualsToNonConstantPart(term))
                    {
                        _cof += tp.ConstantPart;
                        continue;
                    }

                    var tmp = tp.WithoutConstantPart();
                    if (tmp is Expression)
                        _cof += tp.ConstantPart * (tmp as Expression).CoEfficientOf(term);

                    else if (tmp is ExponentTerm)
                        _cof += tp.ConstantPart * (tmp as ExponentTerm).CoefficientOF(term);
                }

                else if (this[i] is ExponentTerm)
                    _cof += (this[i] as ExponentTerm).CoefficientOF(term);
            }
            return _cof;
        }

        public override double EvaluateFor(params Variable[] valuePairs)
        {
            if (valuePairs is null)
                return ConstantPart;

            double to_ret = 0;
            for (int i = 0; i < Count; i++)
            {
                double val = this[i].EvaluateFor(valuePairs ?? Variables.ToArray());
                to_ret += Operator(i) == '+' ? val : -val;
            }
            return to_ret;
        }

        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            if (degree < 1)
                throw new Exception("Provide a valid degrre for differentiation");
            List<(TermBase, char)> tl = new List<(TermBase, char)>();
            for (int i = 0; i < Count; i++)
                tl.Add(((this[i].Derivative(degree, RegardsTo)), Operator(i)));
            return new Expression(tl.ToArray()).Inflate();
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            if (degree < 1)
                throw new Exception("Provide a valid degrre for integration");

            List<(TermBase, char)> tl = new List<(TermBase, char)>();
            for (int i = 0; i < Count; i++)
                tl.Add(((this[i].Integral(degree, RegardsTo, upperbound, lowerbound)), Operator(i)));

            ///if indefinite
            if (double.IsNaN(upperbound) || double.IsNaN(lowerbound))
            {
                tl.Add((new Entity('c'), '+'));
            }

            return new Expression(tl.ToArray());
        }

        public TermBase Inflate()
        {
            if (Count == 0 && ConstantPart != 0)
                return ConstantPart;
            if (Count == 1 && ConstantPart == 0)
                return this[0];

            return this;
        }


        public TermBase Expand()
        {
            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                if (this[i] is IExpandable)
                {
                    var expanded = (this[i] as IExpandable).Expand();
                    if (expanded is Expression)
                    {
                        char oprtr = Operator(i);
                        RemoveAt(i);
                        i--;
                        add_impl(expanded, oprtr);
                    }
                }
            }
            return this;
        }

        public override TermBase Clone()
        {
            (TermBase, char)[] lst = new (TermBase, char)[Count];
            for (int i = 0; i < Count; i++)
            {
                lst[i] = (this[i].Clone(), Operator(i));
            }
            return new Expression(lst) { ConstantPart = ConstantPart };
        }

        private void InvalidateTypeAndVariables()
        {
            Type = ExpressionType.Constant;
            var vars = Enumerable.Empty<Variable>();

            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                Type = Type.CombineWith(this[i].Type);
                vars = vars.Union(this[i].Variables);
            }

            Variables = vars.ToList();
        }

        public int Count => NonConstantChildren.Count;

        public TermBase this[int i]
        {
            get { return NonConstantChildren[i]; }
            //internal set
            //{
            //    NonConstantChildren[i] = value;
            //    InvalidateTypeAndVariables();
            //}
        }

        public char Operator(int i)
        {
            return '+';
        }

        public void RemoveAt(int i)
        {
            if (i >= Count)
                return;

            //var (Term, Operator) = NonConstantChildren[i];
            //if (Term.Type == ExpressionType.Constant)
            //{
            //    if (Operator == '+')
            //        ConstantPart -= Term.EvaluateFor(null);
            //    else ConstantPart += Term.EvaluateFor(null);
            //}
            NonConstantChildren.RemoveAt(i);
            InvalidateTypeAndVariables();
        }

        public bool Remove(TermBase child)
        {
            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                if (this[i] == child)
                {
                    if (child.Type == ExpressionType.Constant)
                    {
                        if (Operator(i) == '+')
                            ConstantPart -= child.EvaluateFor(null);
                        else
                            ConstantPart += child.EvaluateFor(null);
                    }
                    NonConstantChildren.RemoveAt(i);
                    InvalidateTypeAndVariables();
                    return true;
                }
            }

            return false;
        }


        /// <param name="term"></param>
        /// <param name="oprtr"></param>
        /// <returns> true if <paramref name="term"/> was actually added, false otherwise</returns>
        private bool add_impl(TermBase term, char oprtr = '+')
        {
            if (term == Entity.Zero)
            {
                if (NonConstantChildren.Count == 0)
                    NonConstantChildren.Add(term);
                return false;
            }

            if (term.Type == ExpressionType.Constant)
            {
                if (oprtr == '+')
                    ConstantPart += term.EvaluateFor(null);
                else
                    ConstantPart -= term.EvaluateFor(null);
                return false;
            }

            if (term is Expression ex)
            {
                for (int i = 0; i < ex.Count; i++)
                {
                    Add(ex[i]);
                }
                ConstantPart += ex.ConstantPart;
                return true;
            }

            if (term is TermPool tp)
            {
                //if (tp.Degree != UnknownDegree && tp.Degree > Degree)
                //    Degree = tp.Degree;

                /// 5x + 6x = 11x
                for (int i = 0; i < Count; i++)
                {
                    if (this[i] is TermPool tp2 && tp.CanAddWith(tp2))
                    {
                        var tmp = tp2.ConstantPart;
                        tp2.Remove(tp2.ConstantPart);
                        tp2.Add(tp.ConstantPart + tmp);
                        return false;
                    }
                }

                /// 6x + 9y
                NonConstantChildren.Add(tp.Inflate());

                return true;
            }

            /// x + x = 2x, in case term isn't termpool 
            for (int i = 0; i < Count; i++)
            {
                if (this[i] == term)
                {
                    NonConstantChildren[i] = new TermPool((this[i], '*'), (2, '*'));
                    return false;
                }
            }

            //if (term is ExponentTerm et)
            //{
            //    if (et.Type == ExpressionType.Polynomial)
            //    {
            //        NonConstantChildren.Add(new ExpressionMember
            //        {
            //            Term = et,
            //            CoEfficient = 1,
            //        });
            //        var tmp_degree = (int)et.Degree.EvaluateFor(null);
            //        if (Degree < tmp_degree)
            //            Degree = tmp_degree;
            //    }
            //}

            /// +y
            NonConstantChildren.Add(term);

            return true;

        }

        public void Add(TermBase term, char oprtr = '+')
        {
            if (add_impl(term, oprtr))
                InvalidateTypeAndVariables();
        }

        public TermBase WithoutConstantPart()
        {
            if (ConstantPart == 0)
                return this;

            var tmp = Clone() as Expression;
            tmp.ConstantPart = 0;
            return tmp;
        }


        public static Expression operator +(Expression a, TermBase b)
        {
            var ret = a.Clone() as Expression;
            ret.Add(b);
            return ret;
        }

        public static Expression operator -(Expression a, TermBase b)
        {
            var ret = a.Clone() as Expression;
            ret.Add(b, '-');
            return ret;
        }


        public static bool operator ==(Expression a, Expression b)
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
                return !a.Variables.OrderBy(varbl => varbl.Character).Except(a.Variables.OrderBy(vrbl => vrbl.Character)).Any();
            }

            return false;
        }

        public static bool operator !=(Expression a, Expression b)
        {
            return !(a == b);
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("(");
            if (ConstantPart != 0)
                stringBuilder.Append(ConstantPart).Append(" + ");
            for (int i = 0; i < NonConstantChildren.Count; i++)
            {
                if (i > 0)
                    stringBuilder
                        .Append(' ')
                        .Append(Operator(i))
                        .Append(' ');
                stringBuilder.Append(this[i]);
            }
            return stringBuilder.Append(')').ToString();
        }
    }
}
