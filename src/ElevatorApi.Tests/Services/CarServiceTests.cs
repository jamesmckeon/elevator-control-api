using System.Collections.ObjectModel;
using ElevatorApi.Api.Config;
using ElevatorApi.Api.Exceptions;
using Microsoft.Extensions.Options;

namespace ElevatorApi.Tests.Services;

[Category("Unit")]
public class CarServiceTests
{
    private Mock<ICarRepository> Repository { get; set; }
    private Mock<IOptions<ElevatorSettings>> Settings { get; set; }
    private CarService Sut { get; set; }

    [SetUp]
    public void Setup()
    {
        Repository = new();

        Settings = new();
        Settings.Setup(s => s.Value).Returns(new ElevatorSettings()
        {
            MinFloor = -2,
            MaxFloor = 10,
            LobbyFloor = 0,
            CarCount = 3
        });

        Sut = new(Repository.Object, Settings.Object);
    }

    #region GetById

    [Test]
    public void GetById_CarNotFound_ReturnsNull()
    {
        Repository.Setup(s => s.GetById(1)).Returns(null as Car);
        Assert.That(Sut.GetById(1), Is.Null);
    }

    [Test]
    public void GetById_CarFound_ReturnsCar()
    {
        var car = new Car(1, 0, 0, 1);

        Repository.Setup(s => s.GetById(car.Id))
            .Returns(car);

        Assert.That(Sut.GetById(car.Id), Is.EqualTo(car));
    }

    #endregion

    #region AddStop

    [Test]
    public void AddStop_CarNotFound_ReturnsNull()
    {
        byte carId = 1;
        Repository.Setup(s => s.GetById(carId))
            .Returns(null as Car);

        Assert.Throws<CarNotFoundException>(() =>
            Sut.AddStop(carId, 1));
    }

    [Test]
    public void AddStop_CarFound_AddsStop()
    {
        var car = new Car(1, 0, -1, 10);
        Repository.Setup(s => s.GetById(car.Id))
            .Returns(car);

        var actual = Sut.AddStop(car.Id, 2);
        Assert.That(actual.Stops, Does.Contain(2));
    }

    #endregion

    #region MoveCar

    [Test]
    public void MoveCar_CarNotFound_ReturnsNull()
    {
        byte carId = 1;
        Repository.Setup(s => s.GetById(carId))
            .Returns(null as Car);

        Assert.Throws<CarNotFoundException>(() =>
            Sut.MoveCar(carId));
    }

    [Test]
    public void MoveCar_CarFound_MovesCar()
    {
        var car = new Car(1, 0, -1, 10);
        Repository.Setup(s => s.GetById(car.Id))
            .Returns(car);

        car.AddStop(2);

        var actual = Sut.MoveCar(car.Id);
        Assert.That(actual.CurrentFloor, Is.EqualTo(2));
    }

    #endregion

    #region CallCar

    [Test]
    public void CallCar_AllIdle_AssignsCar()
    {
        var cars = SetupAllCars();
        var actual = Sut.CallCar(1);

        Assert.Multiple(() =>
        {
            Assert.That(actual.NextFloor, Is.EqualTo(1));
            Assert.That(actual.Stops, Is.EqualTo(new sbyte[] { 1 }));
            Assert.That(cars.Where(c => c.Id != actual.Id),
                Has.All.Property(nameof(Car.NextFloor)).Null);
            Assert.That(cars.Where(c => c.Id != actual.Id),
                Has.All.Property(nameof(Car.Stops)).Empty);
        });
    }

    [Test]
    public void CallCar_CarAtFloor_AssignsExpected()
    {
        var (first, second, third) = SetupCars();

        first.AddStop(1);
        first.MoveNext();

        var actual = Sut.CallCar(1);
        Assert.That(actual, Is.EqualTo(first));
    }

    [TestCase(-3)]
    [TestCase(11)]
    public void CallCar_InvalidFloorNumber_ThrowsExpected(sbyte floorNumber)
    {
        Assert.Throws<FloorNotFoundException>(() =>
            Sut.CallCar(floorNumber));
    }

