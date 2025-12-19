using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using ElevatorApi.Api.Exceptions;
using NUnit.Framework.Internal;

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

        Assert.Throws<FloorNotFoundException>(() =>
            car.AddStop(-1));
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
    public void Constructor_ValidInput_SetsDefaults()
    {
        var car = new Car(1, -1, 0, 10);

        Assert.Multiple(() =>
        {
            Assert.That(car.Id, Is.EqualTo(1));
            Assert.That(car.CurrentFloor, Is.EqualTo(-1));
            Assert.That(car.NextFloor, Is.Null);
            Assert.That(car.Stops, Is.Empty);
            Assert.That(car.State, Is.EqualTo(CarState.Idle));
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

    #region GetDistanceFrom

    [TestCase((sbyte)-1)]
    [TestCase((sbyte)1)]
    [TestCase((sbyte)0)]
    public void GetDistanceFrom_Idle_ReturnsExpected(sbyte floorNumber)
    {
        var car = new Car(1, 0, -10, 10);

        var actual = car.GetDistanceFrom(floorNumber);
        Assert.Multiple(() =>
        {
            Assert.That(actual.StopsTil, Is.EqualTo(0));
            Assert.That(actual.DistanceFrom, Is.EqualTo(Math.Abs(floorNumber)));
        });
    }

    [TestCase(0, -1)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    [TestCase(-1, 0)]
    [TestCase(0, 3)]
    [TestCase(0, -3)]
    public void GetDistanceFrom_NoStops_ReturnsExpected(sbyte currentFloor, sbyte targetFloor)
    {
        var car = new Car(1, currentFloor, -10, 10);
        var actual = car.GetDistanceFrom(targetFloor);

        Assert.Multiple(() =>
        {
            Assert.That(actual.StopsTil, Is.EqualTo(0));
            Assert.That(actual.DistanceFrom,
                Is.EqualTo(Math.Abs(targetFloor - currentFloor)));
        });
    }

    [TestCase(1, new[] { 1 }, 0, 1)]
    [TestCase(2, new[] { 1 }, 1, 1)]
    [TestCase(3, new[] { 1 }, 1, 2)]
    [TestCase(5, new[] { 1, 2 }, 2, 3)]
    [TestCase(5, new[] { 1, 2, 3 }, 3, 2)]
    [TestCase(5, new[] { 1, 2, 4 }, 3, 1)]
    [TestCase(5, new[] { 1, 2, 3, 4 }, 4, 1)]
    [TestCase(5, new[] { 1, 2, 3, 6 }, 3, 2)]
    [TestCase(7, new[] { 8 }, 0, 7)]
    [TestCase(-1, new[] { 1 }, 1, 2)]
    [TestCase(-2, new[] { 1 }, 1, 3)]
    [TestCase(-3, new[] { 1 }, 1, 4)]
    [TestCase(-5, new[] { 1, 2 }, 2, 7)]
    [TestCase(-5, new[] { 1, 2, -4 }, 3, 1)]
    [TestCase(-5, new[] { 1, 2, -6 }, 2, 7)]
    [TestCase(0, new[] { 5, -2 }, 0, 0)]
    public void GetDistanceFrom_HasStops_ReturnsExpected(
        int targetFloor,
        int[] stops,
        int expectedStops,
        int expectedDistance)
    {
        ArgumentNullException.ThrowIfNull(stops);

        var car = new Car(1, 0, -10, 10);

        foreach (var stop in stops)
        {
            car.AddStop((sbyte)stop);
        }

        var actual = car.GetDistanceFrom((sbyte)targetFloor);

        Assert.Multiple(() =>
        {
            Assert.That(actual.StopsTil, Is.EqualTo(expectedStops));
            Assert.That(actual.DistanceFrom, Is.EqualTo(expectedDistance));
        });
    }

    [TestCase(-10, 5)] // At MinFloor, target above
    [TestCase(10, -5)] // At MaxFloor, target below
    public void GetDistanceFrom_AtBoundaryFloor_ReturnsExpected(sbyte currentFloor, sbyte targetFloor)
    {
        var car = new Car(1, currentFloor, -10, 10);

        var actual = car.GetDistanceFrom(targetFloor);

        Assert.Multiple(() =>
        {
            Assert.That(actual.StopsTil, Is.EqualTo(0));
            Assert.That(actual.DistanceFrom, Is.EqualTo(Math.Abs(targetFloor - currentFloor)));
        });
    }

    [TestCase(5, new[] { 2, 1 }, 2, 1)]
    [TestCase(5, new[] { -1, -2 }, 0, 5)]
    [TestCase(5, new[] { 1, -1, 2 }, 2, 1)] // Stops in both directions
    public void GetDistanceFrom_TargetFloorZero_ReturnsExpected(
        sbyte startFloor, int[] stops, int expectedStops, int expectedDistance)
    {
        ArgumentNullException.ThrowIfNull(stops);

        var car = new Car(1, startFloor, -10, 10);

        foreach (var stop in stops)
            car.AddStop((sbyte)stop);

        var actual = car.GetDistanceFrom(0);

        Assert.Multiple(() =>
        {
            Assert.That(actual.StopsTil, Is.EqualTo(expectedStops));
            Assert.That(actual.DistanceFrom, Is.EqualTo(expectedDistance));
        });
    }

    #endregion

    private static Car TestCar() => new(1, 0, -2, 10);
}