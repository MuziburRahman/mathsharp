
using MathSharp.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MathSharp.Entities
{
    public struct Variable
    {
        public readonly double Value;
        public readonly char Character;

        public Variable(char sign, double value)
        {
            Character = sign;
            Value = value;
        }

        public override string ToString()
        {
            if (Character == '\0')
                return Value.ToString();
            return Character + " = " + Value;
        }


        //public static ReadOnlyCollection<Variable> CombineVariables(IEnumerable<TermBase> terms)
        //{
        //    if (terms is null)
        //        throw new Exception("Term shouldn't be null");


        //    var itrtr = terms.GetEnumerator();
        //    itrtr.MoveNext();

        //    IEnumerable<Variable> vlist = itrtr.Current.Variables;
        //    while (itrtr.MoveNext())
        //    {
        //        vlist = vlist.Union(itrtr.Current.Variables);
        //    }

        //    return new ReadOnlyCollection<Variable>(vlist.ToList());
        //}

        //public int CompareTo(Variable other)
        //{
        //    if (Character == '\0')
        //        return Value.CompareTo(other.Value);
        //    return Character.CompareTo(other.Character);
        //}

        public override bool Equals(object obj)
        {
            if (obj is Variable v)
            {
                if (Character == '\0')
                    return Value == v.Value;
                return Character == v.Character;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 2029806762;
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            hashCode = hashCode * -1521134295 + Character.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Variable a, Variable b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Variable a, Variable b)
        {
            return !(a == b);
        }

        //public static implicit operator Variable((char, double) d)
        //{
        //    return new Variable(d.Item1, d.Item2);
        //}

    }
}
