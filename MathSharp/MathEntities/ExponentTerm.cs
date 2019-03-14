using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using MathSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class ExponentTerm : TermBase, IExpandable, IInflatenable, IDegreeOf
    {
        public override IList<Variable> Variables { get; protected set; }

        public override ExpressionType Type { get; protected set; }

        TermBase _base = Entity.One;
        public TermBase Base
        {
            get { return _base; }
            internal set
            {
                _base = value;
                if (_base is ExponentTerm et)
                {
                    Degree *= et.Degree;
                    _base = et.Base;
                }
                else
                    InvalidateTypeAndVariables();
            }
        }

        TermBase _degree = Entity.Zero;
        public TermBase Degree
        {
            get { return _degree; }
            internal set
            {
                _degree = value;
                InvalidateTypeAndVariables();
            }
        }

        int IDegreeOf.Degree
        {
            get
            {
                if (Base is Entity)
                    return (int)Degree.EvaluateFor(null);

                if (Base is TermPool tp)
                    return tp.Degree * (int)Degree.EvaluateFor(null);

                //if (Base is Polynomial pl)
                //    return pl.Degree * (int)Degree.EvaluateFor(null);

                if (Base is Expression expr)
                    return expr.Degree * (int)Degree.EvaluateFor(null);
                return 0;
            }
        }

        public ExponentTerm(TermBase @base, TermBase degree)
        {
            if (@base is ExponentTerm et)
            {
                _degree = degree * et.Degree;
                _base = et.Base;
            }
            else
            {
                _base = @base;
                _degree = degree;
            }
            InvalidateTypeAndVariables();
        }

        public double CoefficientOF(TermBase term)
        {
            if (this == term)
                return 1;

            if (term.Type == ExpressionType.Polynomial &&
                Type == ExpressionType.Polynomial &&
                Base is Expression)
            {
                var clone = (Clone() as ExponentTerm).Expand();
                if (clone is Expression)
                    return (clone as Expression).CoEfficientOf(term);
            }

            return 0;
        }


        private void InvalidateTypeAndVariables()
        {
            /// 2^3
            if (Base.Type == ExpressionType.Constant && Degree.Type == ExpressionType.Constant)
                Type = ExpressionType.Constant;
            /// x^2
            else if (Base.Type == ExpressionType.Polynomial &&
                     Degree.Type == ExpressionType.Constant &&
                     Degree.EvaluateFor(null) % 1 == 0/*checks whether integer or not*/)
                Type = ExpressionType.Polynomial;
            ///a^x, x^2.5 etc
            else
                Type = ExpressionType.Exponential;

            Variables = new List<Variable>(Base.Variables.Union(Degree.Variables));
        }

        public override TermBase Clone()
        {
            return new ExponentTerm(Base.Clone(), Degree.Clone());
        }

        public TermBase Expand()
        {
            if (Degree.Type == ExpressionType.Constant && Base is Expression exprBase)
            {
                double degree_value = Degree.EvaluateFor(null);
                if (degree_value % 1 != 0 || degree_value < 2)
                    return this;

                if (exprBase.ConstantPart == 0)
                {
                    if (exprBase.Count == 1)
                        return this;
                    if (exprBase.Count > 2)
                    {
                        var Term = exprBase[exprBase.Count - 1];
                        var clone = exprBase.Clone() as Expression;
                        clone.RemoveAt(exprBase.Count - 1);

                        List<(TermBase, char)> list = new List<(TermBase, char)>();
                        ///(a+b)^k
                        for (int i = 0; i <= degree_value; i++)
                        {
                            /// a^i * b^(k-i) * kCi
                            TermBase atok = new ExponentTerm(clone, i).Inflate();
                            if (atok is IExpandable expandable)
                                atok = expandable.Expand();
                            TermBase btok = new ExponentTerm(Term, degree_value - i).Inflate();

                            var term = new TermPool((atok, '*'),
                                                   (btok, '*'),
                                                   (degree_value.Combination(i), '*')).Inflate();
                            if (term is IExpandable)
                                term = (term as IExpandable).Expand();

                            list.Add((term, '+'));
                        }
                        return new Expression(list.ToArray());
                    }

                    List<(TermBase, char)> list2 = new List<(TermBase, char)>();
                    for (int i = 0; i <= degree_value; i++)
                    {
                        /// a^i * b^(k-i) * kCi
                        TermBase atok = new ExponentTerm(exprBase[0], i).Inflate();
                        TermBase btok = new ExponentTerm(exprBase[1], degree_value - i).Inflate();

                        var term = new TermPool((atok, '*'),
                                               (btok, '*'),
                                               (degree_value.Combination(i), '*')).Inflate();
                        if (term is IExpandable)
                            term = (term as IExpandable).Expand();
                        list2.Add((term, '+'));
                    }
                    return new Expression(list2.ToArray());

                }

                else
                {
                    if (exprBase.Count == 0)
                        return this;
                    if (exprBase.Count > 1)
                    {
                        var clone = exprBase.WithoutConstantPart();

                        List<(TermBase, char)> list = new List<(TermBase, char)>();
                        ///(a+b)^k
                        for (int i = 0; i <= degree_value; i++)
                        {
                            /// a^i * b^(k-i) * kCi
                            TermBase atok = new ExponentTerm(clone, i).Inflate();
                            if (atok is IExpandable expandable)
                                atok = expandable.Expand();
                            TermBase btok = Math.Pow(exprBase.ConstantPart, degree_value - i);

                            var term = new TermPool((atok, '*'),
                                                   (btok, '*'),
                                                   (degree_value.Combination(i), '*')).Inflate();
                            if (term is IExpandable)
                                term = (term as IExpandable).Expand();
                            list.Add((term, '+'));
                        }
                        return new Expression(list.ToArray());
                    }

                    List<(TermBase, char)> list2 = new List<(TermBase, char)>();
                    for (int i = 0; i <= degree_value; i++)
                    {
                        /// a^i * b^(k-i) * kCi
                        TermBase atok = new ExponentTerm(exprBase[0], i).Inflate();
                        TermBase btok = Math.Pow(exprBase.ConstantPart, degree_value - i);

                        var term = new TermPool((atok, '*'),
                                               (btok, '*'),
                                               (degree_value.Combination(i), '*')).Inflate();
                        if (term is IExpandable)
                            term = (term as IExpandable).Expand();
                        list2.Add((term, '+'));
                    }
                    return new Expression(list2.ToArray());
                }
            }

            else if (Base is TermPool tp)
            {
                List<(TermBase, char)> list = new List<(TermBase, char)>();
                for (int i = 0; i < tp.Count; i++)
                {
                    list.Add((new ExponentTerm(tp[i], Degree).Inflate(), tp.Operator(i)));
                }
                if (tp.ConstantPart != 1)
                {
                    if (Degree.Type == ExpressionType.Constant)
                        list.Add((Math.Pow(tp.ConstantPart, Degree.EvaluateFor(null)), '*'));
                    else
                        list.Add((new ExponentTerm(tp.ConstantPart, Degree), '*'));
                }
                return new TermPool(list.ToArray()).Inflate();
            }

            return this;
        }

        public TermBase Inflate()
        {
            if (Base == Entity.One)
                return Entity.One;

            if (Degree == Entity.Zero)
            {
                if (Base == Entity.Zero)
                    return double.NaN;
                return Entity.One;
            }
            if (Base == Entity.Zero)
                return Base;

            if (Type == ExpressionType.Constant)
                return EvaluateFor(null);

            if (Degree == Entity.One)
                return Base;

            return this;
        }

        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        //public TermBase Inflate()
        //{
        //    if(Base is TermPool tp)
        //    {
        //        var lst = new List<(TermBase, char)>(); 
        //        for (int i = 0; i < tp.Children.Count; i++)
        //        {
        //            lst.Add((new ExponentTerm(tp.Children[i].Term, Degree), tp.Children[i].Operator));
        //        }
        //        return new TermPool(lst.ToArray());
        //    }
        //    return this;
        //}

        public static bool operator ==(ExponentTerm a, ExponentTerm b)
        {
            /// (x^2)^3 == x^6 is covered in constructor
            return a.Base == b.Base && a.Degree == b.Degree;
        }

        public static bool operator !=(ExponentTerm a, ExponentTerm b)
        {
            return !(a == b);
        }

        public static TermBase operator *(ExponentTerm a, TermBase b)
        {
            if (a.Base == b)
            {
                var added_degree = a.Degree + 1;
                if (added_degree == Entity.Zero)
                    return Entity.One;
                return new ExponentTerm(a.Base, added_degree);
            }
            if (b is ExponentTerm eb && a.Base == eb.Base)
            {
                var added_degree = a.Degree + eb.Degree;
                if (added_degree == Entity.Zero)
                    return Entity.One;
                return new ExponentTerm(a.Base, added_degree);
            }
            return new TermPool((a, '*'), (b, '*'));
        }

        public static TermBase operator /(ExponentTerm a, TermBase b)
        {
            if (a.Base == b)
            {
                var subtracted_degree = a.Degree - 1;
                if (subtracted_degree == Entity.Zero)
                    return Entity.One;
                return new ExponentTerm(a.Base, subtracted_degree);
            }
            if (b is ExponentTerm eb && a.Base == eb.Base)
            {
                var subtracted_degree = a.Degree - eb.Degree;
                if (subtracted_degree == Entity.Zero)
                    return Entity.One;
                return new ExponentTerm(a.Base, subtracted_degree);
            }
            return new TermPool((a, '*'), (b, '/'));
        }


        public override double EvaluateFor(params Variable[] valuePairs)
        {
            return Math.Pow(Base.EvaluateFor(valuePairs), Degree.EvaluateFor(valuePairs));
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            if (Degree.Type == ExpressionType.Constant)
            {
                double degree_value = Degree.EvaluateFor(null);

                if (degree_value == 2)
                    return new TermPool((degree_value, '*'), (Base, '*'));
                return new TermPool((degree_value, '*'), (new ExponentTerm(Base, degree_value - 1), '*'));
            }

            throw new NotImplementedException();
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (Base is Expression || Base is Entity)
                return Base.ToString() + '^' + Degree.ToString();
            return '(' + Base.ToString() + ")^" + Degree.ToString();
        }
    }
}
