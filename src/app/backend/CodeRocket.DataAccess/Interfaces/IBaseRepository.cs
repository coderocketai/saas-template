using CodeRocket.Common.Dto;
using CodeRocket.Common.Interfaces;

namespace CodeRocket.DataAccess.Interfaces;

/// <summary>
/// Base repository interface for entities
/// </summary>
/// <typeparam name="T">Entity type that implements IBaseEntity</typeparam>
public interface IBaseRepository<T> where T : class, IBaseEntity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>Entity or null if not found</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Get all entities with pagination
    /// </summary>
    /// <param name="request">Pagination request</param>
    /// <returns>Paginated response</returns>
    Task<PaginationResponse<T>> GetAllAsync(PaginationRequest request);

    /// <summary>
    /// Create new entity
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <returns>Created entity with ID</returns>
    Task<T> CreateAsync(T entity);

    /// <summary>
    /// Update existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <returns>Updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Soft delete entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Check if entity exists by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <returns>True if exists</returns>
    Task<bool> ExistsAsync(int id);
}
