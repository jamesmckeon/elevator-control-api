using ElevatorApi.Api.Config;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace ElevatorApi.Api.Models;

public sealed class Car : IEquatable<Car>
{
    internal Car(byte id, sbyte initialFloor, sbyte minFloor, sbyte maxFloor)
    {
        if (minFloor >= maxFloor)
        {
            throw new ArgumentOutOfRangeException(nameof(minFloor), 
                "minFloor must be less than maxFloor.");
        }

        Id = id;
        CurrentFloor = initialFloor;
        FloorRange = (minFloor, maxFloor);
        Stops = new List<sbyte>();
    }

    private (sbyte MinFloor, sbyte MaxFloor) FloorRange { get; }
    public byte Id { get; }
    public IReadOnlyCollection<sbyte> Stops { get; }
    public sbyte CurrentFloor { get; private set; }
    public sbyte? NextFloor { get; private set; }

    public bool Equals(Car? other)
    {
        return other != null && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Car);
    }

    public void MoveToNextFloor()
    {
        throw new NotImplementedException();
    }

    public void AddStop(sbyte floorNumber)
    {
        throw new NotImplementedException();
    }
}