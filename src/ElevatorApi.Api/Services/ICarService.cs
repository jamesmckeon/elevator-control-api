using ElevatorApi.Api.Models;
using ElevatorApi.Api.Exceptions;

namespace ElevatorApi.Api.Services;

public interface ICarService
{
    Car? GetById(byte id);

    /// <summary>
    /// Adds a floor to a car's stops
    /// </summary>
    /// <param name="carId">the id of a car</param>
    /// <param name="floorNumber">the floor to add to the car's stops</param>
    /// <returns>the affected car</returns>
    /// <exception cref="CarNotFoundException"/>
    /// <exception cref="FloorNotFoundException"/>
    Car AddStop(byte carId, sbyte floorNumber);

    /// <summary>
    /// Advances a car to its next stop
    /// </summary>
    /// <param name="carId"></param>
    /// <returns></returns>
    /// <exception cref="CarNotFoundException"/>
    Car MoveCar(byte carId);

    /// <summary>
    /// Finds the nearest car and adds a floor to its
    /// list of stops
    /// </summary>
    /// <param name="floorNumber">The floor the car should stop at</param>
    /// <exception cref="FloorNotFoundException"/>
    Car CallCar(sbyte floorNumber);
}