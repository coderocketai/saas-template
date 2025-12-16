using CodeRocket.DataAccess.Interfaces;
using CodeRocket.DataAccess.Database;
using CodeRocket.DataAccess.Database.SqlQueries;
using CodeRocket.Common.Models.Users;
using CodeRocket.Common.Dto;
using CodeRocket.Common.Helpers;
using Dapper;
using System.Data;

namespace CodeRocket.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ConnectionFactory _connectionFactory;

    public UserRepository(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<User>(UserQueries.GetById, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<User>(UserQueries.GetByEmail, new { Email = email });
    }

    public async Task<User?> GetByTelegramIdAsync(string telegramId)
    {
        if (string.IsNullOrWhiteSpace(telegramId))
            return null;

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<User>(UserQueries.GetByTelegramId, new { TelegramId = telegramId });
    }

    public async Task<User?> GetByDiscordIdAsync(string discordId)
    {
        if (string.IsNullOrWhiteSpace(discordId))
            return null;

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<User>(UserQueries.GetByDiscordId, new { DiscordId = discordId });
    }

    public async Task<PaginationResponse<User>> GetAllAsync(PaginationRequest request)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? PgSqlHelper.IdColumnName : request.SortBy;
        var sortDirection = request.SortDescending ? PgSqlHelper.SortingDirectionDesc : PgSqlHelper.SortingDirectionAsc;
        var offset = (request.Page - 1) * request.PageSize;

        var query = string.Format(UserQueries.GetAll, sortBy, sortDirection);
        
        var users = await connection.QueryAsync<User>(query, new 
        { 
            Offset = offset, 
            PageSize = request.PageSize 
        });

        var totalCount = await connection.QuerySingleAsync<int>(UserQueries.GetTotalCount);

        return new PaginationResponse<User>
        {
            Items = users,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<PaginationResponse<User>> GetByRoleAsync(int role, PaginationRequest request)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? PgSqlHelper.IdColumnName : request.SortBy;
        var sortDirection = request.SortDescending ? PgSqlHelper.SortingDirectionDesc : PgSqlHelper.SortingDirectionAsc;
        var offset = (request.Page - 1) * request.PageSize;

        var query = string.Format(UserQueries.GetByRole, sortBy, sortDirection);
        
        var users = await connection.QueryAsync<User>(query, new 
        { 
            Role = role,
            Offset = offset, 
            PageSize = request.PageSize 
        });

        var totalCount = await connection.QuerySingleAsync<int>(UserQueries.GetTotalCountByRole, new { Role = role });

        return new PaginationResponse<User>
        {
            Items = users,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<User> CreateAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.CreatedAt = DateTimeHelper.UtcNow;
        entity.UpdatedAt = DateTimeHelper.UtcNow;

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var id = await connection.QuerySingleAsync<int>(UserQueries.Create, entity);
        entity.Id = id;
        
        return entity;
    }

    public async Task<User> UpdateAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.UpdatedAt = DateTimeHelper.UtcNow;

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(UserQueries.Update, entity);
        
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(UserQueries.SoftDelete, new 
        { 
            Id = id, 
            UpdatedAt = DateTimeHelper.UtcNow,
            UpdatedBy = (string?)null // TODO: Get from current user context
        });
        
        return affectedRows > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var count = await connection.QuerySingleAsync<int>(UserQueries.Exists, new { Id = id });
        return count > 0;
    }

    public async Task<bool> IsEmailTakenAsync(string email, int? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var count = await connection.QuerySingleAsync<int>(UserQueries.IsEmailTaken, new 
        { 
            Email = email, 
            ExcludeUserId = excludeUserId 
        });
        
        return count > 0;
    }
}