
using MathSharp.Interface;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MathSharp.Entities
{
    public class Variable 
    {
        public double Value { get; }

        public char Sign { get; }


        public Variable(char sign, double value)
        {
            Sign = sign;
            Value = value;
        }

        public override string ToString()
        {
            return "[ " + Sign + " = " + Value + " ]";
        }
        

        public static List<Variable> CombineVariables(IEnumerable<ITerm> terms)
        {
            if (terms is null)
                throw new Exception("Term shouldn't be null");


            var itrtr = terms.GetEnumerator();
            itrtr.MoveNext();

            IEnumerable<Variable> vlist = itrtr.Current.Variables;
            while (itrtr.MoveNext())
            {
                vlist = vlist.Union(itrtr.Current.Variables);
            }

            return vlist.ToList();
        }

        public int CompareTo(Variable other)
        {
            if (Sign == other.Sign)
                return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            if (obj is Variable v)
            {
                return CompareTo(v) == 0;

            }
            return false;
        }

        public override int GetHashCode()
        {
            return Sign.GetHashCode();
        }

        //public static bool operator ==(Variable a, Variable b)
        //{
        //    return a.CompareTo(b) == 0;
        //}

        //public static bool operator !=(Variable a, Variable b)
        //{
        //    return a.CompareTo(b) == 0;
        //}
    }
}
