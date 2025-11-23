using System.Security.Cryptography;
using System.Text;
namespace CodeRocket.Common.Helpers;

/// <summary>
/// Helper methods for string operations
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Checks if string is null or empty
    /// </summary>
    public static bool IsNullOrEmpty(string? value)
    {
        return string.IsNullOrEmpty(value);
    }
    
    /// <summary>
    /// Checks if string is null, empty or whitespace
    /// </summary>
    public static bool IsNullOrWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
    
    /// <summary>
    /// Truncates string to specified length
    /// </summary>
    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
    
    /// <summary>
    /// Generates MD5 hash from string
    /// </summary>
    public static string ToMd5Hash(string input)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes).ToLower();
    }
    
    /// <summary>
    /// Converts string to camelCase
    /// </summary>
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLowerInvariant(input[0]) + input[1..];
    }
    
    /// <summary>
    /// Converts string to PascalCase
    /// </summary>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpperInvariant(input[0]) + input[1..];
    }
}

