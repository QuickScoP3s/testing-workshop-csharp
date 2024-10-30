namespace EdgeCases;

public class Clock : IClock
{
    public DateTime Now => DateTime.Now;
}

internal interface IClock
{
    public DateTime Now { get; }
}
