namespace CodeRocket.DataAccess.Database.SqlQueries;

/// <summary>
/// SQL queries for User entity operations (PostgreSQL)
/// </summary>
public static class UserQueries
{
    public const string GetById = @"
        SELECT id, role, email, telegram_id, discord_id, first_name, last_name, display_name, 
               created_by, updated_by, created_at, updated_at, is_deleted
        FROM users 
        WHERE id = @Id AND is_deleted = false";

    public const string GetByEmail = @"
        SELECT id, role, email, telegram_id, discord_id, first_name, last_name, display_name, 
               created_by, updated_by, created_at, updated_at, is_deleted
        FROM users 
        WHERE email = @Email AND is_deleted = false";

    public const string GetByTelegramId = @"
        SELECT id, role, email, telegram_id, discord_id, first_name, last_name, display_name, 
               created_by, updated_by, created_at, updated_at, is_deleted
        FROM users 
        WHERE telegram_id = @TelegramId AND is_deleted = false";

    public const string GetByDiscordId = @"
        SELECT id, role, email, telegram_id, discord_id, first_name, last_name, display_name, 
               created_by, updated_by, created_at, updated_at, is_deleted
        FROM users 
        WHERE discord_id = @DiscordId AND is_deleted = false";

    public const string GetAll = @"
        SELECT id, role, email, telegram_id, discord_id, first_name, last_name, display_name, 
               created_by, updated_by, created_at, updated_at, is_deleted
        FROM users 
        WHERE is_deleted = false
        ORDER BY {0} {1}
        LIMIT @PageSize OFFSET @Offset";

    public const string GetByRole = @"
        SELECT id, role, email, telegram_id, discord_id, first_name, last_name, display_name, 
               created_by, updated_by, created_at, updated_at, is_deleted
        FROM users 
        WHERE role = @Role AND is_deleted = false
        ORDER BY {0} {1}
        LIMIT @PageSize OFFSET @Offset";

    public const string GetTotalCount = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE is_deleted = false";

    public const string GetTotalCountByRole = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE role = @Role AND is_deleted = false";

    public const string Create = @"
        INSERT INTO users (role, email, telegram_id, discord_id, first_name, last_name, display_name, 
                          created_by, updated_by, created_at, updated_at, is_deleted)
        VALUES (@Role, @Email, @TelegramId, @DiscordId, @FirstName, @LastName, @DisplayName, 
                @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt, @IsDeleted)
        RETURNING id;";

    public const string Update = @"
        UPDATE users 
        SET role = @Role, email = @Email, telegram_id = @TelegramId, discord_id = @DiscordId, 
            first_name = @FirstName, last_name = @LastName, display_name = @DisplayName,
            updated_by = @UpdatedBy, updated_at = @UpdatedAt
        WHERE id = @Id AND is_deleted = false";

    public const string SoftDelete = @"
        UPDATE users 
        SET is_deleted = true, updated_at = @UpdatedAt, updated_by = @UpdatedBy
        WHERE id = @Id";

    public const string Exists = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE id = @Id AND is_deleted = false";

    public const string IsEmailTaken = @"
        SELECT COUNT(*) 
        FROM users 
        WHERE email = @Email AND is_deleted = false AND (@ExcludeUserId IS NULL OR id != @ExcludeUserId)";
}
