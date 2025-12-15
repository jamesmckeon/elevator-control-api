using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace ElevatorApi.Tests.Models;

[Category("Unit")]
// arrays aren't reused between methods
[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
public class CarTests
{
    #region Equals

    [Test]
    public void Equals_SameCarNumber_ReturnsTrue()
    {
        var firstCar = new Car(1, 0, 0, 1);
        var secondCar = new Car(1, 1, 0, 1);
        Assert.That(firstCar.Equals(secondCar), Is.True);
    }

    [Test]
    public void Equals_DifferentCarNumber_ReturnsFalse()
    {
        var firstCar = new Car(1, 0, 0, 1);
        var secondCar = new Car(2, 0, 0, 1);
        Assert.That(firstCar.Equals(secondCar), Is.False);
    }

    #endregion

    #region Equals GetHashCode

    [TestCase(1)]
    [TestCase(0)]
    public void GetHashcode_ReturnsExpected(byte id)
    {
        var car = new Car(id, 0, 0, 1);
        Assert.That(car.GetHashCode(), Is.EqualTo(id.GetHashCode()));
    }

    #endregion

    #region Equals (Object Overload)

    [Test]
    public void EqualsOverload_DifferentType_ReturnsFalse()
    {
        var tesObj = new
        {
            Id = (byte)1
        };

        var sut = new Car(1, 0, 0, 1);

        Assert.That(sut.Equals(tesObj), Is.False);
    }

    [Test]
    public void EqualsOverload_SameType_ReturnsTrue()
    {
        var firstCar = new Car(1, 0, 0, 1);
        var secondCar = new Car(1, 0, 0, 1);
        Assert.That(firstCar.Equals(secondCar as Object), Is.True);
    }

    [Test]
    public void EqualsOverload_SameType_ReturnsFalse()
    {
        var firstCar = new Car(1, 0, 0, 1);
        var secondCar = new Car(2, 0, 0, 1);
        Assert.That(firstCar.Equals(secondCar as Object), Is.False);
    }

    #endregion

    #region AddStop

    [Test]
    public void AddStop_InvalidFloorNumber_ThrowsExpected()
    {
        var car = new Car(1, 0, 0, 10);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            car.AddStop(-1));

        Assert.Multiple(() =>
        {
            Assert.That(ex.ParamName, Is.EqualTo("floorNumber"));
            Assert.That(ex.Message, Does.StartWith("floorNumber must be between 0 and 10."));
        });
    }

    [Test]
    public void AddStop_ValidFloorNumber_ReturnsExpectedCurrentFloor()
    {
        var car = new Car(1, 0, 0, 10);

        car.AddStop(1);
        Assert.That(car.CurrentFloor, Is.EqualTo(0));
    }

    [TestCase(new sbyte[] { 1, 2 }, new sbyte[] { 1, 2 })]
    [TestCase(new sbyte[] { 2, 1 }, new sbyte[] { 1, 2 })]
    [TestCase(new sbyte[] { 2 }, new sbyte[] { 2 })]
    [TestCase(new sbyte[] { 2, 2 }, new sbyte[] { 2 })]
    [TestCase(new sbyte[] { 2, -2, 2 }, new sbyte[] { 2, -2 })]
    [TestCase(new sbyte[] { 1, 2, -2 }, new sbyte[] { 1, 2, -2 })]
    [TestCase(new sbyte[] { 1, -2, 2 }, new sbyte[] { 1, 2, -2 })]
    [TestCase(new sbyte[] { -1, -2 }, new sbyte[] { -1, -2 })]
    [TestCase(new sbyte[] { -2, -1 }, new sbyte[] { -1, -2 })]
    [TestCase(new sbyte[] { -2, -1, 1 }, new sbyte[] { -1, -2, 1 })]
    [TestCase(new sbyte[] { -2, -1, 4, 10, 8, 0, 3, -2, 4 }, new sbyte[] { -1, -2, 3, 4, 8, 10 })]
    public void AddStop_ValidFloors_ReturnsExpectedStops(sbyte[] input, sbyte[] expected)
    {
        ArgumentNullException.ThrowIfNull(input);

        var car = new Car(1, 0, -2, 10);

        foreach (var stop in input)
        {
            car.AddStop(stop);
        }

        Assert.That(car.Stops, Is.EqualTo(expected));
    }

    #endregion

    #region Constructor

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    public void Constructor_InvalidFloorRange_ThrowsExpected(sbyte min, sbyte max)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = new Car(1, 0, min, max));

        Assert.Multiple(() =>
        {
            Assert.That(ex.ParamName, Is.EqualTo("minFloor"));
            Assert.That(ex.Message,
                Does.StartWith("minFloor must be less than maxFloor."));
        });
    }

    [Test]
    public void Constructor_ValidInput_ReturnsExpectedState()
    {
        var car = new Car(1, -1, 0, 10);

        Assert.Multiple(() =>
        {
            Assert.That(car.Id, Is.EqualTo(1));
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    #endregion

    #region MoveNext

    [Test]
    public void MoveNext_NoStops_DoesntMove()
    {
        var car = TestCar();
        var startFloor = car.CurrentFloor;

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(startFloor));
            Assert.That(car.NextFloor, Is.Null);
        });
    }

    [Test]
    public void MoveNext_OneStop_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(1);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_StopAddedInSameDirection_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(1);
        car.AddStop(5);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(1));
            Assert.That(car.NextFloor, Is.EqualTo(5));
            Assert.That(car.Stops, Is.EqualTo(new[] { 5 }));
        });

        car.AddStop(3);
        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(3));
            Assert.That(car.NextFloor, Is.EqualTo(5));
            Assert.That(car.Stops, Is.EqualTo(new[] { 5 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(5));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_StopAddedInOtherDirection_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(1);
        car.AddStop(5);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(1));
            Assert.That(car.NextFloor, Is.EqualTo(5));
            Assert.That(car.Stops, Is.EqualTo(new[] { 5 }));
        });

        car.AddStop(-1);
        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(5));
            Assert.That(car.NextFloor, Is.EqualTo(-1));
            Assert.That(car.Stops, Is.EqualTo(new[] { -1 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_StopAddedInOtherDirectionDescending_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(-1);
        car.AddStop(-2);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.EqualTo(-2));
            Assert.That(car.Stops, Is.EqualTo(new[] { -2 }));
        });

        car.AddStop(5);
        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-2));
            Assert.That(car.NextFloor, Is.EqualTo(5));
            Assert.That(car.Stops, Is.EqualTo(new[] { 5 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(5));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_MultipleStopsAddedInConflictingDirections_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(-1);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });

        car.AddStop(5);
        car.AddStop(-2);
        car.AddStop(0);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-2));
            Assert.That(car.NextFloor, Is.EqualTo(0));
            Assert.That(car.Stops, Is.EqualTo(new[] { 0, 5 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(0));
            Assert.That(car.NextFloor, Is.EqualTo(5));
            Assert.That(car.Stops, Is.EqualTo(new[] { 5 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(5));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_ThreeInitialStops_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(1);
        car.AddStop(-1);
        car.AddStop(2);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(1));
            Assert.That(car.NextFloor, Is.EqualTo(2));
            Assert.That(car.Stops, Is.EqualTo(new[] { 2, -1 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(2));
            Assert.That(car.NextFloor, Is.EqualTo(-1));
            Assert.That(car.Stops, Is.EqualTo(new[] { -1 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_ThreeInitialStopsDescending_SetsExpectedState()
    {
        var car = TestCar();

        car.AddStop(-2);
        car.AddStop(1);
        car.AddStop(-1);

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.EqualTo(-2));
            Assert.That(car.Stops, Is.EqualTo(new[] { -2, 1 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(-2));
            Assert.That(car.NextFloor, Is.EqualTo(1));
            Assert.That(car.Stops, Is.EqualTo(new[] { 1 }));
        });

        car.MoveNext();

        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    [Test]
    public void MoveNext_StopAddedAtCurrentFloor_IgnoresOrHandlesCorrectly()
    {
        var car = TestCar();
        car.AddStop(0);

        car.MoveNext();
        Assert.Multiple(() =>
        {
            Assert.That(car.CurrentFloor, Is.EqualTo(0));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
        });
    }

    #endregion


    private static Car TestCar() => new(1, 0, -2, 10);
}