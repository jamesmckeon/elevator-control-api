namespace ElevatorApi.Tests.Services;

public class CarServiceTests
{
    private Mock<ICarRepository> Repository { get; set; }
    private CarService Sut { get; set; }

    [SetUp]
    public void Setup()
    {
        Repository = new();
        Sut = new(Repository.Object);
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
        var car = new Car()
        {
            Id = 1
        };

        Repository.Setup(s => s.GetById(car.Id))
            .Returns(car);

        Assert.That(Sut.GetById(car.Id), Is.EqualTo(car));
    }

    #endregion
}