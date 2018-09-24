using System.Text;

namespace MathSharp.Expression
{
    public class Expression
    {
        public char[] Variables;
        public int Degree = 0;


        private StringBuilder Expr_builder;

        public Expression(string expr)
        {
            Expr_builder = new StringBuilder(expr);
        }
    }
}
