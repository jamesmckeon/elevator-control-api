namespace ElevatorApi.Api.Config;

/// <summary>
/// specifies the total count of floors and cars
/// </summary>
public sealed class ElevatorSettings
{
    public byte CarCount { get; set; }
    public sbyte MinFloor { get; set; }
    public sbyte MaxFloor { get; set; }
    
}