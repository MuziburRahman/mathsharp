
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
    public class Expression : ITerm
    {
        public List<ITerm> Members { get; private set; }

        //public Dictionary<char, double?> Variables
        //{
        //    get
        //    {
        //        if (Members.Count == 1)
        //            return Members[0].Variables;
        //        else
        //        {
        //            var list = Members[0].Variables.Union(Members[1].Variables);
        //            for (int i = 2; i < Members.Count; i++)
        //                list = list.Union(Members[i].Variables);
        //            return list.ToDictionary(k => k.Key, k=> k.Value);
        //        }

        //    }
        //}

        public Extent Range { get ; }

        public ReadOnlyCollection<Variable> Variables;

        public ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public string Body { get ;  }



        private List<char> AdditiveOperators;



        public Expression(string str) 
        {
            int index = 0;
            Members = new List<ITerm>();
            AdditiveOperators = new List<char>();

            while (index < str.Length)
            {
                str.PassWhiteSpace(ref index);

                if (str[index].IsAdditiveOperator())
                {
                    AdditiveOperators.Add(str[index]);
                    index++;
                }

                Members.Add(new Expression(str, ref index));
                if (Members.Count > AdditiveOperators.Count)
                    AdditiveOperators.Add('+');
            }

            Variables = Variable.CombineVariables(Members);
            //Variables = Variables.Distinct((a)=> a.Sign == b.Sign)
        }
        public Expression(string str, ref int index )
        {
            Body = str;
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

            while (index < Body.Length)
            {
                ITerm ex;

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
                }

                ex = str.NextTerm(ref index);
                Members.Add(ex);
                if (Members.Count > AdditiveOperators.Count)
                    AdditiveOperators.Add('+');

                //other stuffs
                vars = vars.Union(ex.Variables);
                Type = Type.CombineWith(ex.Type);
            }

            Variables = vars.ToList();
            Range = new Extent(start, Body.Length);
        }
        


        public double EvaluateFor(List<Variable> valuePairs)
        {
            double to_ret = 0;
            for (int i = 0; i < Members.Count; i++)
            {
                double val = Members[i].EvaluateFor(valuePairs);
                to_ret += AdditiveOperators[i] == '+' ? val : -val;
            }
            return to_ret;
        }

        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public void SetVariableValue(char x, double val)
        {
            Members.ForEach(member => member.SetVariableValue(x, val));
        }
    }
}
