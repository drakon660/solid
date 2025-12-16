namespace LiskovSamples.Simple.Other;

public class Square : Rectangle
{
    public Square(int side) : base(side, side)
    {
        Height = side;
        Width = side;
    }

    public override void SetHeight(int value)
    {
        Height = value;
        Width = value;
    }

    public override void SetWidth(int value)
    {
        Width = value;
        Height = value;
    }
}