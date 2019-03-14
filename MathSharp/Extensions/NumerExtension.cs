using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class NumerExtension
    {
        public static double Combination(this double n, double r)
        {
            if (r > n)
                throw new InvalidOperationException("smaller number expected");
            if(n%1 != 0 || r%1 != 0)
                throw new InvalidOperationException("integer number expected");

            if (r == 1 || n == r + 1)
                return n;
            if (n == r)
                return 1;

            double result = 1;
            int iter = (int)Math.Min(r, n - r);
            for (int i = 0; i < iter; i++)
            {
                result *= (n - i) / (1 + i);
            }

            return result;
        }
    }
}
