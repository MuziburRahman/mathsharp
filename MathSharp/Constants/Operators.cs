using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Constants
{
    public static class Operators
    {
        public enum Types
        {
            Trigonomatric,
            Logarithmic
        }

        //public const string PLUS = "+";
        public const string MINUS = "-";
        public const string MULTIPLY = "*";
        public const string DIVIDE = "/";
        public const string POWER = "^";
        public const string EQUATION = "=";

        //logarithmic
        public const string LOG10 = "log";
        public const string NaturalLogarithm = "ln";

        //trigonometric

        public static bool IsTrigonomatricOperator(this string str)
        {
            switch (str)
            {
                case "sin":
                case "cos":
                case "tan":
                case "cot":
                case "sec":
                case "cosec":
                    return true;
                default: return false;
            }
        }
        
    }
}
