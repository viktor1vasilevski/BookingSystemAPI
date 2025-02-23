namespace Main.Helpers;

public static class RandomHelper
{
    private static Random _random = new Random();

    public static int GenerateRandomInt(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public static double GenerateRandomDouble(double minValue, double maxValue)
    {
        return _random.NextDouble() * (maxValue - minValue) + minValue;
    }
}
