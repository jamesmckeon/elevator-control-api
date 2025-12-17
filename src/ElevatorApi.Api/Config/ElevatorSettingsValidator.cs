using Microsoft.Extensions.Options;

namespace ElevatorApi.Api.Config;

internal sealed class ElevatorSettingsValidator : IValidateOptions<ElevatorSettings>
{
    public ValidateOptionsResult Validate(string? name, ElevatorSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        if (options.CarCount < 1)
            return ValidateOptionsResult.Fail("CarCount must be greater than zero");

        if (options.MinFloor >= options.MaxFloor)
            return ValidateOptionsResult.Fail("MinFloor must be less than MaxFloor");
        
        if (options.LobbyFloor < options.MinFloor || options.LobbyFloor > options.MaxFloor)
            return ValidateOptionsResult.Fail("LobbyFloor must be between MinFloor and MaxFloor");

        return ValidateOptionsResult.Success;
    }
}