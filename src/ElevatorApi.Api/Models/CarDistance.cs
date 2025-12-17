namespace ElevatorApi.Api.Models;

/// <summary>
/// Represents the distance from a car to a floor, considering both the number of stops and physical distance.
/// </summary>
/// <param name="StopsTil">The number of stops the car must make before
/// reaching the target floor (not including the floor itself).</param>
/// <param name="DistanceFrom">The physical distance (in floors) between the car and the target floor.</param>
public record CarDistance(
    int StopsTil,
    int DistanceFrom)
{
}