namespace CodeRocket.Common.Extensions;

/// <summary>
/// Extension methods for DateTime
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
    
    /// <summary>
    /// Checks if date is today
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }
    
    /// <summary>
    /// Gets age from birth date
    /// </summary>
    public static int GetAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}