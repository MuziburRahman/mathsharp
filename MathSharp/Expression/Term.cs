using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;

namespace MathSharp.Expression
{
    public class Term : ITerm
    {
        public Dictionary<char, double?> Variables { get; }
        public int Degree { get; } = 0;
        public ExpressionType Type { get; } = ExpressionType.Constant;
        public Expression Dady { get; }
        public Extent Range { get; }
        public char StartingOperator { get; private set; } = '+';

        private Queue<char> BracketStack ;
        public Term(Expression expr, ref int index)
        {
            if (index >= expr.Body.Length)
                throw new Exception("Index is larger than the expression body length! 😂😂");

            int start = index;
            Dady = expr;
            Variables = new Dictionary<char, double?>();
            BracketStack = new Queue<char>();

            // all the chars before the index position ae considered to be done analyzed;
            // define a starting position and length of the term

            do
            {
                char c = expr.Body[index];
                if (char.IsWhiteSpace(c))
                {
                    if (index == start)
                        start++;
                    continue;
                }
                if (c.IsAlphabet() && !Variables.ContainsKey(c))
                {
                    Variables.Add(c, null);
                    
                    // evaluate degree
                }
                else if (c.IsBracket())
                {
                    if (c.IsStartingBracket())
                    {
                        BracketStack.Enqueue(c);

                    }
                    else if (BracketStack.Count > 0)
                    {
                        BracketStack.Dequeue();
                    }
                    else break;

                    //if (!c.TryGetInverse(out char inverseChar))
                    //    throw new Exception($"No inverse character found for {c}");
                    //while (index < expr.Body.Length)
                    //{
                    //    index++;
                    //    if (expr.Body[index] != inverseChar)
                    //    {
                    //        index++;
                    //        break;
                    //    }
                    //}
                }
                else if (c.IsAdditiveOperator())
                {
                    if (index == start )
                        StartingOperator = c;
                    else if (index != start && BracketStack.Count == 0) break;
                }
            } while ((++index < expr.Body.Length));

            // 
            if (index == start)
                throw new Exception("Couldn't hit a start point in the given expring");
            Range = new Extent(start, index);
        }





        public double EvaluateFor((char variable, double value)[] valuePairs)
        {
            #region error handling

            if (valuePairs.Length < Variables.Count)
                throw new Exception("Variable count mismatch 😒😒");

            foreach(var valuePair in valuePairs)
            {
                //if (!Variables.ContainsKey(valuePair.variable))
                //    throw new Exception($"There's no varibale as {valuePair.variable}");
                //if (valuePair.value is char )
                //{
                //    if(char.Equals(valuePair.variable, valuePair.value))
                //        throw new Exception($"Ok, so, the value of {valuePair.variable} is {valuePair.value} ? WTF !!");

                //}
                //else if(valuePair.value is double || valuePair.value is int || valuePair.value is float)
                //{

                //}
                if (Variables.ContainsKey(valuePair.variable))
                    Variables[valuePair.variable] = valuePair.value;
                //else throw new Exception("Hey moron! Check what you feed me 👿👿");
            }

            if (Variables.Values.Any(val => val is null))
                throw new Exception("No enough value to evaluate");

            if (string.IsNullOrEmpty(Dady.Body))
                throw new Exception("The string is empty! 😂😂");

            #endregion

            // now the evaluation starts
            int index = Range.Start;
            BracketStack.Clear();
            WritableExtent number1 = null;
            bool IsNum1Complete = false;
            char awaitingVar = '\0';
            double ret_value = 1;
            char awaitingOperator = '*';

            // if iteration enters into a bracket, then this queue get larger
            Queue<double> ValueQueue = new Queue<double>();

            do
            {
                char c = Dady.Body[index];

                if(c.IsDigit() || c == '.')
                {
                    if (awaitingVar != '\0')
                        EvaluateValue();
                    if (number1 is null) number1 = new WritableExtent(index, index + 1);
                    else if (!IsNum1Complete) number1.End++;
                    //else if (awaitingVar is null) awaitingVar = new Extent(index, index + 1);
                    //else awaitingVar.End++;
                }
                else if(c.IsMutiplicativeOperator() || c == '^')
                {
                    EvaluateValue();
                    
                    awaitingOperator = c;
                }

                else if (c.IsAlphabet())
                {
                    if (!(number1 is null))
                    {
                        EvaluateValue();
                    }
                    awaitingVar = c;
                }
                else if (c.IsBracket())
                {
                    if (c.IsStartingBracket())
                    {
                        BracketStack.Enqueue(c);
                        ValueQueue.Enqueue(ret_value);
                        Expression ex = new Expression(Dady.Body, index, c);
                        ret_value *= ex.EvaluateFor(valuePairs);
                        index += ex.Range.Length;
                    }
                    else
                    {
                        BracketStack.Dequeue(); // we should never hit this
                    }
                    
                }
                else if (c.IsAdditiveOperator())
                {
                    if(index != Range.Start)
                    //we should never come here
                        throw new Exception("You should never come here");
                }
            } while ((++index < Range.End));

            EvaluateValue();
            return StartingOperator == '+' ? ret_value : -ret_value;

            void EvaluateValue()
            {
                double val = 0;
                if (number1 is null || !double.TryParse(Dady.Body.Substring(number1.Start, number1.End - number1.Start), out val))
                {
                    if (Variables.ContainsKey(awaitingVar) && !(Variables[awaitingVar] is null))
                    {
                        val = Variables[awaitingVar].Value;
                        awaitingVar = '\0';
                    }
                    else return;
                }
                if (awaitingOperator == '*')
                    ret_value *= val;
                else if (awaitingOperator == '/')
                {
                    if (val == 0)
                        throw new Exception("You aren't allowed to divide anything by zero! Don't you believe in God?!");
                    else ret_value /= val;
                }
                else if (awaitingOperator == '^')
                    ret_value = Math.Pow(ret_value, val); // error
                number1 = null;
            }
        }


        public override string ToString()
        {
            return $"Range =[{Range.Start},{Range.End}]"; 
        }
    }
}
