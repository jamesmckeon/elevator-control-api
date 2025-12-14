using ElevatorApi.Api.Dal;

namespace ElevatorApi.Api.Services;

public class CarService : ICarService
{
    private ICarRepository CarRepository { get; }

    public CarService(ICarRepository carRepository)
    {
        CarRepository = carRepository ??
                        throw new ArgumentNullException(nameof(carRepository));
    }

    public Car? GetById(byte id)
    {
        return CarRepository.GetById(id);
    }
}