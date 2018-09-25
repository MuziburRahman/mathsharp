using System;
using MathSharp.Expression;

namespace ConsoleApp
{
    class Program
    { 
        static void Main(string[] args)
        {
            Expression expr = new Expression("2x*9*(y-2) + 78*x/z + 6");
            var t = expr.EvaluateFor(new (char, double)[] { ('x', 4), ('y', 8), ('z', 5) });
            Console.WriteLine("Hello World!");
        }
    }
}
