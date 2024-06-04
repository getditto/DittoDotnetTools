namespace DittoTools_Heartbeat;
public static class ISO8601DateFormatter
{
    public static string GetISODate()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}