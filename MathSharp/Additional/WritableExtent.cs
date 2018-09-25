
public class WritableExtent
{
    /// <summary>Start value of the Extent.</summary>
    public int Start;

    /// <summary>End value of the Extent.</summary>
    public int End;

    public WritableExtent(int start, int end)
    {
        Start = start;
        End = end;
    }


    /// <summary>Presents the Extent in readable format.</summary>
    /// <returns>String representation of the Extent</returns>
    public override string ToString()
    {
        return string.Format("[{0} - {1}]", this.Start, this.End);
    }

    /// <summary>Determines if the Extent is valid.</summary>
    /// <returns>True if Extent is valid, else false</returns>
    public bool IsValid()
    {
        return this.Start.CompareTo(this.End) <= 0;
    }

    /// <summary>Determines if the provided value is inside the Extent.</summary>
    /// <param name="value">The value to test</param>
    /// <returns>True if the value is inside Extent, else false</returns>
    public bool ContainsValue(int value)
    {
        return (this.Start.CompareTo(value) <= 0) && (value.CompareTo(this.End) <= 0);
    }

    /// <summary>Determines if this Extent is inside the bounds of another Extent.</summary>
    /// <param name="WritableExtent">The parent Extent to test on</param>
    /// <returns>True if Extent is inclusive, else false</returns>
    public bool IsInsideExtent(Extent Extent)
    {
        return this.IsValid() && Extent.IsValid() && Extent.ContainsValue(this.Start) && Extent.ContainsValue(this.End);
    }

    /// <summary>Determines if another Extent is inside the bounds of this Extent.</summary>
    /// <param name="Extent">The child Extent to test</param>
    /// <returns>True if Extent is inside, else false</returns>
    public bool ContainsExtent(Extent Extent)
    {
        return this.IsValid() && Extent.IsValid() && this.ContainsValue(Extent.Start) && this.ContainsValue(Extent.End);
    }
}