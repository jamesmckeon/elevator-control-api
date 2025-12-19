namespace ElevatorApi.Api.Exceptions;

public class FloorNotFoundException : Exception
{
    public FloorNotFoundException(string paramName, sbyte minFloor, sbyte maxFloor) :
        base($"{paramName} must be between {minFloor} and {maxFloor}")
    {
    }

    public FloorNotFoundException(string message) : base(message)
    {
    }

    public FloorNotFoundException()
    {
    }

    public FloorNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}