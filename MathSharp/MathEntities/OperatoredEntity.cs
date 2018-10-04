using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class OperatoredEntity : ITerm
    {
        public string Operator { get; }

        public ITerm Degree { get; private set; }

        public ITerm MainBody { get; private set; }

        public Extent Range { get; private set; }

        public List<Variable> Variables { get; }

        public ExpressionType Type { get; }

        public bool IsExponential => throw new NotImplementedException();

        public string Body { get; }


        /// <param name="index">the index from where main body starts, excluding operator</param>
        public OperatoredEntity(string op, string str, ref int index)
        {
            Body = str;
            Operator = op;
            int start = index - op.Length;

            /// degree , if any
            str.PassWhiteSpace(ref index);
            if (str[index] == '^')
            {
                Degree = str.NextSingleTerm(ref index);
            }

            /// body 
            if (str[index].IsStartingBracket())
            {
                MainBody = new Expression(str, ref index);
            }
            else MainBody = new Entity(str, ref index);

            ///other stuffs
            Range = new Extent(start, index);

            if (Degree is null)
                Variables = MainBody.Variables;
            else Variables = MainBody.Variables.Union(Degree.Variables).ToList();

            switch (op)
            {
                case "sin":
                case "Sin":
                case "cos":
                case "Cos":
                case "Tan":
                case "tan":
                case "Cot":
                case "cot":
                case "Sec":
                case "sec": Type = ExpressionType.Trigonometric; break;
                case "Log":
                case "log":
                case "ln":
                case "Ln": Type = ExpressionType.Logarithmic; break;
            }
        }


        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public double EvaluateFor(List<Variable> valuePairs)
        {
            double ret_value = MainBody.EvaluateFor(valuePairs);
            switch (Operator)
            {
                case "sin":
                case "Sin": ret_value = Math.Sin(ret_value); break;
                case "cos":
                case "Cos": ret_value = Math.Cos(ret_value); break;
                case "Tan":
                case "tan":
                    if (Math.Cos(ret_value) == 0)
                        throw new Exception("Math error");
                    ret_value = Math.Tan(ret_value); break;
                case "Cot":
                case "cot":
                    if (Math.Sin(ret_value) == 0)
                        throw new Exception("Math error");
                    ret_value = 1 / Math.Tan(ret_value); break;
                case "Sec":
                case "sec":
                    if (Math.Cos(ret_value) == 0)
                        throw new Exception("Math error");
                    ret_value = 1 / Math.Cos(ret_value); break;
                case "Cosec":
                case "cosec":
                    if (Math.Sin(ret_value) == 0)
                        throw new Exception("Math error");
                    ret_value = 1 / Math.Sin(ret_value); break;
                case "Log":
                case "log":
                    if (Math.Log10(ret_value) == 0)
                        throw new Exception("Math error");
                    ret_value = Math.Log10(ret_value); break;
                case "ln":
                case "Ln":
                    if (Math.Log(ret_value) == 0)
                        throw new Exception("Math error");
                    ret_value = Math.Log(ret_value); break;
                default: throw new Exception("Couldn't evaluate operatored entity");
            }
            return ret_value;
        }

        public void SetVariableValue(char x, double val)
        {
            MainBody?.SetVariableValue(x, val);
            Degree?.SetVariableValue(x, val);
        }
    }
}
