
using MathSharp.Entities;

namespace MathSharp.Interface
{
    public interface IEntity 
    {
        string Body { get; }
        Variable Variable { get; }
    }
}
