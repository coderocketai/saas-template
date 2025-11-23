using System.Text.RegularExpressions;

namespace CodeRocket.Common.Helpers;

/// <summary>
/// Helper methods for validation
/// </summary>
public static class ValidationHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    /// <summary>
    /// Validates email format
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex.IsMatch(email);
    }
    
    /// <summary>
    /// Validates string length
    /// </summary>
    public static bool IsValidLength(string? value, int minLength, int maxLength)
    {
        if (value == null) return minLength == 0;
        return value.Length >= minLength && value.Length <= maxLength;
    }
    
    /// <summary>
    /// Validates that value is not null or empty
    /// </summary>
    public static bool IsRequired(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    
    /// <summary>
    /// Validates phone number format (Russian format)
    /// </summary>
    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
        var phoneRegex = new Regex(@"^(\+7|8)?(\d{10})$");
        return phoneRegex.IsMatch(phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));
    }
}
