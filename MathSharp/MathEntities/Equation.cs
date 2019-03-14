using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MathSharp.MathEntities
{
    public class Equation : TermBase
    {
        public TermBase LeftSide { get; private set; }

        public TermBase RightSide { get; private set; }

        public Equation(TermBase lhs, TermBase rhs)
        {
            if (lhs is null || rhs is null)
                throw new ArgumentNullException("lhs", "Invalid equation, lhs or rhs was null");

            LeftSide = lhs;
            RightSide = rhs;

            Variables = LeftSide.Variables.Union(RightSide.Variables).ToList();
            if (Variables.Count == 0)
                throw new Exception("Equation found without any variable in it");
            Type = lhs.Type.CombineWith(rhs.Type);
        }

        public Equation(string str)
        {
            if (str.Length < 3)
                throw new Exception("Invalid equation");

            int index = 0;
            LeftSide = new Expression(str, ref index).Inflate();
            str.PassWhiteSpace(ref index);

            if (str[index] != '=')
                throw new Exception("Invalid equation");

            index++;
            RightSide = new Expression(str, ref index).Inflate();

            Variables = new List<Variable>(LeftSide.Variables.Union(RightSide.Variables));
            if (Variables.Count == 0)
                throw new Exception("Equation found without any variable in it");
        }

        public override IList<Variable> Variables { get; protected set; }

        public override ExpressionType Type { get; protected set; }

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

        public override TermBase Integral(int degree = 1, char RegardsTo = '\0', double upperbound = double.NaN, double lowerbound = double.NaN)
        {
            throw new NotImplementedException();
        }

        public override TermBase Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return LeftSide.ToString() + "=" + RightSide.ToString();
        }
    }
}
