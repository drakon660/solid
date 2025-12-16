namespace LiskovSamples.Simple.Other;


public class Rectangle
{
    public int Height { get; protected set; }
    public int Width { get; protected set; }

    public Rectangle(int height, int width)
    {
        Width = width;
        Height = height;
    }
    
    public virtual void SetWidth(int value) => Width = value;
    
    public virtual void SetHeight(int value) => Height = value;
}