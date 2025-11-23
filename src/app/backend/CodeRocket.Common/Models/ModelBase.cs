using CodeRocket.Common.Interfaces;

namespace CodeRocket.Common.Models;

/// <summary>
/// Base class for all models.
/// </summary>
/// <remarks>
/// Contains common properties - Id, CreatedAt, UpdatedAt, and IsDeleted.
/// Used both in API and Database models.
/// </remarks>
public class ModelBase : IBaseEntity
{
    public int Id { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}