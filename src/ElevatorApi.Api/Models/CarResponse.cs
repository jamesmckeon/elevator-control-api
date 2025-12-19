namespace ElevatorApi.Api.Models;

public record CarResponse(
    byte Id,
    sbyte? NextFloor,
    sbyte CurrentFloor,
    IReadOnlyCollection<sbyte> Stops)
{
};