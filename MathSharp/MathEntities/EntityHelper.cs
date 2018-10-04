using MathSharp.Entities;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Collections.Generic;

namespace MathSharp.MathEntities
{
    public static class EntityHelper
    {

        public static ITerm NextTerm(this string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            List<char> mult_oprtrs = new List<char>();
            int start = index;
            List<ITerm> termPool = new List<ITerm>();

            do
            {
                str.PassWhiteSpace(ref index);
                char c = str[index];
                
                if (c.IsMutiplicativeOperator())
                {
                    mult_oprtrs.Add( c);
                    index++;
                    continue;
                }
                if (c.IsAdditiveOperator())
                {
                    if (index == start)
                        index++;
                    else  break;
                }

                else if (c.IsEndingBracket())
                    break;

                termPool.Add(str.NextSingleTerm(ref index));
                if (termPool.Count > mult_oprtrs.Count)
                    mult_oprtrs.Add('*');

            } while ((index < str.Length));

            if (termPool.Count == 1)
                return termPool[0];

            else return new TermPool(termPool, mult_oprtrs);
        }

        
        public static ITerm NextSingleTerm(this string str, ref int index)
        {
            int start = index;

            str.PassWhiteSpace(ref index);

            char c = str[index];

            if (c.IsDigit() || c == '.')
            {
                Entity ent = new Entity(str, ref index);

                if (str.Length > index + 2 && str[index] == '^')
                {
                    index++;
                    return new ExponentTerm(ent, str.NextTerm(ref index));
                }
                return ent;
            }

            else if (c.IsAlphabet())
            {
                ///a start with alphabet might be an operatoredTerm or variable

                /// operator check
                if (str.IsMathematicalOperator(ref index, out string oprtr))
                {
                    return new OperatoredEntity(oprtr, str, ref index);
                }

                /// variable check
                else
                {
                    Entity ent = new Entity(str, ref index);

                    if (str.Length > index  && str[index] == '^') /// Range won't be allowed here 'cause Range hasn't be found yet
                    {
                        index++;
                        return new ExponentTerm(ent, str.NextSingleTerm(ref index));
                    }
                    return ent;
                }
            }

            else if (c.IsStartingBracket())
            {
                return new Expression(str, ref index);
            }

            //else if(c.IsEndingBracket() && str[start].TryGetInverse(out char inv))
            //{
            //    if(inv == c)
            //    {
            //        index++;
                    
            //    }
            //}

            throw new Exception("Couldn't extract single term " );
        }

    }
}
