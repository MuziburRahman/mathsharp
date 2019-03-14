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
    public class OperatoredTerm : TermBase
    {
        public string Operator { get; }

        //public TermBase Degree { get; private set; }

        public TermBase MainBody { get; private set; }
        

        public override IList<Variable> Variables { get; protected set; }

        public override ExpressionType Type { get; protected set; }


        /// <param name="index">the index from where main body starts, excluding operator</param>
        public OperatoredTerm(string op, TermBase term)
        {
            Operator = op;

            MainBody = term;

            ///other stuffs
            Variables = MainBody.Variables;

            if (term.Type == ExpressionType.Constant)
                Type = ExpressionType.Constant;

            else switch (op)
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


        public int CompareTo(TermBase other)
        {
            throw new NotImplementedException();
        }

        public override double EvaluateFor(params Variable[] valuePairs)
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

        public override string ToString()
        {
            return Operator + MainBody.ToString();
        }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            throw new NotImplementedException();
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }

        public override TermBase Clone()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(OperatoredTerm a, OperatoredTerm b)
        {
            if (a.Type == ExpressionType.Constant && a.Type == b.Type)
                return a.EvaluateFor(null) == b.EvaluateFor(null);
            return a.Operator == b.Operator && a.MainBody == b.MainBody;
        }
        public static bool operator !=(OperatoredTerm a, OperatoredTerm b)
        {
            return !(a == b);
        }
    }
}
