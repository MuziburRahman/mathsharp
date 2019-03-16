# MathSharp

Evaluate mathematical expression with MathSharp.
Suported features:

```
TermBase term = "2(x+y^2)^2 + 3(y+2)^2 + 2y^2";

 if(term is Expression expression)
{
   Console.WriteLine(expression.CoEfficientOf("y^2"));
   Console.WriteLine(expression.EvaluateFor(('x', 2), ('y', 3)));
   Console.WriteLine(expression.Expand());
}
```

**Output**:  
  2  
  335  
  (5y^2 + 2y^4 + 4xy^2 + 2x^2 + 12y + 12)
