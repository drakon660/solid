namespace LiskovSamples.Simple.Other;

public class Rectangle
{
    public int Height { get; set; }
    public int Width { get; set; }

    public Rectangle(int height, int width)
    {
        Width = width;
        Height = height;
    }

    //Post-conditions
    //(Constraint violation) If the subtype promises something about how its objects change over time, that promise must be at least as strong as what the supertype promises."
    public virtual void SetWidth(int value) => Width = value;

    //Post-conditions
    //(Constraint violation) If the subtype promises something about how its objects change over time, that promise must be at least as strong as what the supertype promises."
    public virtual void SetHeight(int value) => Height = value;
}

//Rectangle2's Implicit Contract (Invariant)
//Rectangle2 allows independent dimensions
//invariant: true  // height and width CAN be different
//precondition for constructor: any int height, any int width (can be x != y)

public class ColoredRectangle(int height, int width) : Rectangle(height, width)
{
    public string Color { get; protected set; } = "White";

    public void SetColor(string color)
    {
        Color = color;
    }
}