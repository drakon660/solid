namespace LiskovSamples.Simple.Internet;

public interface IDuck
{
    void Swim();
    bool IsSwimming { get; }
}

public class OrganicDuck : IDuck
{
    private bool _isSwimming = false;
    public void Swim()
    {
        Console.WriteLine("OrganicDuck swims");
        _isSwimming = true;
    }

    public bool IsSwimming => _isSwimming;
}

public class ElectricDuck : IDuck
{
    private bool _isSwimming;

    public void Swim()
    {
        if (!IsTurnedOn)
            return;
        _isSwimming = true;
    }

    public bool IsTurnedOn { get; set; }
    public bool IsSwimming => _isSwimming;
}	
