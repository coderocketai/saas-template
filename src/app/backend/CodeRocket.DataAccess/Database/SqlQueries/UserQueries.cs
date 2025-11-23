namespace CodeRocket.DataAccess.Database.SqlQueries;

/// <summary>
/// SQL queries for User entity operations
/// </summary>
public static class UserQueries
{
    public const string GetById = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE Id = @Id AND IsDeleted = 0";

    public const string GetByEmail = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE Email = @Email AND IsDeleted = 0";

    public const string GetByTelegramId = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE TelegramId = @TelegramId AND IsDeleted = 0";

    public const string GetByDiscordId = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE DiscordId = @DiscordId AND IsDeleted = 0";

    public const string GetAll = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE IsDeleted = 0
        ORDER BY {0} {1}
        LIMIT @Offset, @PageSize";

    public const string GetByRole = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE Role = @Role AND IsDeleted = 0
        ORDER BY {0} {1}
        LIMIT @Offset, @PageSize";

    public const string GetTotalCount = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE IsDeleted = 0";

    public const string GetTotalCountByRole = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE Role = @Role AND IsDeleted = 0";

    public const string Create = @"
        INSERT INTO users (Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
                          CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted)
        VALUES (@Role, @Email, @TelegramId, @DiscordId, @FirstName, @LastName, @DisplayName, 
                @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt, @IsDeleted);
        SELECT LAST_INSERT_ID();";

    public const string Update = @"
        UPDATE users 
        SET Role = @Role, Email = @Email, TelegramId = @TelegramId, DiscordId = @DiscordId, 
            FirstName = @FirstName, LastName = @LastName, DisplayName = @DisplayName,
            UpdatedBy = @UpdatedBy, UpdatedAt = @UpdatedAt
        WHERE Id = @Id AND IsDeleted = 0";

    public const string SoftDelete = @"
        UPDATE users 
        SET IsDeleted = 1, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy
        WHERE Id = @Id";

    public const string Exists = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE Id = @Id AND IsDeleted = 0";

    public const string IsEmailTaken = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE Email = @Email AND IsDeleted = 0 AND (@ExcludeUserId IS NULL OR Id != @ExcludeUserId)";
}