    [Test]
    public void CallCar_BeyondAllAscending_AssignsNearest()
    {
        // "beyond" in the sense that the called floor is above
        // the highest stop for all cars
        var (first, second, third) = SetupCars();

        MoveTo(first, 1);
        MoveTo(second, 2);
        MoveTo(third, 3);

        first.AddStop(3);
        second.AddStop(5);
        third.AddStop(6);

        var actual = Sut.CallCar(8);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(third));
            Assert.That(actual.Stops, Is.EqualTo(new sbyte[] { 6, 8 }));
        });
    }

    [Test]
    public void CallCar_WithinAllAscending_AssignsNearest()
    {
        var (first, second, third) = SetupCars();

        MoveTo(first, 1);
        MoveTo(second, 2);
        MoveTo(third, 3);

        first.AddStop(7);
        second.AddStop(5);
        third.AddStop(6);

        var actual = Sut.CallCar(4);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(third));
            Assert.That(actual.Stops, Is.EqualTo(new sbyte[] { 4, 6 }));
        });
    }

    [Test]
    public void CallCar_BelowAllAscending_AssignsNearest()
    {
        // "below" in the sense that the called floor is below
        // the last stop for all cars; SUT should assign car with the
        // nearest last stop to the called floor
        var (first, second, third) = SetupCars();

        MoveTo(first, 1);
        MoveTo(second, 2);
        MoveTo(third, 3);

        first.AddStop(8);
        second.AddStop(7);
        third.AddStop(6);

        var actual = Sut.CallCar(-1);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(third));
            Assert.That(actual.Stops, Is.EqualTo(new sbyte[] { 6, -1 }));
        });
    }

    [Test]
    public void CallCar_AllDescendingToSameFloor_AssignsExpected()
    {
        var (first, second, third) = SetupCars();

        // advance cars to an upper floor
        // so they can descend
        MoveTo(first, 6);
        MoveTo(second, 7);
        MoveTo(third, 6);

        // add descending stops
        AddStops(first, 3, 2, 1);
        AddStops(second, 5, 1);
        AddStops(third, 5, 4, 2, 1);

        var actual = Sut.CallCar(0);

        Assert.Multiple(() =>
        {
            // SUT should assign car with fewest stops
            Assert.That(actual, Is.EqualTo(second));
            Assert.That(actual.Stops, Is.EqualTo(new sbyte[] { 5, 1, 0 }));
        });
    }

    [Test]
    public void CallCar_BeyondAllDescending_AssignsNearest()
    {
        var (first, second, third) = SetupCars();

        // move cars to an upper floor to establish
        // descending direction
        MoveTo(first, 8);
        MoveTo(second, 7);
        MoveTo(third, 10);

        AddStops(first, 6, 1);
        AddStops(second, 4, 0);
        AddStops(third, 8, 4, 0);

        var actual = Sut.CallCar(-2);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(second));
            Assert.That(actual.Stops,
                Is.EqualTo(new sbyte[] { 4, 0, -2 }));
        });
    }

    [Test]
    public void CallCar_WithinAllDescending_AssignsFewestStops()
    {
        var (first, second, third) = SetupCars();

        MoveTo(first, 8);
        MoveTo(second, 7);
        MoveTo(third, 10);

        AddStops(first, 6, 1);
        AddStops(second, 4);
        AddStops(third, 8, 4, 0);

        var actual = Sut.CallCar(5);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(second));
            Assert.That(actual.Stops,
                Is.EqualTo(new sbyte[] { 5, 4 }));
        });
    }

    [Test]
    public void CallCar_WithinAllDescending_AssignsClosestCar()
    {
        var (first, second, third) = SetupCars();

        MoveTo(first, 8);
        MoveTo(second, 7);
        MoveTo(third, 10);

        AddStops(first, 4);
        AddStops(second, 3);
        AddStops(third, 8, 4);

        var actual = Sut.CallCar(5);

        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(second));
            Assert.That(actual.Stops,
                Is.EqualTo(new sbyte[] { 5, 3 }));
        });
    }

    #endregion

    #region Helpers

    /// <summary>
    /// sets up repository to return a list of cars and
    /// returns each car in the list
    /// </summary>
    private (Car first, Car second, Car third) SetupCars()
    {
        var cars = SetupAllCars();
        return (cars.First(), cars.Skip(1).First(), cars.Last());
    }

    /// <summary>
    /// sets up repository to return a list of cars and returns the list
    /// </summary>
    private ReadOnlyCollection<Car> SetupAllCars()
    {
        var cars = Enumerable.Range(1, 3).Select(i =>
                new Car(
                    (byte)i,
                    Settings.Object))
            .ToList();

        Repository.Setup(s => s.GetAll())
            .Returns(cars);

        return new(cars);
    }

    /// <summary>
    /// Assigns stops to a car in the order that they appear in <paramref name="floors"/>
    /// </summary>
    /// <param name="car">The car to assign stops to</param>
    /// <param name="floors">The stops to assign</param>
    private static void AddStops(Car car, params IEnumerable<sbyte> floors)
    {
        foreach (var floor in floors)
        {
            car.AddStop(floor);
        }
    }

    /// <summary>
    /// Assigns <paramref name="floorNumber"/> to <paramref name="car"/>'s stops
    /// and calls car.MoveNext()
    /// </summary>
    private static void MoveTo(Car car, sbyte floorNumber)
    {
        if (car.Stops.Count > 0)
        {
            throw new ArgumentException("car cannot have any stops", nameof(car));
        }

        car.AddStop(floorNumber);
        car.MoveNext();
    }

    #endregion
}