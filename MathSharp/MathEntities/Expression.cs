
using System.Linq;
using System.Collections.Generic;
using MathSharp.Enum;
using MathSharp.Interface;
using MathSharp.Extensions;
using System;
using MathSharp.MathEntities;
using System.Collections.ObjectModel;

namespace MathSharp.Entities
{
    public class Expression : TermBase, IMultiTerm
    {
        public override ReadOnlyCollection<Variable> Variables { get; }

        public override ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public string Body { get ;  }

        public ReadOnlyCollection<(TermBase Term, char Operator)> Children { get ; internal set; }

        public Expression(string str) 
        {
            Body = str;
            var operators = new List<char>();

            int index = 0;

            IEnumerable<Variable> vars = Enumerable.Empty<Variable>();
            List<(TermBase, char)> membrs = new List<(TermBase, char)>();
            Type = ExpressionType.Constant;

            while (index < str.Length)
            {
                char oprtr = '\0';

                if (str[index].IsEndingBracket() || str[index] == '=')
                    break;

                else if (str[index].IsAdditiveOperator())
                {
                    oprtr = str[index];
                    index++;
                    continue;
                }

                TermBase ex = str.NextTerm(ref index);

                membrs.Add((ex, oprtr == '\0' ? '+' : oprtr));

                vars = vars.Union(ex.Variables);
                Type = Type.CombineWith(ex.Type);
            }

            Children = new ReadOnlyCollection<(TermBase, char)>(membrs.ToArray());
            Variables = new ReadOnlyCollection<Variable>(vars.ToList());
        }

        public Expression(string str, ref int index )
        {
            var operators = new List<char>();

            int start = index;

            IEnumerable<Variable> vars = Enumerable.Empty<Variable>();
            List<(TermBase, char)> membrs = new List<(TermBase, char)>();
            Type = ExpressionType.Constant;

            while (index < str.Length)
            {
                char oprtr = '\0';

                if (str[index].IsEndingBracket() || str[index] == '=')
                    break;

                else if (str[index].IsAdditiveOperator())
                {
                    operators.Add(str[index]);
                    index++;
                    continue;
                }

                TermBase ex = str.NextTerm(ref index);

                membrs.Add((ex, oprtr == '\0' ? '+' : oprtr));

                vars = vars.Union(ex.Variables);
                Type = Type.CombineWith(ex.Type);
            }

            Body = str.Substring(start, index - start);
            Children = new ReadOnlyCollection<(TermBase, char)>(membrs);
            Variables = new ReadOnlyCollection<Variable>(vars.ToList());
        }

        public Expression(params (TermBase, char)[] termlist)
        {
            Children = new ReadOnlyCollection<(TermBase, char)>(termlist);
            Body = Children[0].ToString();

            IEnumerable<Variable> vars = Children[0].Term.Variables;
            for(int i = 1; i< termlist.Length; i++)
            {
                if (i > 0)
                    Body += Children[i].Operator;
                Body += Children[i].ToString();
                vars = Variables.Union(Children[i].Term.Variables);
            }

            Variables = new ReadOnlyCollection<Variable>(vars.ToList());
        }
        

        public bool TryFactorize()
        {
            return false;
        }

        public TermBase InFlate()
        {
            if (Children.Count == 1)
                return Children[0].Term;
            return this;
        }


        public override double EvaluateFor(params Variable[] valuePairs)
        {
            double to_ret = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                double val = Children[i].Term.EvaluateFor(valuePairs ?? Variables.ToArray());
                to_ret += Children[i].Operator == '+' ? val : -val;
            }
            return to_ret;
        }

        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return '(' + Body + ')';
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            if (degree < 1)
                throw new Exception("Provide a valid degrre for differentiation");
            List<(TermBase, char)> tl = new List<(TermBase, char)>();
            foreach (var (Term, Operator) in Children)
                tl.Add(((Term.Derivative(degree, RegardsTo)), Operator));
            return new Expression(tl.ToArray());
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            if (degree < 1)
                throw new Exception("Provide a valid degrre for integration");

            List<(TermBase, char)> tl = new List<(TermBase, char)>();
            foreach (var (Term, Operator) in Children)
                tl.Add(((Term.Integral(degree, RegardsTo, upperbound, lowerbound)), Operator));

            ///if indefinite
            if(double.IsNaN(upperbound) || double.IsNaN(lowerbound))
            {
                tl.Add((new Entity('c'), '+'));
            }

            return new Expression(tl.ToArray());
        }

        public TermBase Inflate()
        {
            if (Children.Count == 1)
                return Children[0].Term;
            return this;
        }
    }
}
