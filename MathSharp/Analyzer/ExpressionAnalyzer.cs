using MathSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp.Analyzer
{

    public class ExpressionAnalyzer
    {
        public int index ;


        private string MainString;

        public ExpressionAnalyzer(string str)
        {
            MainString = str;
            index = 0;
        }



        public string NextTerm()
        {
            if (index > MainString.Length)
                return string.Empty;
            
            // all the chars before the index position ae considered to be done analyzed;
            // define a starting position and length of the term

            int? start = null;
            char c = MainString[index];

            while (true)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                else if (c.IsDigit() || c=='.' || c.IsMutiplicativeOperator())
                {
                    if (start is null)
                        start = index;
                }
                else if(c.IsAdditiveOperator())

            }
        }

    }


}
