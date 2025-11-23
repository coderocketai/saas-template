using CodeRocket.Common.Dto;
using CodeRocket.Common.Models.Users;

namespace CodeRocket.Services.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="request">Pagination request</param>
    /// <returns>Paginated response with users</returns>
    Task<PaginationResponse<User>> GetAllUsersAsync(PaginationRequest request);

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Create new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user with ID</returns>
    Task<User> CreateUserAsync(User user);

    /// <summary>
    /// Update existing user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Updated user</returns>
    Task<User> UpdateUserAsync(User user);

    /// <summary>
    /// Soft delete user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteUserAsync(int id);
}