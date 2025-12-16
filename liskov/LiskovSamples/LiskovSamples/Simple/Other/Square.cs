namespace LiskovSamples.Simple.Other;

//Rectangle's Implicit Contract (Invariant)
//Rectangle invariant rule : independent dimensions
public class Square : Rectangle
{
    //precondition for constructor: any int height, any int width (must be height != width)
    public Square(int size) : base(size, size)
    {
        Height = size;
        Width = size;
    }

    //Post-conditions(WEAKER than Rectangle's)
    //(Constraint violation) If the subtype promises something about how its objects change over time, that promise must be at least as strong as what the supertype promises."
    public override void SetHeight(int value)
    {
        Height = value;
        Width = value;
    }

    //Post-conditions(WEAKER than Rectangle's)
    //(Constraint violation) If the subtype promises something about how its objects change over time, that promise must be at least as strong as what the supertype promises."
    public override void SetWidth(int value)
    {
        Width = value;
        Height = value;
    }
}