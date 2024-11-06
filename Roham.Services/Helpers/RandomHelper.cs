namespace Roham.Services.Helpers;

public class RandomHelper
{
    private static readonly Random getrandom = new();

    public static long GetRandomNumber(long min, long max)
    {
        lock (getrandom) // synchronize
        {
            return getrandom.NextInt64(min, max);
        }
    }
}
