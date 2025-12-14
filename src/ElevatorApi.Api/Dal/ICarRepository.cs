using ElevatorApi.Api.Models;
using ElevatorApi.Api.Models;

namespace ElevatorApi.Api.Dal;

public interface ICarRepository
{
    IReadOnlyList<Car> GetAll();
    Car? GetById(byte id);
}