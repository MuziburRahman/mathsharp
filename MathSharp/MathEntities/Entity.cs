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
    public class Entity : ITerm
    {
        public Extent Range { get ; set ; }

        public ReadOnlyCollection<Variable> Variables { get; }

        public ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public string Body { get; }



        public Entity(string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            if (str[index].IsStartingBracket())
                throw new Exception("Awww.. didn't expect a bracket in front of a single entity!");

            Body = str;
            int start = index;
            var vlist = new List<Variable>();

            if (str[index].IsDigit() || str[index] == '.')
            {
                for (; index < str.Length && (str[index].IsDigit() || str[index] == '.') ; index++) ;
                Type = ExpressionType.Constant;
            }
            else if (str[index].IsAlphabet())
            {
                /// Note: Entity contructor won't check for operator

                vlist.Add(new Variable(str[index], double.NaN));
                index++;
                Type = ExpressionType.Polynomial;
            }

            Variables = new ReadOnlyCollection<Variable>(vlist);
            Range = new Extent(start, index);
        }



        public double EvaluateFor(ReadOnlyCollection<Variable> valuePairs)
        {
            if (Type == ExpressionType.Constant && double.TryParse(Body.Substring(Range.Start, Range.Length), out double ret_value))
                return ret_value;
            
            if(Type == ExpressionType.Polynomial)
            {
                if (valuePairs is null)
                {
                    if (Variables is null || double.IsNaN(Variables[0].Value))
                        throw new Exception("Variable(s) found null");

                    return Variables[0].Value;
                }

                   
                var selected_pair = valuePairs.FirstOrDefault(pair => pair.Sign == Variables[0].Sign);

                if (selected_pair is null)
                    throw new Exception("Couldn't evaluate entity");
                return selected_pair.Value;
            }

            throw new Exception("Couldn't evaluate entity");
        }

        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public void SetVariableValue(char x, double val)
        {
            if (!(Variables is null) && Variables[0].Sign == x)
                Variables[0].Value = val;
        }
    }
}
