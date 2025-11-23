namespace CodeRocket.DbTools.Models;

/// <summary>
/// Migration related constants
/// </summary>
internal static class MigrationConstants
{
    public const string Initial = "Initial";
}

/// <summary>
/// Represents a database version record stored in db_versions table
/// </summary>
public class DatabaseVersion
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Represents migration information from the file system
/// </summary>
public class Migration
{
    public string Version { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public List<string> SqlFiles { get; set; } = new();
    public string? Description { get; set; }
    
    /// <summary>
    /// Parse version string to allow proper version comparison
    /// </summary>
    public Version GetParsedVersion()
    {
        if (Version == MigrationConstants.Initial)
            return new Version(0, 0, 0);
        
        return System.Version.TryParse(Version, out var result) 
            ? result : 
            new Version(0, 0, 0);
    }
}

/// <summary>
/// Result of migration operation
/// </summary>
public class MigrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> ExecutedScripts { get; set; } = new();
    public Exception? Exception { get; set; }
}
