namespace CodeRocket.Common.Interfaces;

/// <summary>
/// Base interface combining common entity features
/// </summary>
public interface IBaseEntity : IIdentity, IAuditable, ISoftDeletable;

/// <summary>
/// Interface for entities with unique identifier
/// </summary>
public interface IIdentity
{
    int Id { get; set; }
}

/// <summary>
/// Interface for entities that can be soft deleted
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

/// <summary>
/// Interface for entities with audit information
/// </summary>
public interface IAuditable 
{
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}

