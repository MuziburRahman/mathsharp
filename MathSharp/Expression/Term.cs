using MathSharp.Constants;
using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using System.Linq;

namespace MathSharp.Expression
{
    public class Term : ITerm 
    {
        public List<Variable> Variables { get; }
        public double Degree
        {
            get
            {
                if (Variables.Count == 0)
                    return 0;
                else return Variables.Max(v => v.Power).Value;
            }
        }
        public ExpressionType Type { get; }
        public Expression Dady { get; }
        public Extent Range { get; }
        public string StartingOperator { get; private set; } = "+";
        public bool IsExponential { get; }

        public List<Expression> InlinedExpressions;
        private Queue<char> BracketStack;

        public Term(Expression expr, ref int index)
        {
            if (index >= expr.Body.Length)
                throw new Exception("Index is larger than the expression body length! 😂😂");

            int start = index;
            Dady = expr;
            Variables = new List<Variable>();
            InlinedExpressions = new List<Expression>();
            BracketStack = new Queue<char>();
            //WritableExtent Number = null;

            List<ExpressionType> types = new List<ExpressionType>();
            /// all the chars before the index position ae considered to be done analyzed;
            /// define a starting position and length of the term

            do
            {
                char c = expr.Body[index];

                if (char.IsWhiteSpace(c))
                {
                    if (index == start)
                        start++;
                    continue;
                }
                
                //if (c.IsDigit() || c == '.')
                //{
                //    if (Number is null) Number = new WritableExtent(index, index + 1);
                //    else Number.End++;
                //}

                else if (c.IsAlphabet())
                {
                    /// operator check
                    if (expr.Body.Length >= index + 2 && expr.Range.End >= index + 2)
                    {
                        string NextThreeLetter = expr.Body.Substring(index, 3);

                        if (NextThreeLetter.IsTrigonomatricOperator() && !types.Contains(ExpressionType.Trigonometric))
                        {
                            types.Add(ExpressionType.Trigonometric);
                            index += 2;
                        }
                        else if (!types.Contains(ExpressionType.Logarithmic)
                            && (NextThreeLetter.Equals("log") || NextThreeLetter.Substring(0, 2).Equals("ln")))
                        {
                            types.Add(ExpressionType.Logarithmic);
                            index += 2;
                        }
                    }

                    else if (expr.Body.Length >= index + 2 && expr.Range.End >= index + 2 && expr.Body.Substring(index, 2).Equals(Operators.NaturalLogarithm))
                        index++;

                    /// variable check
                    else
                    {
                        if (Variables.All(v => v.Sign != c))
                            Variables.Add(new Variable(c));

                        if (expr.Body.Length > index + 2 && expr.Body[index + 1] == '^') // Range won't be allowed here 'cause Range hasn't be found yet
                        {
                            /// if power is a number
                            if (expr.Body[index + 2].IsDigit())
                            {

                                index++; /// passed ^ operator

                                while (expr.Body[index].IsDigit() || expr.Body[index] == '.')
                                    index++;
                            }
                        }
                    }
                }

                else if (c.IsStartingBracket())
                {
                    //if (c.IsStartingBracket())
                    //{
                    //    BracketStack.Enqueue(c);

                    //}
                    //else if (BracketStack.Count > 0)
                    //{
                    //    BracketStack.Dequeue();
                    //}
                    //else break;
                    Expression ex = new Expression(expr.Body, index, c);
                    InlinedExpressions.Add(ex);
                    index += ex.Range.Length;
                }

                //else if (c.IsAdditiveOperator())
                //{
                //    if (index == start)
                //        StartingOperator = c.ToString();
                //    else if (index != start && BracketStack.Count == 0) break;
                //}
                else if (c == '^')
                {

                }
            } while ((++index < expr.Body.Length));

            // 
            if (index == start)
                throw new Exception("Couldn't hit a start point in the given expring");
            Range = new Extent(start, index);

            #region Determine ExpressionType
            /// covers trigonometric, logarithmic, calculus (?)
            if (types.Count > 0)
            {
                if (types.Count == 1)
                    Type = types[0];
                else Type = ExpressionType.Complex;
            }
            ///now polynomial
            else if (Variables.Count > 0 && Degree > 0)
                Type = ExpressionType.Polynomial;
            else Type = ExpressionType.Constant;
            #endregion
        }





