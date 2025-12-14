using ElevatorApi.Api.Config;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace ElevatorApi.Api.Models;

public sealed class Car : IEquatable<Car>
{

    internal Car(byte id, sbyte initialFloor)
    {
        Id = id;
        CurrentFloor = initialFloor;
        Stops = new List<sbyte>();
    }
    
    public byte Id { get;  }
    public IReadOnlyCollection<sbyte> Stops { get; }
    public sbyte CurrentFloor { get; private set; }
    public sbyte? NextFloor{ get; private set; }
    
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

    public void MoveToNextFloor()
    {
        throw new NotImplementedException();
    }
    
    // cars need to be instantiated at floor 1 by default
    /* call car, car is on floor already
     * cars need to be instantiated with first floor (or lobby -- add lobby floor # to appsettings)
     * next floor = 1st floor
     * last floor = null
     * client sets car destination
     * 
     */
}