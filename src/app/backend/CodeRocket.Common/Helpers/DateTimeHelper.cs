namespace CodeRocket.Common.Helpers;

/// <summary>
/// Helper methods for working with dates and times
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Gets the current UTC time
    /// </summary>
    public static DateTime UtcNow => DateTime.UtcNow;
    
    /// <summary>
    /// Converts DateTime to Unix timestamp
    /// </summary>
    public static long ToUnixTimestamp(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    public static DateTime FromUnixTimestamp(long unixTimestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
    }
    
    /// <summary>
    /// Checks if date is in the past
    /// </summary>
    public static bool IsInPast(DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }
    
    /// <summary>
    /// Gets the start of day for given date
    /// </summary>
    public static DateTime StartOfDay(DateTime dateTime)
    {
        return dateTime.Date;
    }
    
    /// <summary>
    /// Gets the end of day for given date
    /// </summary>
    public static DateTime EndOfDay(DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }
}