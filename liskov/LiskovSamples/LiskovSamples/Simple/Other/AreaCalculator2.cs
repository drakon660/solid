namespace LiskovSamples.Simple.Other;

public class AreaCalculator2
{
    public static int CalculateArea(Rectangle r)
    {
        return r.Height * r.Width;
    }
    
    public static int CalculateArea(Square s)
    {
        return s.Height * s.Height;
    }
}