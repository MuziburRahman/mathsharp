using MathSharp.Entities;
using MathSharp.Enum;
using MathSharp.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MathSharp.MathEntities
{
    public class TermPool : ITerm
    {
        public ReadOnlyCollection<Variable> Variables { get; }

        public ExpressionType Type { get; }
        
        public IList<ITerm> TermList;


        private IList<char> MultiplicativeOperators;



        public TermPool(IList<ITerm> terms, IList<char> m_opertrs)
        {
            TermList = terms;
            MultiplicativeOperators = m_opertrs;
            
            Type = ExpressionType.Constant;

            IEnumerable<Variable> t = new List<Variable>();
            foreach(ITerm term in terms)
            {
                t = t.Union(term.Variables);
                Type = Type.CombineWith(term.Type);
            }
            Variables = new ReadOnlyCollection<Variable>(t.ToList());
        }




        public int CompareTo(ITerm other)
        {
            throw new NotImplementedException();
        }

        public double EvaluateFor(IList<Variable> valuePairs)
        {
            double ret_value = 1;
            for(int i = 0; i<TermList.Count; i++)
            {
                double val = TermList[i].EvaluateFor(valuePairs);
                if (MultiplicativeOperators[i] == '*')
                    ret_value *= val;
                else if (val == 0)
                    throw new Exception("Math error");
                else ret_value /= val;
            }
            return ret_value;
        }
    }
}
