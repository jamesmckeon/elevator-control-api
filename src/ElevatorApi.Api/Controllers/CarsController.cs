using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ElevatorApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarsController:ControllerBase
{
    private ILogger<CarsController> Logger { get; }
    private ICarService CarService { get; }

    public CarsController(ILogger<CarsController> logger, ICarService carService)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        CarService = carService ?? throw new ArgumentNullException(nameof(carService));
    }

    [HttpGet("{carId}")]
    public IActionResult Index(byte carId)
    {
        
        var car = CarService.GetById(carId);

        if (car == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(car);
        }

    }
}