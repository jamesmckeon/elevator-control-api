using ElevatorApi.Api.Config;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ElevatorApi.Api.Exceptions;

namespace ElevatorApi.Api.Models;

[DebuggerDisplay("Id = {Id}; Current={CurrentFloor}; Next={NextFloor}")]
public sealed class Car : IEquatable<Car>
{
    private readonly Lock _carLock = new();

    #region Properties

    private (sbyte MinFloor, sbyte MaxFloor) FloorRange { get; }
    public byte Id { get; }

    /// <summary>
    /// The stops/floor number currently assigned to the car
    /// </summary>
    public IReadOnlyCollection<sbyte> Stops => GetStops();

    /// <summary>
    /// The floor # that the car is currently on
    /// </summary>
    public sbyte CurrentFloor { get; private set; }

    /// <summary>
    /// The floor # the car will advance to next, if present
    /// </summary>
    public sbyte? NextFloor => Stops.Count > 0 ? Stops.First() : null;

    private bool? Ascending { get; set; }
    private SortedSet<sbyte> AscendingStops { get; }
    private SortedSet<sbyte> DescendingStops { get; }

    /// <summary>
    /// The current car's current state (<seealso cref="CarState"/>
    /// </summary>
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

    /// <summary>
    /// Adds a stop to the car
    /// </summary>
    /// <param name="floorNumber">the floor # to add to the car's list of stops</param>
    /// <exception cref="FloorNotFoundException">Thrown
    /// when <param name="floorNumber"/> is invalid</exception>
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
            throw new FloorNotFoundException(nameof(floorNumber),
                FloorRange.MinFloor, FloorRange.MaxFloor);
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

    /// <summary>
    /// Moves the car to its next stop
    /// </summary>
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

    /// <summary>
    /// Determines the car's distance from and stops until <param name="floorNumber"/>
    /// and returns a CarFloorDistance instance
    /// </summary>
    /// <param name="floorNumber">the floor # to measure the can's proximity against</param>
    public CarDistance GetDistanceFrom(sbyte floorNumber)
    {
        var lastFloor = LastFloorTil(floorNumber);
        var distance = Math.Abs(floorNumber - (lastFloor ?? CurrentFloor));
        var stops = StopsTil(floorNumber).Count;

        return new(stops, distance);
    }

    /// <summary>
    /// Returns the closest floor in a car's list of stops to <param name="floorNumber"></param>
    /// </summary>
    /// <param name="floorNumber">the floorNumber to evaluate</param>
    /// <returns>the number of the car's closest stop or null if car has no stops</returns>
    sbyte? LastFloorTil(sbyte floorNumber)
    {
        lock (_carLock)
        {
            if (State == CarState.Idle || CurrentFloor == floorNumber)
                return null;

            bool between(sbyte first, sbyte second) =>
                floorNumber >= Math.Min(first, second) &&
                floorNumber <= Math.Max(first, second);

            var withCurrent = new sbyte[] { CurrentFloor }
                .Concat(Stops)
                .ToArray();

            for (var i = 0; i < withCurrent.Length - 1; i++)
            {
                if (between(withCurrent[i], withCurrent[i + 1]))
                {
                    return withCurrent[i];
                }
            }

            return withCurrent.Last();
        }
    }

    /// <summary>
    /// Returns the stops a car has to traverse before reaching <param name="floorNumber"/>
    /// </summary>
    /// <param name="floorNumber">The floor # to evaluate</param>
    /// <returns>The list of pending car stops prior to <param name="floorNumber"/>
    /// or an empty list if car has no stops</returns>
    List<sbyte> StopsTil(sbyte floorNumber)
    {
        lock (_carLock)
        {
            if (State == CarState.Idle || CurrentFloor == floorNumber)
                return [];

            bool between(sbyte first, sbyte second) =>
                floorNumber >= Math.Min(first, second) &&
                floorNumber <= Math.Max(first, second);

            var withCurrent = new sbyte[] { CurrentFloor }
                .Concat(Stops)
                .ToArray();

            var stops = new List<sbyte>();

            for (var i = 0; i < withCurrent.Length - 1; i++)
            {
                if (between(withCurrent[i], withCurrent[i + 1]))
                {
                    break;
                }

                stops.Add(withCurrent[i]);
            }

            return stops;
        }
    }

    #endregion
}