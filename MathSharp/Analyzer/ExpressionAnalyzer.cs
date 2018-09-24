using MathSharp.Extensions;
using System;

namespace MathSharp.Analyzer
{

    public class ExpressionAnalyzer
    {
        public int index ;

        /// <summary>
        /// if the iterator is in the power of an expression, it's position is raised,
        /// like if the iterator position is 5 at 2x^(5y+6), then it's psotion is 1
        /// baselin's postion is 0
        /// </summary>
        private int curPosition;
        private string MainString;

        public ExpressionAnalyzer(string str)
        {
            MainString = str;
            index = 0;
        }



        public (int start, int end) NextTerm()
        {
            if (index >= MainString.Length)
                return (-1, -1);
            
            // all the chars before the index position ae considered to be done analyzed;
            // define a starting position and length of the term

            int start = -1;
           
            do
            {
                char c = MainString[index];
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                else if (c.IsDigit() || c == '.' || c.IsMutiplicativeOperator() || c == '^')
                {
                    if (start == -1)
                        start = index;
                }
                else if (c.IsStartingBracket())
                {
                    if (!c.TryGetInverse(out char inverseChar))
                        throw new Exception("Inverse of given bracket isn't found");
                    while (index < MainString.Length)
                    {
                        index++;
                        if (MainString[index] != inverseChar)
                        {
                            index++;
                            break;
                        }
                    }
                }
                else if (c.IsAdditiveOperator())
                {
                    if (start == -1)
                        throw new Exception("Couldn't hit a start point in the given string");
                    index++;
                    return (start, index - 1); // the only valid return
                }
            } while ((++index < MainString.Length));

            // 
            if (start == -1)
                throw new Exception("Couldn't hit a start point in the given string");
            return (start, index);
        }

    }


}
