using System.Data;
using System.Runtime;
using ElevatorApi.Api;
using ElevatorApi.Api.Config;
using Microsoft.Extensions.Options;

namespace ElevatorApi.Tests.Dal;

[Category("Unit")]
public class CarRepositoryTests
{
    private Mock<IOptions<ElevatorSettings>> SettingsOptions { get; set; }
    private CarRepository Sut { get; set; }

    [SetUp]
    public void SetUp()
    {
        SettingsOptions = new();
        Sut = new CarRepository(SettingsOptions.Object);
    }

    #region GetAll

    [TestCase(1)]
    [TestCase(3)]
    public void GetAll_VariousCarCounts_ReturnsExpected(byte carCount)
    {
        
        SettingsOptions.Setup(x => x.Value)
            .Returns(new ElevatorSettings()
            {
                CarCount = carCount
            });
        
        var expected = Enumerable.Range(1, carCount)
            .Select(id => new Car()
            {
                Id = (byte)id,
            }).ToList().AsReadOnly();

        var actual = Sut.GetAll();

        Assert.That(actual, Is.EquivalentTo(expected));
    }

    #endregion

    #region GetById

    [Test]
    public void GetById_CarExists_ReturnsCar()
    {
        SettingsOptions.Setup(x => x.Value)
            .Returns(new ElevatorSettings()
            {
                CarCount = 3
            });
        
        var expected = new Car()
        {
            Id = 2
        };

        var actual = Sut.GetById(expected.Id);

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void GetById_NonExistentCar_ReturnsNull()
    {
        SettingsOptions.Setup(x => x.Value)
            .Returns(new ElevatorSettings()
            {
                CarCount = 1
            });
        
        Assert.That(Sut.GetById(2), Is.Null);
    }

    #endregion
}