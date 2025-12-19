using System.Net;
using ElevatorApi.Api.Config;

namespace ElevatorApi.Tests.Integration;

[Category("Integration")]
internal sealed class MultipleCarsMultipleFloorsTests : TestsBase
{
    protected override ElevatorSettings ElevatorSettings => new()
    {
        CarCount = 3,
        MinFloor = -1,
        MaxFloor = 8,
        LobbyFloor = 1
    };

    #region NotFound

    [TestCase(4)]
    [TestCase(0)]
    public async Task MultipleEndpoints_CarDoesntExist_ReturnsNotFound(byte carId)
    {
        var methods = new Task<HttpResponseMessage>[]
        {
            GetCarResponse(carId),
            AddCarStopResponse(carId, -1),
            MoveCarResponse(carId)
        };

        foreach (var method in methods)
        {
            var response = await method;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }

    [TestCase(-2)]
    [TestCase(9)]
    public async Task MultipleEndpoints_FloorDoesntExist_ReturnsNotFound(sbyte floorNumber)
    {
        var methods = new[]
        {
            AddCarStopResponse(1, floorNumber),
            CallCarResponse(floorNumber)
        };

        foreach (var method in methods)
        {
            var response = await method;

            Assert.That(response.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }

    #endregion

    #region CallCar

    [Test]
    public async Task CallCar_AllIdle_AssignsIdleCar()
    {
        var carOne = await ParseCar(await CallCarResponse(2));
        var carTwo = await ParseCar(await CallCarResponse(3));
        var carThree = await ParseCar(await CallCarResponse(4));

        Assert.Multiple(() =>
        {
            Assert.That(carOne.NextFloor, Is.EqualTo(2));
            Assert.That(carTwo.NextFloor, Is.EqualTo(3));
            Assert.That(carThree.NextFloor, Is.EqualTo(4));
        });
    }

    [Test]
    public async Task CallCar_FloorAlreadyAssigned_DoesntReassign()
    {
        var carOne = await ParseCar(await CallCarResponse(2));
        var carTwo = await ParseCar(await CallCarResponse(2));

        Assert.Multiple(() =>
        {
            Assert.That(carOne.NextFloor, Is.EqualTo(2));
            Assert.That(carTwo.NextFloor, Is.EqualTo(2));
            Assert.That(carOne.Id, Is.EqualTo(carTwo.Id));
        });
    }

    [Test]
    public async Task CallCar_CarAtFloor_DoesntReassign()
    {
        // all cars start at floor 1 (in this class/scenario)
        // so SUT shouldn't assign floor 1 to any car initially
        var lobbyCar = await ParseCar(await CallCarResponse(1));
        Assert.That(lobbyCar.NextFloor, Is.Null);

        await AddCarStopResponse(lobbyCar.Id, 2);
        var movedCar = await ParseCar(await MoveCarResponse(lobbyCar.Id));

        Assert.That(movedCar.CurrentFloor, Is.EqualTo(2));

        // likewise car is already at floor 2, SUT shouldn't assign
        // this floor to any cars
        var floorTwoCar = await ParseCar(await CallCarResponse(2));

        Assert.Multiple(() =>
        {
            Assert.That(floorTwoCar.NextFloor, Is.Null);
            Assert.That(floorTwoCar.Id, Is.EqualTo(movedCar.Id));
        });
    }

    [Test]
    public async Task CallCar_NoCarsIdle_AssignsToFewestStops()
    {
        var carOne = await ParseCar(await CallCarResponse(1));
        await AddCarStopResponse(carOne.Id, 2);

        var carTwo = await ParseCar(await CallCarResponse(-1));
        await AddCarStopResponse(carTwo.Id, 0);

        var carThree = await ParseCar(await CallCarResponse(6));

        // carOne is closest, but it should get assigned to car 3
        var assignedCar = await ParseCar(await CallCarResponse(3));

        Assert.That(assignedCar.Id, Is.EqualTo(carThree.Id));
    }

    [Test]
    public async Task CallCar_NoCarsIdle_AssignsToClosestCar()
    {
        // assign all cars three floors apart
        // all are idle, should one Call() request should get assigned
        // to each car
        await ParseCar(await CallCarResponse(-1));
        await ParseCar(await CallCarResponse(2));
        var carThree = await ParseCar(await CallCarResponse(5));

        var assignedCar = await ParseCar(await CallCarResponse(4));

        Assert.That(assignedCar.Id, Is.EqualTo(carThree.Id));
    }

    #endregion
}