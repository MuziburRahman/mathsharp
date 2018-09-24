using System;
using MathSharp.Analyzer;
using MathSharp.Expression;

namespace ConsoleApp
{
    class Program
    {
        static ExpressionAnalyzer expr = new ExpressionAnalyzer("2x*9y + 78/z + 6");
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var (start, end) = expr.NextTerm();
            var j = expr.NextTerm();
        }
    }
}
