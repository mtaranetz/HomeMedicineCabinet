namespace HomeMedicineCabinet.UI.Helpers;

public static class QuantityFormatter
{
    public static string Format(decimal value)
    {
        if (value % 1 == 0)
        {
            return value.ToString("0");
        }

        if (value * 10 % 1 == 0)
        {
            return value.ToString("0.0");
        }

        return value.ToString("0.##");
    }
}