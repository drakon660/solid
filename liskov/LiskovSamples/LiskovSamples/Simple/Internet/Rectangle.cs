namespace LiskovSamples.Simple.Internet;

public class Rectangle
{
    public virtual int Height { get; set; }
    public virtual int Width { get; set; }
}

public class Square : Rectangle
{
    private int _height;
    private int _width;
    public override int Height
    {
        get => _height;
        set
        {
            _height = value;
            _width = value;
        }
    }
    public override int Width
    {
        get => _width;
        set
        {
            _width = value;
            _height = value;
        }
    }
}