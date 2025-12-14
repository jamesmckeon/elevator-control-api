using System.Collections.Concurrent;
using ElevatorApi.Api;
using ElevatorApi.Api.Config;
using ElevatorApi.Api.Models;
using ElevatorApi.Api.Services;
using Microsoft.Extensions.Options;

namespace ElevatorApi.Api.Dal;

public class CarRepository : ICarRepository
{
    private ConcurrentDictionary<byte, Car> Cars { get; }
    private IOptions<ElevatorSettings> SettingsOptions { get; }

    public CarRepository(IOptions<ElevatorSettings> settingsOptions)
    {
        ArgumentNullException.ThrowIfNull(settingsOptions);

        SettingsOptions = settingsOptions;
        Cars = new();
    }

    private void Init()
    {
        if (Cars.IsEmpty)
        {
            foreach (var id in Enumerable.Range(1, SettingsOptions.Value.CarCount)
                         .Select(i => (byte)i))
            {
                Cars.TryAdd(id, new Car() { Id = id });
            }
        }
    }

    public IReadOnlyList<Car> GetAll()
    {
        Init();

        return Cars.Values.ToList();
    }

    public Car? GetById(byte id)
    {
        Init();

        return Cars.GetValueOrDefault(id);
    }
}