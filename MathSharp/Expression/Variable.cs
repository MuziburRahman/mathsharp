
using System;

namespace MathSharp.Expression
{
    public class Variable : IComparable<Variable>
    {
        public double Value { get; set; } = double.NaN;

        public Variable Power { get; set; } // currently doesn't support expression or term as power 

        public char Sign { get; }


        public Variable(char sign)
        {
            Sign = sign;
        }

        public static bool operator ==(Variable a, Variable b) => a.CompareTo(b) == 0;

        public static bool operator !=(Variable a, Variable b) => a.CompareTo(b) != 0;

        public override string ToString()
        {
            return "[ " + Sign + " = " + Value + " ]";
        }

        public int CompareTo(Variable other)
        {
            if (Sign != other.Sign || Power != other.Power)
                return 1;
            return 0;
        }
    }
}
