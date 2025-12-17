namespace ElevatorApi.Api.Config;

/// <summary>
/// specifies the total count of floors and cars
/// </summary>
public class ElevatorSettings
{
    /// <summary>
    /// the count of cars in the building
    /// </summary>
    public byte CarCount { get; set; }
    /// <summary>
    /// the lowest floor in the building
    /// </summary>
    public sbyte MinFloor { get; set; }
    /// <summary>
    /// the top floor of the building
    /// </summary>
    public sbyte MaxFloor { get; set; }
    /// <summary>
    /// the number of the lobby floor; must be between min and max floor
    /// </summary>
    public sbyte LobbyFloor { get; set; }
    
}