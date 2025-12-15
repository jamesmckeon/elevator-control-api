using ElevatorApi.Api.Config;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;

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
        AscendingStops = new();
        DescendingStops = new();
    }

    private (sbyte MinFloor, sbyte MaxFloor) FloorRange { get; }
    public byte Id { get; }

    public IReadOnlyCollection<sbyte> Stops => GetStops();

    public sbyte CurrentFloor { get; private set; }
    public sbyte? NextFloor => Stops.Count > 0 ? Stops.First() : null;
    private bool? Ascending { get; set; }
    private SortedSet<sbyte> AscendingStops { get; }
    private SortedSet<sbyte> DescendingStops { get; }

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

    public void AddStop(sbyte floorNumber)
    {
        if (floorNumber < FloorRange.MinFloor || floorNumber > FloorRange.MaxFloor)
        {
            throw new ArgumentOutOfRangeException(nameof(floorNumber),
                $"floorNumber must be between {FloorRange.MinFloor} and {FloorRange.MaxFloor}.");
        }

        if (!Stops.Contains(floorNumber) && floorNumber != CurrentFloor)
        {
            if (floorNumber < CurrentFloor)
            {
                DescendingStops.Add(floorNumber);
            }
            else
            {
                AscendingStops.Add(floorNumber);
            }

            if (!Ascending.HasValue)
            {
                Ascending = floorNumber > CurrentFloor;
            }
        }
    }

    private ReadOnlyCollection<sbyte> GetStops()
    {
        //TODO Refactor This!

        // ascending by default if idle
        return !Ascending.HasValue || Ascending == true
            ? AscendingStops.Concat(DescendingStops.Reverse()).ToList().AsReadOnly()
            : DescendingStops.Reverse().Concat(AscendingStops).ToList().AsReadOnly();
    }

    public void MoveNext()
    {
        if (NextFloor.HasValue)
        {
            CurrentFloor = NextFloor.Value;
            RemoveStop(CurrentFloor);
        }
    }

    private void RemoveStop(sbyte floorNumber)
    {
        DescendingStops.Remove(floorNumber);
        AscendingStops.Remove(floorNumber);
    }
}