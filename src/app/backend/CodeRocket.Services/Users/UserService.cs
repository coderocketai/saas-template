using CodeRocket.Common.Dto;
using CodeRocket.Common.Models.Users;
using CodeRocket.DataAccess.Interfaces;
using CodeRocket.Services.Interfaces;

namespace CodeRocket.Services.Users;

public class UserService(IUserRepository repository) : IUserService
{
    /// <summary>
    /// Get all users with pagination
    /// </summary>
    /// <param name="request">Pagination request</param>
    /// <returns>Paginated response with users</returns>
    public async Task<PaginationResponse<User>> GetAllUsersAsync(PaginationRequest request)
    {
        return await repository.GetAllAsync(request);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User or null if not found</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    /// <summary>
    /// Create new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user with ID</returns>
    public async Task<User> CreateUserAsync(User user)
    {
        // Set creation timestamp
        user.CreatedAt = DateTime.UtcNow;
        user.IsDeleted = false;

        return await repository.CreateAsync(user);
    }

    /// <summary>
    /// Update existing user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Updated user</returns>
    public async Task<User> UpdateUserAsync(User user)
    {
        // Update timestamp
        user.UpdatedAt = DateTime.UtcNow;

        return await repository.UpdateAsync(user);
    }

    /// <summary>
    /// Soft delete user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteUserAsync(int id)
    {
        return await repository.DeleteAsync(id);
    }
}