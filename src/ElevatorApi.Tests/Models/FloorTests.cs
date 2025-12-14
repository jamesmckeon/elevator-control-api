namespace ElevatorApi.Tests.Models;

[Category("Unit")]
public class FloorTests
{
    #region Equals

    [Test]
    public void Equals_SameFloorNumber_ReturnsTrue()
    {
        var firstFloor = new Floor()
        {
            Number = 1
        };

        var secondFloor = new Floor()
        {
            Number = 1
        };
        
        Assert.That(firstFloor.Equals(secondFloor), Is.True);
    }
    
    [Test]
    public void Equals_DifferentFloorNumber_ReturnsFalse()
    {
        var firstFloor = new Floor()
        {
            Number = 1
        };

        var secondFloor = new Floor()
        {
            Number = 2
        };
        
        Assert.That(firstFloor.Equals(secondFloor), Is.False);
    }

    #endregion
    
    #region Equals GetHashCode

    [TestCase(1)]
    [TestCase(0)]
    public void GetHashcode_ReturnsExpected(byte number)
    {

        var floor = new Floor()
        {
            Number = number
        };
        
        Assert.That(floor.GetHashCode(), Is.EqualTo(number.GetHashCode()));
    }
    #endregion
    
    #region Equals (Object Overload)

    [Test]
    public void EqualsOverload_DifferentType_ReturnsFalse()
    {
        var tesObj = new
        {
            FloorNumber = (byte)1
        };

        var sut = new Floor()
        {
            Number = 1
        };
        
        Assert.That(sut.Equals(tesObj), Is.False);
    }
    
    [Test]
    public void EqualsOverload_SameType_ReturnsTrue()
    {
        var firstFloor = new Floor()
        {
            Number = 1
        };
        
        var secondFloor = new Floor()
        {
            Number = 1
        };
        
        Assert.That(firstFloor.Equals(secondFloor as Object), Is.True);
    }
    
    [Test]
    public void EqualsOverload_SameType_ReturnsFalse()
    {
        var firstFloor = new Floor()
        {
            Number = 1
        };
        
        var secondFloor = new Floor()
        {
            Number = 2
        };
        
        Assert.That(firstFloor.Equals(secondFloor as Object), Is.False);
    }
    #endregion
}