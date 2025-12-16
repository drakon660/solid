using AwesomeAssertions;
using LiskovSamples.Simple.Internet;

namespace LiskovSamples.Tests;

public class AreaCalculatorTests
{
    [Fact]
    public void Six_For_2x3_Rectangle()
    {
        var myRectangle = new Rectangle { Height = 2, Width = 3 };
        var result = AreaCalculator.CalculateArea(myRectangle);
        result.Should().Be(6);
    }
    
    [Fact]
    public void Ninefor3x3Squre()
    {
        var mySquare = new Square { Height = 3 };
        var result = AreaCalculator.CalculateArea(mySquare);
        result.Should().Be(9);
    }
   
    [Fact]
    public void TwentyFourfor4x6RectanglefromSquare()
    {
        Rectangle newRectangle = new Square();
        newRectangle.Height = 4; 
        newRectangle.Width = 6;

        var result = AreaCalculator.CalculateArea(newRectangle);
        result.Should().Be(36);
    }
}