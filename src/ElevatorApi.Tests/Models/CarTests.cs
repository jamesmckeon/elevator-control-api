namespace ElevatorApi.Tests.Models;

[Category("Unit")]
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
    public void AddStop_NoStops_ReturnsExpectedState()
    {
        var car = new Car(1, 0, 0, 10);

        Assert.That(car.NextFloor, Is.Null);
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
}