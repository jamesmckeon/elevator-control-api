using System.Collections.ObjectModel;
using ElevatorApi.Api.Config;
using ElevatorApi.Api.Dal;
using ElevatorApi.Api.Exceptions;
using Microsoft.Extensions.Options;

namespace ElevatorApi.Api.Services;

public class CarService : ICarService
{
    private ICarRepository CarRepository { get; }
    private IOptions<ElevatorSettings> Settings { get; }

    public CarService(ICarRepository carRepository, IOptions<ElevatorSettings> settings)
    {
        CarRepository = carRepository ??
                        throw new ArgumentNullException(nameof(carRepository));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public Car? GetById(byte id)
    {
        return CarRepository.GetById(id);
    }

    public Car AddStop(byte carId, sbyte floorNumber)
    {
        var car = CarRepository.GetById(carId) ??
                  throw new CarNotFoundException(carId);
        car.AddStop(floorNumber);
        return car;
    }

    public Car MoveCar(byte carId)
    {
        var car = CarRepository.GetById(carId) ??
                  throw new CarNotFoundException(carId);
        car.MoveNext();
        return car;
    }

    public Car CallCar(sbyte floorNumber)
    {
        ValidateFloor(floorNumber);

        Car? car = null;
        var cars = CarRepository.GetAll().ToList();

        var hasStop = cars.SingleOrDefault(c => c.Stops.Contains(floorNumber));

        if (hasStop != null)
        {
            return hasStop;
        }

        var carDistances = CarRepository.GetAll().Select(c =>
            new
            {
                Car = c,
                Distance = c.GetDistanceFrom(floorNumber)
            }).ToList();

        var fewestStops = carDistances.Where(s =>
                s.Distance.StopsTil == carDistances.Min(m => m.Distance.StopsTil))
            .ToList();

        car = fewestStops.Count == 1
            ? fewestStops.Single().Car
            : fewestStops.OrderBy(fs => fs.Distance.DistanceFrom).First().Car;

        car.AddStop(floorNumber);
        return car;
    }

    private void ValidateFloor(sbyte floorNumber)
    {
        if (floorNumber < Settings.Value.MinFloor || floorNumber > Settings.Value.MaxFloor)
        {
            throw new FloorNotFoundException(nameof(floorNumber),
                Settings.Value.MinFloor, Settings.Value.MaxFloor);
        }
    }
}