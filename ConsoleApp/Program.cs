using System;
using System.Diagnostics;
using MathSharp.Entities;

namespace ConsoleApp
{

    class Program
    { 
        static void Main(string[] args)
        {
            //Stopwatch watch = new Stopwatch();

            //watch.Start();
            //for (int i = 0; i <= 100_000; i++)
            //{
            //    Expression expr = new Expression("6^2 + x^2");
            //    var t = expr.EvaluateFor(new (char, double)[] { ('x', 8) });
            //}
            //watch.Stop();

            //Expression expr = new Expression("2x^2*log(5x) - sin(3x)");
            //Console.WriteLine(expr.EvaluateFor(new (char, double)[] { ('x', 8) }));


            Console.WriteLine("Write an expression:");

            Expression expr = new Expression(Console.ReadLine());

            for (int i = 0; i < expr.Variables.Count; i++)
            {
                Console.Write($"Define the value for {expr.Variables[i].Sign} : ");
            TryAgain:
                if (double.TryParse(Console.ReadLine(), out double value))
                {
                    expr.Variables[i] = new Variable(expr.Variables[i].Sign, value);
                }
                else
                {
                    Console.WriteLine("Input isn't palpable, try again: ");
                    goto TryAgain;
                }
            }

            Console.WriteLine($"The value is {expr.EvaluateFor(null)}");

            //Variable a = new Variable('x');
            //Variable b = new Variable('x');

            //Console.Write(a.Equals(b));

            Console.ReadKey();
        }
    }
}
