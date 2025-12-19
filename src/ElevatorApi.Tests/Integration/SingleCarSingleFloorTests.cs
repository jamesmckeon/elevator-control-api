using System.Net;
using ElevatorApi.Api.Config;

namespace ElevatorApi.Tests.Integration;

[Category("Integration")]
internal sealed class SingleCarSingleFloorTests : TestsBase
{
    protected override ElevatorSettings ElevatorSettings => new()
    {
        CarCount = 1,
        MinFloor = 0,
        MaxFloor = 1,
        LobbyFloor = 0
    };

    #region Index

    [Test]
    public async Task Index_CarExists_ReturnsExpected()
    {
        byte carId = 1;

        var response = await GetCarResponse(carId);
        var car = await ParseCar(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(car.Id, Is.EqualTo(carId));
            // car should be at the lobby
            Assert.That(car.CurrentFloor, Is.EqualTo(0));
        });
    }

    [TestCase(0)]
    [TestCase(2)]
    public async Task Index_CarDoesntExist_ReturnsNotFound(byte carId)
    {
        var response = await GetCarResponse(carId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region AddStop

    [Test]
    public async Task AddStop_CarExists_ReturnsExpected()
    {
        byte carId = 1;
        sbyte floorNumber = 1;

        var response = await AddCarStopResponse(carId, floorNumber);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.OK));

        var car = await ParseCar(response);

        Assert.Multiple(() =>
        {
            Assert.That(car.Id, Is.EqualTo(carId));
            Assert.That(car.NextFloor, Is.EqualTo(floorNumber));
        });
    }

    [Test]
    public async Task AddStop_InvalidCarId_ReturnsNotFound()
    {
        byte carId = 2;
        sbyte floorNumber = -2;

        var response = await AddCarStopResponse(carId, floorNumber);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task AddStop_InvalidFloor_ReturnsBadRequest()
    {
        var response = await AddCarStopResponse(1, -1);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
    }

    #endregion

    #region MoveCar

    [Test]
    public async Task MoveCar_CarExists_ReturnsExpected()
    {
        byte carId = 1;
        sbyte floorNumber = 1;

        // request a car and then move it
        await AddCarStopResponse(carId, floorNumber);
        var moveResponse = await MoveCarResponse(carId);

        Assert.That(moveResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.OK));

        var movedCar = await ParseCar(moveResponse);

        Assert.Multiple(() =>
        {
            Assert.That(movedCar.Id, Is.EqualTo(carId));
            Assert.That(movedCar.CurrentFloor, Is.EqualTo(floorNumber));
            Assert.That(movedCar.Stops, Is.Empty);
            Assert.That(movedCar.NextFloor, Is.Null);
        });
    }

    [Test]
    public async Task MoveCar_InvalidCarId_ReturnsNotFound()
    {
        byte carId = 2;
        var response = await MoveCarResponse(carId);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region CallCar

    [Test]
    public async Task CallCar_ValidFloor_AssignsCar()
    {
        var response = await CallCarResponse(1);
        var car = await ParseCar(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(car.NextFloor, Is.EqualTo(1));
        });
    }
    
    [Test]
    public async Task CallCar_LobbyFloor_DoesntAddStop()
    {
        // car is parked at lobby by default
        var response = await CallCarResponse(0); // lobby floor
        var car = await ParseCar(response);

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(car.NextFloor, Is.Null);
        });
    }

    [Test]
    public async Task CallCar_InvalidFloorNumber_Returns400()
    {
        var response = await CallCarResponse(2);

        Assert.That(response.StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
    }

    #endregion
}