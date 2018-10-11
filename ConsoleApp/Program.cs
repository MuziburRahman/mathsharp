using System;
using System.Collections.Generic;
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

            string[] TestStrings = new[]
            {
                "2x^2*log(5x) - sin(3x)", // x = 9
                "log150/x + 78/x^2",

            };

            double[] Solutions = new[]
            {
                266.86,
                1.2
            };
            var ValueOfX = 9;
            var VarList = new List<Variable> { new Variable('x', 9) };

            for (int i = 0; i < TestStrings.Length; i++)
            {
                Expression ex = new Expression(TestStrings[i]);
                Console.WriteLine(TestStrings[i] + "  for x = "+ValueOfX);

                try
                {
                    double val = ex.EvaluateFor(VarList);
                    Console.Write("Value obtained = " + val);
                    Console.WriteLine(" , Actual value = "+Solutions[i]);
                    if (val - Solutions[i] < 0.01)
                        Pass();
                    else Fail();
                }
                catch (Exception)
                {
                    Fail();
                }
                Console.Write('\n');
            }

            void Pass()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" PASSED");
                Console.ForegroundColor = ConsoleColor.White;
            }

            void Fail()
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" FAILED");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.ReadKey();
        }
    }
}
