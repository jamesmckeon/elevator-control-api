using ElevatorApi.Api;
using ElevatorApi.Api.Config;

namespace ElevatorApi.Tests.Config;

[Category("Unit")]
public class ElevatorSettingsValidatorTests
{
    private ElevatorSettingsValidator Sut { get; set; }

    [SetUp]
    public void SetUp()
    {
        Sut = new();
    }

    [Test]
    public void Validate_ValidSettings_ReturnsNotFailed()
    {
        var settings = new ElevatorSettings()
        {
            CarCount = 1,
            MaxFloor = 1,
            MinFloor = 0,
            LobbyFloor = 0
        };
        
        var actual = Sut.Validate(null, settings);
        Assert.That(actual.Failed, Is.False);
    }

    [Test]
    public void Validate_InvalidCarCount_ReturnsFailed()
    {
        var settings = new ElevatorSettings()
        {
            CarCount = 0,
            MaxFloor = 1,
            MinFloor = 0
        };
        var actual = Sut.Validate(null, settings);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Failed, Is.True);
            Assert.That(actual.FailureMessage, Is.EqualTo("CarCount must be greater than zero"));
        });
    }

    [TestCase(1, 1)]
    [TestCase(2, 1)]
    public void Validate_InvalidMinMaxFloor_ReturnsFailed(sbyte minFloor, sbyte maxFloor)
    {
        var settings = new ElevatorSettings()
        {
            CarCount = 1,
            MaxFloor = maxFloor,
            MinFloor = minFloor
        };
        var actual = Sut.Validate(null, settings);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Failed, Is.True);
            Assert.That(actual.FailureMessage, Is.EqualTo("MinFloor must be less than MaxFloor"));
        });
    }
    
    [Test]
    public void Validate_LobbyNotBetweenMinAndMax_ReturnsFailed()
    {
        var settings = new ElevatorSettings()
        {
            CarCount = 1,
            MaxFloor = 2,
            MinFloor = 1,
            LobbyFloor = 0
        };
        var actual = Sut.Validate(null, settings);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Failed, Is.True);
            Assert.That(actual.FailureMessage, Is.EqualTo("LobbyFloor must be between MinFloor and MaxFloor"));
        });
    }
}