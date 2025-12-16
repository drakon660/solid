namespace LiskovSamples.Rules;

public class SubResult : Result;

public class Result
{
    
}

public class Tau
{
    public virtual Result Method()
    {
        return new Result();
    }
}

public class Sigma : Tau
{
    public override SubResult Method()
    {
        return new SubResult();
    }
}

public class Argument;
public class ServiceArgument : Argument; 
public class ServiceSubArgument : Argument; 

public class Service
{
    public virtual void Action(ServiceArgument argument)
    {
         
    }
}

public class ServiceSub : Service
{
    //Not possible in c#
    // public override void Action(Argument argument)
    // {
    //     
    // }
}

public class Sample
{
    public static void Run()
    {
        Service service = new ServiceSub();
        service.Action(new ServiceArgument());
    }
}
