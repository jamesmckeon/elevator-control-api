using ElevatorApi.Api.Config;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ElevatorApi.Api.Models;

public record CarFloorDistance(int StopsTil, int DistanceFrom)
{
}

[DebuggerDisplay("Id = {Id}; Current={CurrentFloor}; Next={NextFloor}")]
public sealed class Car : IEquatable<Car>
{
    private readonly Lock _carLock = new();

    #region Properties

    private (sbyte MinFloor, sbyte MaxFloor) FloorRange { get; }
    public byte Id { get; }
    public IReadOnlyCollection<sbyte> Stops => GetStops();
    public sbyte CurrentFloor { get; private set; }
    public sbyte? NextFloor => Stops.Count > 0 ? Stops.First() : null;
    private bool? Ascending { get; set; }
    private SortedSet<sbyte> AscendingStops { get; }
    private SortedSet<sbyte> DescendingStops { get; }

    public CarState State
    {
        get
        {
            lock (_carLock)
            {
                if (Ascending == null)
                {
                    return CarState.Idle;
                }
                else
                {
                    return Ascending.Value ? CarState.Ascending : CarState.Descending;
                }
            }
        }
    }

    #endregion

    #region Constructors

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

    internal Car(byte id, IOptions<ElevatorSettings> settings) :
        this(id, settings.Value.LobbyFloor, settings.Value.MinFloor, settings.Value.MaxFloor)
    {
    }

    #endregion

    #region Equals

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

    #endregion

    #region Methods

    public void AddStop(sbyte floorNumber)
    {
        ValidateFloorNumber(floorNumber);

        lock (_carLock)
        {
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
    }

    private void ValidateFloorNumber(sbyte floorNumber)
    {
        if (floorNumber < FloorRange.MinFloor || floorNumber > FloorRange.MaxFloor)
        {
            throw new ArgumentOutOfRangeException(nameof(floorNumber),
                $"floorNumber must be between {FloorRange.MinFloor} and {FloorRange.MaxFloor}.");
        }
    }

    private ReadOnlyCollection<sbyte> GetStops()
    {
        lock (_carLock)
        {
            // ascending by default if idle
            return !Ascending.HasValue || Ascending == true
                ? AscendingStops.Concat(DescendingStops.Reverse()).ToList().AsReadOnly()
                : DescendingStops.Reverse().Concat(AscendingStops).ToList().AsReadOnly();
        }
    }


    public void MoveNext()
    {
        lock (_carLock)
        {
            if (NextFloor.HasValue)
            {
                CurrentFloor = NextFloor.Value;
                RemoveStop(CurrentFloor);
            }
        }
    }

    private void RemoveStop(sbyte floorNumber)
    {
        DescendingStops.Remove(floorNumber);
        AscendingStops.Remove(floorNumber);
    }

    public CarFloorDistance GetDistanceFrom(int floorNumber)
    {
        int stopsTil = 0;
        int distanceFrom = Math.Abs(floorNumber - CurrentFloor);

        if (State == CarState.Ascending)
        {
            // NextFloor has to have a value if 
            // Car is ascending
            foreach (var floor in AscendingStops)
            {
                if (floor < floorNumber)
                {
                    stopsTil++;
                    distanceFrom = Math.Abs(floor - floorNumber);
                }
                else
                {
                    break;
                }
            }
        }

        return new(stopsTil, distanceFrom);
    }

    #endregion
}