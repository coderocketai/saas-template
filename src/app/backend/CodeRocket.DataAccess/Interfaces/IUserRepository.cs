using CodeRocket.Common.Dto;
using CodeRocket.Common.Models.Users;

namespace CodeRocket.DataAccess.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Get user by Telegram ID
    /// </summary>
    /// <param name="telegramId">Telegram ID</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetByTelegramIdAsync(string telegramId);

    /// <summary>
    /// Get user by Discord ID
    /// </summary>
    /// <param name="discordId">Discord ID</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetByDiscordIdAsync(string discordId);

    /// <summary>
    /// Check if email is already taken
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <returns>True if email exists</returns>
    Task<bool> IsEmailTakenAsync(string email, int? excludeUserId = null);

    /// <summary>
    /// Get users by role with pagination
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="request">Pagination request</param>
    /// <returns>Paginated response</returns>
    Task<PaginationResponse<User>> GetByRoleAsync(int role, PaginationRequest request);
}