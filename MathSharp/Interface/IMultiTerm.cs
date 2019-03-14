using MathSharp.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MathSharp.Interface
{
    internal interface IMultiTerm
    {
        //double CoEfficientOf(TermBase term);
        double ConstantPart { get; }
        TermBase WithoutConstantPart();
        TermBase this[int i] { get; }
        char Operator(int i);
        void RemoveAt(int i);
        bool Remove(TermBase child);
        void Add(TermBase term, char oprtr);
        int Count { get; }
    }

    internal interface IDegreeOf
    {
        int Degree { get; }
    } 

    internal interface IExpandable
    {
        TermBase Expand();
    }

    internal interface IInflatenable
    {
        TermBase Inflate();
    }
}
