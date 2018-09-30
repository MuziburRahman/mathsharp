using System;
using System.Diagnostics;
using MathSharp.Expression;

namespace ConsoleApp
{
    public class Person : IComparable<Person>
    {
        public double Height { get; set; }

        public int CompareTo(Person other)
        {
            if (Height == other.Height) return 0;
            if (Height < other.Height) return 1;
            return -1;
        }

        public static bool operator >(Person a, Person b) => a.Height > b.Height;
        public static bool operator <(Person a, Person b) => a.Height < b.Height;
    }

    class Program
    { 
        static void Main(string[] args)
        {
            //Stopwatch watch = new Stopwatch();

            //watch.Start();
            //for(int i = 0; i<= 100_000; i++)
            //{
            //    Expression expr = new Expression("6^2 + x^2");
            //    var t = expr.EvaluateFor(new (char, double)[] {('x', 8)});
            //}
            //watch.Stop();

            Person pa = new Person { Height = 12 };
            Person pb = new Person { Height = 13 };

            Console.WriteLine("Total time = {0}", pa > pb);
            Console.ReadKey();
        }
    }
}
