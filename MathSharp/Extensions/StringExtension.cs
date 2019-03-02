using MathSharp.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MathSharp.Extensions
{
    public static class StringExtension
    {

        public static bool IsMathematicalOperator(this string str, ref int index, out string oprtr)
        {
            ///two letter operator
            if (index + 2 >= str.Length)
            {
                oprtr = null;
                return false;
            }

            if (str.Substring(index, 2) == "ln")
            {
                oprtr = "ln";
                index += 2;
                return true;
            }

            /// three letter operators
            if (index + 3 >= str.Length)
            {
                oprtr = null;
                return false;
            }

            switch (str.Substring(index, 3))
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
                case "sec":
                case "Log":
                case "log":
                    oprtr = str.Substring(index, 3);
                    index += 3;
                    return true;
            }

            if (index + 5 >= str.Length)
            {
                oprtr = null;
                return false;
            }
            if (str.Substring(index, 5) == "cosec")
            {
                oprtr = "cosec";
                index += 5;
                return true;
            }

            oprtr = null;
            return false;
        }


        /// <summary>
        /// moves <paramref name="index"/> to the end of a single term, not term pool
        /// </summary>
        /// <param name="str">body</param>
        /// <param name="index">the index to start iterating from</param>
        public static void GetNextSingleEntity(this string str, ref int index)
        {
            str.PassWhiteSpace(ref index);
            if (index >= str.Length)
                return;

            Queue<char> BracketQueue = new Queue<char>();

            /// if starts with a bracket
            if (str[index].IsStartingBracket()) do
                {
                    char c = str[index];

                    if (c.IsStartingBracket())
                    {
                        BracketQueue.Enqueue(c);
                    }
                    else if (c.IsEndingBracket())
                    {
                        if (c.TryGetInverse(out char inv) && inv == BracketQueue.Dequeue())
                        {
                            if (BracketQueue.Count == 0)
                            {
                                index++;
                                break;
                            }
                        }
                        else throw new Exception("Un-necessary bracket found : " + c);
                    }
                }
                while (++index < str.Length);

            /// if starts with a number
            else if (str[index].IsDigit() || str[index] == '.')
            {
                GetNextNumberIndex(str, ref index);

                if (str.Length > index && str[index] == '^')
                {
                    index++;
                    GetNextSingleEntity(str, ref index);
                }
            }

            /// if starts with a char : variable or operator
            else if (str[index].IsAlphabet())
            {
                /// first check for possible operators, like tan or sin
                if (str.IsMathematicalOperator(ref index, out string _))
                    GetNextSingleEntity(str, ref index);

                /// now check for variables
                else if (str.Length > index + 1 && str[index + 1] == '^')
                {
                    index+=2;
                    GetNextSingleEntity(str, ref index);
                }
            }
        }


        /// <summary>
        /// if the sring starts with digit, this method works
        /// </summary>
        /// <param name="index">the index to start iteration from</param>
        public static void GetNextNumberIndex(this string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            for (; index < str.Length && str[index].IsDigit() || str[index] == '.'; index++) ;
        }

        public static double GetNextDecimalNumber(this string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            for (; index < str.Length && !str[index].IsDigit() && str[index] != '.' && str[index] != '-' && str[index] != '+'; index++) ;

            if (index >= str.Length)
                return double.NaN;

            bool dot_found = false;
            int start = index;

            if (str[index] == '+' || str[index] == '-')
                index++;

            if (index < str.Length && str[index] == '.')
            {
                dot_found = true;
                if (++index < str.Length && !str[index].IsDigit())
                    return double.NaN;
            }

            for (; index < str.Length && str[index].IsDigit() || (!dot_found && str[index] == '.'); index++) ;
            return double.Parse(str.Substring(start, index - start));
        }

        public static void PassWhiteSpace(this string str, ref int index)
        {
            while (index < str.Length && char.IsWhiteSpace(str[index]))
                index++;
        }
    }
}
