
using System.Linq;
using System.Collections.Generic;
using MathSharp.Enum;
using MathSharp.Interface;
using MathSharp.Extensions;
using System;

namespace MathSharp.Expression
{
    public class Expression : ITerm
    {
        public double Degree { get; } = 0;
        public string Body { get; }
        public List<ExpressionType> Types { get; } 
        public Extent Range { get; }
        public bool IsEquation { get; }

        public Dictionary<char, double?> Variables
        {
            get
            {
                if (Terms.Count == 1)
                    return Terms[0].Variables;
                else
                {
                    var list = Terms[0].Variables.Union(Terms[1].Variables);
                    for (int i = 2; i < Terms.Count; i++)
                        list = list.Union(Terms[i].Variables);
                    return list.ToDictionary(k => k.Key, k=> k.Value);
                }

            }
        }

        /// <summary>
        /// if the iterator is in the power of an expression, it's position is raised,
        /// like if the iterator position is 5 at 2x^(5y+6), then it's psotion is 1
        /// baselin's postion is 0
        /// </summary>
        /// 
        public List<Term> Terms;

        public Expression(string expr)
        {
            int index = 0;
            Body = expr;
            Terms = new List<Term>();
            Range = new Extent(0, Body.Length);

            while(index < Body.Length)
            {
                Terms.Add(new Term(this, ref index));
            }
        }

        //for expression inside an expression , embraced by brackets
        //expected to be started with a character
        public Expression(string mainString, int startingIndex, char startingBracket)
        {
            Body = mainString;
            Terms = new List<Term>();
            int end = startingIndex;
            
            if (mainString[end].IsStartingBracket())
                end++;
            if (!startingBracket.TryGetInverse(out char InvChar))
                throw new Exception($"Found no inverse for {startingBracket}");

            while(end < mainString.Length && mainString[end] != InvChar)
            {
                var term = new Term(this, ref end);
                Terms.Add(term);
            }
            if (mainString[end] == InvChar)
                end++;
            Range = new Extent(startingIndex, end);
        }

        public double EvaluateFor((char variable, double value)[] valuePairs)
        {
            double to_ret = 0;
            for (int i = 0; i < Terms.Count; i++)
                to_ret += Terms[i].EvaluateFor(valuePairs);
            return to_ret;
        }
    }
}
