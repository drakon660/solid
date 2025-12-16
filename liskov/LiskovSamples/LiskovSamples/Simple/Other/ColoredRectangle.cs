namespace LiskovSamples.Simple.Other;

public class ColoredRectangle(int height, int width) : Rectangle(height, width)
{
    public string Color { get; protected set; } = "White";

    public void SetColor(string color)
    {
        Color = color;
    }
}