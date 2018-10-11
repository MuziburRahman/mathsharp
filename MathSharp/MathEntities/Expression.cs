
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
    public class Expression : IEntity
    {
        public List<ITerm> Members { get; private set; }
        
        public ReadOnlyCollection<Variable> Variables { get; }

        public ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public string Body { get ;  }



        private List<char> AdditiveOperators;



        public Expression(string str) 
        {
            Body = str;
            Members = new List<ITerm>();
            if (AdditiveOperators is null)
                AdditiveOperators = new List<char>();

            int index = 0;
            str.PassWhiteSpace(ref index);

            char StartingBracket = '\0';
            if (str[index].IsStartingBracket())
            {
                StartingBracket = str[index];
                index++;
            }

            IEnumerable<Variable> vars = new List<Variable>();
            Type = ExpressionType.Constant;

            while (index < str.Length)
            {
                ITerm ex = null;

                if (str[index].IsStartingBracket())
                    ex = new Expression(str, ref index);

                else if (str[index].IsEndingBracket())
                {
                    if (StartingBracket != '\0')
                    {
                        if (str[index].TryGetInverse(out char c))
                            if (c == StartingBracket)
                            {
                                index++;
                                break;
                            }
                    }
                    throw new Exception("Bracket mismatch");
                }

                else if (str[index].IsAdditiveOperator())
                {
                    AdditiveOperators.Add(str[index]);
                    index++;
                    continue;
                }
                else ex = str.NextTerm(ref index);

                Members.Add(ex);
                if (Members.Count > AdditiveOperators.Count)
                    AdditiveOperators.Add('+');

                //other stuffs
                vars = vars.Union(ex.Variables);
                Type = Type.CombineWith(ex.Type);
            }

            Variables = new ReadOnlyCollection<Variable>(vars.ToList());
        }

        public Expression(string str, ref int index )
        {
            Members = new List<ITerm>();
            if(AdditiveOperators is null)
                AdditiveOperators = new List<char>();

            int start = index;
            str.PassWhiteSpace(ref index);

            char StartingBracket = '\0';
            if (str[index].IsStartingBracket())
            {
                StartingBracket = str[index];
                index++;
            }

            IEnumerable<Variable> vars = new List<Variable>();
            Type = ExpressionType.Constant;

            while (index < str.Length)
            {
                ITerm ex = null;

                if(str[index].IsStartingBracket())
                    ex = new Expression(str, ref index);

                else if (str[index].IsEndingBracket())
                {
                    if(StartingBracket != '\0')
                    {
                        char c;
                        if(str[index].TryGetInverse(out c))
                            if(c == StartingBracket)
                            {
                                index++;
                                break;
                            }
                    }
                    throw new Exception("Bracket mismatch");
                }

                else if (str[index].IsAdditiveOperator())
                {
                    AdditiveOperators.Add(str[index]);
                    index++;
                    continue;
                }
                else ex = str.NextTerm(ref index);

                Members.Add(ex);
                if (Members.Count > AdditiveOperators.Count)
                    AdditiveOperators.Add('+');

                //other stuffs
                vars = vars.Union(ex.Variables);
                Type = Type.CombineWith(ex.Type);
            }

            Body = str.Substring(start, index - start);
            Variables = new ReadOnlyCollection<Variable>(vars.ToList());
        }
        


        public double EvaluateFor(IList<Variable> valuePairs)
        {
            double to_ret = 0;
            for (int i = 0; i < Members.Count; i++)
            {
                double val = Members[i].EvaluateFor(valuePairs ?? Variables);
                to_ret += AdditiveOperators[i] == '+' ? val : -val;
            }
            return to_ret;
        }

        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Body;
        }
    }
}
