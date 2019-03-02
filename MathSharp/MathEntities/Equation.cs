using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MathSharp.MathEntities
{
    public class Equation : TermBase
    {

        public TermBase LeftSide { get; private set; }

        public TermBase RightSide { get; private set; }

        public Equation(Expression lhs, Expression rhs)
        {
            if (lhs is null || rhs is null)
                throw new ArgumentNullException("lhs", "Invalid equation, lhs or rhs was null");

            LeftSide = lhs;
            RightSide = rhs;

            Variables = new ReadOnlyCollection<Variable>(LeftSide.Variables.Union(RightSide.Variables).ToList());
            if (Variables.Count == 0)
                throw new Exception("Equation found without any variable in it");
        }

        public Equation(string str)
        {
            if (str.Length < 3)
                throw new Exception("Invalid equation");

            int index = 0;
            LeftSide = new Expression(str, ref index).InFlate();
            str.PassWhiteSpace(ref index);

            if (str[index] != '=')
                throw new Exception("Invalid equation");

            index++;
            RightSide = new Expression(str, ref index).InFlate();

            Variables = new ReadOnlyCollection<Variable>(LeftSide.Variables.Union(RightSide.Variables).ToList());
            if (Variables.Count == 0)
                throw new Exception("Equation found without any variable in it");
        }

        public override ReadOnlyCollection<Variable> Variables { get; }

        public override ExpressionType Type { get; }

        public override TermBase Derivative(int degree = 1, char RegardsTo = '\0')
        {
            return new Equation(LeftSide.Derivative(degree, RegardsTo) as Expression, RightSide.Derivative(degree, RegardsTo) as Expression);
        }

        public override double EvaluateFor(params Variable[] valuePairs)
        {
            //if(Variables.Count == 1)
            //{
            //    if(valuePairs is null || !valuePairs.Contains(Variables[0]))
            //    {
            //        if (RightSide.Type == ExpressionType.Constant)
            //        {
            //            if (LeftSide is TermPool ltp)
            //            {
            //                var ltpnew = new List<TermBase>(ltp.Children.Count);
            //                var mopsnew = new List<char>(ltp.Operators.Count);
            //                for (int i = 0; i < ltp.Children.Count; i++)
            //                {
            //                    if (ltp.Children[i].Type == ExpressionType.Constant)
            //                    {
            //                        if (i == 0 || ltp.Operators[i - 1] == '/')
            //                            RightSide *= ltp.Children[i];
            //                        else RightSide /= ltp.Children[i];
            //                    }
            //                    else
            //                    {
            //                        ltpnew.Add(ltp.Children[i]);
            //                        if(i > 0)
            //                            mopsnew.Add(ltp.Operators[i - 1]);
            //                    }
            //                }
            //                LeftSide = new TermPool(ltpnew, mopsnew);
            //            }
            //        }
            //    }
            //    else 
            //        AssertEquality(valuePairs);

            //    return 0;
            //}

            //var VarsWithoutValue = Variables.Except(valuePairs).ToList();

            ///// if all the variables are defined, and value is given, then just assert lhs == rhs
            //if(VarsWithoutValue.Count == 0)
            //{
            //    if(!AssertEquality(valuePairs))
            //        return 0;
            //    return 1;
            //}

            ///// if one variable need to be solved, may be it's easy
            //else if(VarsWithoutValue.Count == 1)
            //{
            //    /// try to solve
                
            //}

            return 0;
        }

        bool AssertEquality(Variable[] valuePairs)
        {
            var dif = Math.Abs(LeftSide.EvaluateFor(valuePairs) - RightSide.EvaluateFor(valuePairs));
            if (dif == 0)
            {
                Console.WriteLine("Right side = Left side");
                return true;
            }

            if (dif < 0.1)
            {
                Console.WriteLine("Right side is almost same as Left side, difference = {0}", dif);
                return true;
            }
            return false;
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// true: if there is no problem to conduct next iteration,
        /// false: if no other iteration should be continued;
        /// </returns>
        bool SolutionIteration(Variable[] valuePairs)
        {
            //if (Variables.Count == 1)
            //{
            //    if (valuePairs is null || !valuePairs.Contains(Variables[0]))
            //    {
            //        if (RightSide.Type == ExpressionType.Constant)
            //        {
            //            if (LeftSide is TermPool ltp)
            //            {
            //                var ltpnew = new List<TermBase>(ltp.Children.Count);
            //                var mopsnew = new List<char>(ltp.Operators.Count);
            //                for (int i = 0; i < ltp.Children.Count; i++)
            //                {
            //                    if (ltp.Children[i].Type == ExpressionType.Constant)
            //                    {
            //                        if (i == 0 || ltp.Operators[i - 1] == '/')
            //                            RightSide *= ltp.Children[i];
            //                        else RightSide /= ltp.Children[i];
            //                    }
            //                    else
            //                    {
            //                        ltpnew.Add(ltp.Children[i]);
            //                        if (i > 0)
            //                            mopsnew.Add(ltp.Operators[i - 1]);
            //                    }
            //                }
            //                LeftSide = new TermPool(ltpnew, mopsnew);
            //            }

            //            else if (LeftSide is Expression ex)
            //            {
            //                var ltpnew = new List<TermBase>(ex.Children.Count);
            //                var mopsnew = new List<char>(ex.Operators.Count);
            //                for (int i = 0; i < ex.Children.Count; i++)
            //                {
            //                    if (ex.Children[i].Type == ExpressionType.Constant)
            //                    {
            //                        if (ex.Operators[i] == '+')
            //                            RightSide -= ex.Children[i];
            //                        else RightSide += ex.Children[i];
            //                    }
            //                    else
            //                    {
            //                        ltpnew.Add(ex.Children[i]);
            //                        if (i > 0)
            //                            mopsnew.Add(ex.Operators[i - 1]);
            //                    }
            //                }
            //                LeftSide = new TermPool(ltpnew, mopsnew);
            //            }
            //        }
            //    }
            //    else
            //        AssertEquality(valuePairs);
            //}
            return false;
        }

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }

        public override bool CanAdd(TermBase other)
        {
            throw new NotImplementedException();
        }
    }
}
