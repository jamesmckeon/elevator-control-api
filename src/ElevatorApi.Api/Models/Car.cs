using Microsoft.VisualBasic;

namespace ElevatorApi.Api.Models;

public sealed class Car: IEquatable<Car>
{
    public byte Id { get; init; }
    public IReadOnlyCollection<sbyte> DestinationFloors { get; } = new List<sbyte>();
    public sbyte? NextFloorNumber { get; private set; }
    public bool? Ascending  { get; private set; }

    public bool Equals(Car? other)
    {
        return  other != null && Id == other.Id;

    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Car);
    }
}