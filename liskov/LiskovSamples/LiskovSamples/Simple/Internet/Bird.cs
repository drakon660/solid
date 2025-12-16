namespace LiskovSamples.Simple.Internet;

public class Bird
{
    public string Name { get; protected set; }
    public double Weight { get; protected set; }

    public Bird(string name, double weight)
    {
        Name = name;
        Weight = weight;
    }
    
    public virtual void Fly()
    {
        Console.WriteLine($"{Name} is flying!");
    }

    public virtual void Eat()
    {
        Console.WriteLine($"{Name} is eating.");
    }
}


public class Penguin : Bird
{
    public Penguin(string name, double weight) : base(name, weight)
    {
    }
    
    public override void Fly()
    {
        throw new InvalidOperationException("Penguins cannot fly!");
    }
}

