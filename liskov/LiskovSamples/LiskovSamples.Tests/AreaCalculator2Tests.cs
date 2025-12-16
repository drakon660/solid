using AwesomeAssertions;
using LiskovSamples.Simple.Other;

namespace LiskovSamples.Tests;

public class AreaCalculator2Tests
{
    [Fact]
    public void TwentyFourfor4x6RectanglefromSquar2()
    {
        Rectangle newRectangle = new Square(4);
        var result = AreaCalculator2.CalculateArea(newRectangle);
        result.Should().Be(16);
    }
}