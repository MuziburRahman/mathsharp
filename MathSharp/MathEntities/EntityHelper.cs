using MathSharp.Entities;
using MathSharp.Extensions;
using MathSharp.Interface;
using System;
using System.Collections.Generic;

namespace MathSharp.MathEntities
{
    public static class EntityHelper
    {

        public static TermBase NextTerm(this string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            int start = index;
            List<(TermBase, char)> termPool = new List<(TermBase, char)>();

            char oprtr = '\0';
            do
            {
                str.PassWhiteSpace(ref index);
                char c = str[index];

                if (c.IsMutiplicativeOperator())
                {
                    oprtr = c;
                    index++;
                    continue;
                }
                if (c.IsAdditiveOperator())
                {
                    if (index == start)
                        index++;
                    else
                        break;
                }

                else if (c.IsEndingBracket() || c == '=')
                    break;

                termPool.Add((str.NextSingleTerm(ref index), oprtr == '\0' ? '*' : oprtr));
                oprtr = '\0';

            } while ((index < str.Length));


            if (termPool.Count == 1)
                return termPool[0].Item1;

            else
                return new TermPool(termPool.ToArray()).Inflate();
        }

        public static TermBase NextSingleTerm(this string str, ref int index)
        {
            str.PassWhiteSpace(ref index);

            char c = str[index];

            if (c.IsDigit() || c == '.')
            {
                Entity ent = new Entity(str, ref index); /// entity like 24.2756

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
                    /// degree , if any
                    str.PassWhiteSpace(ref index);
                    if (str[index] == '^')
                    {
                        index++;
                        var degree = str.NextSingleTerm(ref index);

                        return new ExponentTerm(new OperatoredTerm(oprtr, str.NextSingleTerm(ref index)), degree);
                    }

                    return new OperatoredTerm(oprtr, str.NextSingleTerm(ref index));
                }

                /// variable check
                else
                {
                    Entity ent = new Entity(str, ref index);

                    if (str.Length > index && str[index] == '^') /// Range won't be allowed here 'cause Range hasn't be found yet
                    {
                        index++;
                        return new ExponentTerm(ent, str.NextSingleTerm(ref index));
                    }
                    return ent;
                }
            }

            else if (c.IsStartingBracket())
            {
                var exp = new Expression(str, ref index);
                str.PassWhiteSpace(ref index);
                if (str.Length > index && str[index] == '^')
                {
                    index++;
                    return new ExponentTerm(exp, str.NextSingleTerm(ref index));
                }
                return exp;
            }

            throw new Exception("Couldn't extract single term ");
        }
    }
}