        public double EvaluateFor((char variable, double value)[] valuePairs)
        {
            #region error handling
            if (valuePairs is null)
            {
                if (Degree > 0)
                    throw new Exception("The degree is larger than 0 , yet you fed nothing?");
            }
            else
            {
                if (valuePairs.Length < Variables.Count)
                    throw new Exception("Variable count mismatch 😒😒");

                foreach (var valuePair in valuePairs)
                {
                    var var = Variables.FirstOrDefault(v => v.Sign == valuePair.variable);

                    if(var is null)
                        throw new Exception("No such variable");

                    var.Value = valuePair.value;
                }

                if (Variables.Any(v => double.IsNaN(v.Value)))
                    throw new Exception("No enough value to evaluate");

                if (string.IsNullOrEmpty(Dady.Body))
                    throw new Exception("The string is empty! 😂😂");
            }


            #endregion


            int index = Range.Start;
            BracketStack.Clear();
            WritableExtent number1 = null;
            bool IsNum1Complete = false;
            char awaitingVar = '\0';
            double ret_value = 1;
            string awaitingOperator = Operators.MULTIPLY;

            /// if iteration enters into a bracket, then this queue get larger
            Queue<double> ValueQueue = new Queue<double>();

            do
            {
                char c = Dady.Body[index];

                if (c.IsDigit() || c == '.')
                {
                    if (awaitingVar != '\0' && awaitingOperator != Operators.POWER)
                        EvaluateValue();
                    if (number1 is null) number1 = new WritableExtent(index, index + 1);
                    else if (!IsNum1Complete) number1.End++;
                }

                else if (c.IsMutiplicativeOperator())
                {
                    EvaluateValue();
                    awaitingOperator = c.ToString();
                }

                else if (c == '^')
                {
                    if (number1 is null)
                    {
                        if (awaitingVar == '\0')
                            throw new Exception("Seek God's help to solve this problem 😂😂");
                        else
                            ValueQueue.Enqueue(Variables[awaitingVar].Value);
                    }
                    else
                    {
                        ValueQueue.Enqueue(double.Parse(Dady.Body.Substring(number1.Start, number1.End - number1.Start)));
                        number1 = null;
                    }
                    awaitingOperator = c.ToString();
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
                        Expression ex = new Expression(Dady.Body, index, c);
                        ret_value *= ex.EvaluateFor(valuePairs);
                        index += ex.Range.Length;
                    }
                    else break;

                }

                else if (c.IsAdditiveOperator())
                {
                    if (index != Range.Start)
                        throw new Exception("You should never come here");
                }

            } while ((++index < Range.End));

            EvaluateValue();
            return StartingOperator == "+" ? ret_value : -ret_value;

            void EvaluateValue(double val = double.NaN)
            {
                if (double.IsNaN(val) // the argument wasn't set
                    && 
                   (number1 is null || !double.TryParse(Dady.Body.Substring(number1.Start, number1.End - number1.Start), out val)))
                {
                    if (Variables.Any(v=> v.Sign == awaitingVar) && !(Variables[awaitingVar] is null))
                    {
                        val = Variables[awaitingVar].Value;
                        awaitingVar = '\0';
                    }
                    else return;
                }
                if (awaitingOperator == Operators.MULTIPLY)
                    ret_value *= val;
                else if (awaitingOperator == Operators.DIVIDE)
                {
                    if (val == 0)
                        throw new Exception("You aren't allowed to divide anything by zero! Don't you believe in God?!");
                    else ret_value /= val;
                }
                else if (awaitingOperator == Operators.POWER)
                {
                    ret_value *= Math.Pow(ValueQueue.Dequeue(), val);
                }
                number1 = null;
            }
        }



        public static bool operator ==(Term a, Term b)
        {
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(Term a, Term b)
        {
            return a.CompareTo(b) != 0;
        }

        public override string ToString()
        {
            return $"Range = {Range.Start},{Range.End}";
        }

        public int CompareTo(ITerm other)
        {
            if (this is null || other is null)
                return 1;
            if (Variables.Except(other.Variables).Count() > 0)
                return 1;
            if(other is Expression)
            {
                // then we have to normalize that expression and this term and this term
                // those will be implemented in future. for now, return 1
                return 1;
            }
            else if(other is Term t)
            {
                if (InlinedExpressions.Count != t.InlinedExpressions.Count)
                    return 1;
                if (InlinedExpressions.Count > 0)
                {
                    for (int i = 0; i < InlinedExpressions.Count; i++)
                        if (t.InlinedExpressions.All(ex => ex != InlinedExpressions[i]))
                            return 1;
                }
                return 0;
            }
            return 1;
        }
    }
}
