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
        public ReadOnlyCollection<Variable> Variables { get; }

        public ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public string Body { get; }



        public Entity(string str)
        {
            int index = 0;
            str.PassWhiteSpace(ref index);

            if (str[index].IsStartingBracket())
                throw new Exception("Awww.. didn't expect a bracket in front of a single entity!");

            Body = str;
            var vlist = new List<Variable>();

            if (str[index].IsDigit() || str[index] == '.')
            {
                for (; index < str.Length && (str[index].IsDigit() || str[index] == '.'); index++) ;
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
        }

        public Entity(string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            if (str[index].IsStartingBracket())
                throw new Exception("Awww.. didn't expect a bracket in front of a single entity!");
            
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
            Body = str.Substring(start, index - start);
        }
        



        public double EvaluateFor(IList<Variable> valuePairs)
        {
            if (Type == ExpressionType.Constant && double.TryParse(Body, out double ret_value))
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

        //public ITerm Derivative(int degree = 1, ITerm )
        //{
        //    if (Type == ExpressionType.Polynomial && )
        //        return new Entity("0");
        //    return new Entity("1");
        //}

        //public ITerm Integral(int degree = 1)
        //{
        //    throw new NotImplementedException();
        //}

        public static ITerm operator * (Entity a, Entity b)
        {
            if (a.Type == ExpressionType.Constant && b.Type == ExpressionType.Constant)
                return new Entity((a.EvaluateFor(null) * b.EvaluateFor(null)).ToString());
            else return null;
        } 
    }
}
