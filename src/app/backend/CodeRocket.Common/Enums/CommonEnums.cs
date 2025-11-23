namespace CodeRocket.Common.Enums;

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    Guest = 0,
    User = 1,
    Moderator = 2,
    Administrator = 3,
    SuperAdmin = 4
}

/// <summary>
/// Operation types
/// </summary>
public enum OperationType
{
    Create = 0,
    Read = 1,
    Update = 2,
    Delete = 3
}
