namespace ElevatorApi.Api.Models;

public sealed class Floor: IEquatable<Floor>
{
    public byte Number { get; init; }
    public bool Equals(Floor? other)
    {
        if (other == null)
        {
            return false;
        }
        
        return Number.Equals(other.Number);
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return Equals(obj as Floor);
    }
}