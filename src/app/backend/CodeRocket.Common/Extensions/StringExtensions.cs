namespace CodeRocket.Common.Extensions;

/// <summary>
/// Extension methods for strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if string is null or empty
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }
    
    /// <summary>
    /// Checks if string is null, empty or whitespace
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
    
    /// <summary>
    /// Converts string to nullable int
    /// </summary>
    public static int? ToNullableInt(this string? value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }
    
    /// <summary>
    /// Capitalizes first letter of string
    /// </summary>
    public static string Capitalize(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return char.ToUpper(value[0]) + value[1..];
    }
}

