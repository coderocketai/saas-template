# CodeRocket.DataAccess - Data Access Layer

## Documentation Language Policy

**IMPORTANT**: All documentation, comments, specification files, and .md files in this project MUST be written in English. This includes:
- Code comments and XML documentation
- README files and technical documentation
- API specifications and schemas
- Configuration documentation
- Commit messages and pull request descriptions

This policy ensures consistency, improves code maintainability, and facilitates collaboration in international development teams.

## Library Goals and Purpose

**CodeRocket.DataAccess** is the data access layer of the project that provides unified access to the MariaDB database using Dapper ORM. This layer implements the Repository pattern and handles all database operations with strong typing and async/await patterns.

### Main Goals:
- **Database Abstraction** - providing clean abstraction over database operations
- **Performance** - using Dapper for fast data access with minimal overhead
- **Type Safety** - ensuring strongly typed database operations
- **Consistency** - unified patterns for all data access operations
- **Testability** - easily mockable interfaces for unit testing

## Library Structure

```
CodeRocket.DataAccess/
├── Database/                    # Database infrastructure
│   ├── ConnectionFactory.cs    # MariaDB connection factory
│   ├── create_users_table.sql  # Database schema scripts
│   └── SqlQueries/             # SQL query constants
│       └── UserQueries.cs      # User-related SQL queries
├── Interfaces/                 # Repository interfaces
│   ├── IBaseRepository.cs      # Base repository interface
│   └── IUserRepository.cs      # User repository interface
├── Repositories/               # Repository implementations
│   └── UserRepository.cs       # User repository implementation
└── DI.cs                       # Dependency injection configuration
```

## Detailed Component Description

### 1. Database Infrastructure

#### ConnectionFactory.cs
Factory class for managing MariaDB database connections:
- Creates and manages `MySqlConnection` instances
- Handles connection string configuration from `appsettings.json`
- Provides both synchronous and asynchronous connection creation
- Ensures proper connection lifecycle management

**Key Methods:**
- `CreateConnection()` - creates new connection instance
- `CreateOpenConnectionAsync()` - creates and opens connection asynchronously

#### SqlQueries/
Directory containing SQL query constants organized by entity:
- **UserQueries.cs** - all SQL queries for User entity operations
- Separates SQL logic from repository code
- Provides compile-time safety for SQL queries
- Enables easy query optimization and maintenance

### 2. Repository Interfaces

#### IBaseRepository<T>
Generic base repository interface that defines standard CRUD operations:
- `GetByIdAsync(int id)` - retrieve entity by primary key
- `GetAllAsync(PaginationRequest request)` - paginated entity retrieval
- `CreateAsync(T entity)` - create new entity
- `UpdateAsync(T entity)` - update existing entity
- `DeleteAsync(int id)` - soft delete entity
- `ExistsAsync(int id)` - check entity existence

**Generic Constraints:**
- `T : class, IBaseEntity` - ensures entity implements base interface

#### IUserRepository
Specialized repository interface for User entity operations:
- Extends `IBaseRepository<User>` with User-specific methods
- `GetByEmailAsync(string email)` - find user by email
- `GetByTelegramIdAsync(string telegramId)` - find user by Telegram ID
- `GetByDiscordIdAsync(string discordId)` - find user by Discord ID
- `GetByRoleAsync(int role, PaginationRequest request)` - paginated users by role
- `IsEmailTakenAsync(string email, int? excludeUserId)` - email uniqueness check

### 3. Repository Implementations

#### UserRepository
Concrete implementation of `IUserRepository` using Dapper and MariaDB:
- **Async Operations** - all methods use async/await patterns
- **Connection Management** - proper using statements with ConnectionFactory
- **Parameter Safety** - all queries use Dapper parameters to prevent SQL injection
- **Soft Delete Support** - implements soft delete pattern with IsDeleted flag
- **Pagination** - supports MySQL LIMIT/OFFSET pagination
- **Sorting** - dynamic sorting with configurable columns and directions

**Key Features:**
- Automatic `CreatedAt`/`UpdatedAt` timestamp management
- Input validation (null checks, empty string handling)
- Proper error handling and resource disposal
- SQL query constants separation for maintainability

### 4. Dependency Injection Configuration

#### DI.cs
Extension method for configuring DataAccess layer services:
- **AddDataAccess()** - registers all DataAccess services in DI container
- Encapsulates all repository registrations in one place
- Provides clean interface for consuming projects
- Follows extension method pattern for IServiceCollection

**Key Features:**
- Centralized service registration
- Scoped lifetime for repositories and ConnectionFactory
- Easy integration with host applications
- Extensible design for adding new repositories

## Database Schema Design

### Users Table Structure
```sql
CREATE TABLE users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Role INT NOT NULL DEFAULT 0,
    Email VARCHAR(255) NULL UNIQUE,
    TelegramId VARCHAR(50) NULL UNIQUE,
    DiscordId VARCHAR(50) NULL UNIQUE,
    FirstName VARCHAR(100) NULL,
    LastName VARCHAR(100) NULL,
    DisplayName VARCHAR(100) NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);
```

**Indexes for Performance:**
- `idx_users_email` - for email lookups
- `idx_users_telegram_id` - for Telegram integration
- `idx_users_discord_id` - for Discord integration
- `idx_users_role` - for role-based queries
- `idx_users_is_deleted` - for soft delete filtering

## Usage Patterns

### 1. Basic Repository Usage
```csharp
// Dependency injection setup
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ConnectionFactory>();

// Repository usage in service layer
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<User?> GetUserAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }
}
```

### 2. Pagination Usage
```csharp
public async Task<PaginationResponse<User>> GetUsersPagedAsync(int page, int pageSize)
{
    var request = new PaginationRequest
    {
        Page = page,
        PageSize = pageSize,
        SortBy = "CreatedAt",
        SortDescending = true
    };
    
    return await _userRepository.GetAllAsync(request);
}
```

### 3. Creating New Entities
```csharp
public async Task<User> CreateUserAsync(CreateUserDto dto)
{
    var user = new User
    {
        Email = dto.Email,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Role = (int)UserRole.User,
        CreatedBy = "system"
    };
    
    return await _userRepository.CreateAsync(user);
}
```

### 4. Validation and Business Logic
```csharp
public async Task<Result<User>> UpdateUserEmailAsync(int userId, string newEmail)
{
    // Check if email is already taken
    if (await _userRepository.IsEmailTakenAsync(newEmail, userId))
    {
        return Result<User>.Failure("Email is already taken");
    }
    
    var user = await _userRepository.GetByIdAsync(userId);
    if (user == null)
    {
        return Result<User>.Failure("User not found");
    }
    
    user.Email = newEmail;
    user.UpdatedBy = "current_user"; // TODO: Get from context
    
    var updatedUser = await _userRepository.UpdateAsync(user);
    return Result<User>.Success(updatedUser);
}
```

## Best Practices

### 1. SQL Query Organization
Store all SQL queries as constants in dedicated query classes:
```csharp
public static class UserQueries
{
    public const string GetById = @"
        SELECT Id, Role, Email, TelegramId, DiscordId, FirstName, LastName, DisplayName, 
               CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, IsDeleted
        FROM users 
        WHERE Id = @Id AND IsDeleted = 0";
}
```

### 2. Connection Management
Always use `using` statements for proper connection disposal:
```csharp
public async Task<User?> GetByIdAsync(int id)
{
    using var connection = await _connectionFactory.CreateOpenConnectionAsync();
    return await connection.QueryFirstOrDefaultAsync<User>(UserQueries.GetById, new { Id = id });
}
```

### 3. Parameter Safety
Always use Dapper parameters to prevent SQL injection:
```csharp
// Good - parameterized query
var users = await connection.QueryAsync<User>(sql, new { Role = role, Offset = offset });

// Bad - string concatenation (vulnerable to SQL injection)
var sql = $"SELECT * FROM users WHERE role = {role}";
```

### 4. Async/Await Patterns
Use async methods consistently throughout the data access layer:
```csharp
// Repository method
public async Task<User> CreateAsync(User entity)
{
    using var connection = await _connectionFactory.CreateOpenConnectionAsync();
    var id = await connection.QuerySingleAsync<int>(UserQueries.Create, entity);
    entity.Id = id;
    return entity;
}
```

### 5. Error Handling
Implement proper error handling and let exceptions bubble up to service layer:
```csharp
public async Task<User> UpdateAsync(User entity)
{
    if (entity == null)
        throw new ArgumentNullException(nameof(entity));
    
    entity.UpdatedAt = DateTimeHelper.UtcNow;
    
    using var connection = await _connectionFactory.CreateOpenConnectionAsync();
    await connection.ExecuteAsync(UserQueries.Update, entity);
    
    return entity;
}
```

### 6. Soft Delete Implementation
Implement soft delete pattern consistently:
```csharp
public async Task<bool> DeleteAsync(int id)
{
    using var connection = await _connectionFactory.CreateOpenConnectionAsync();
    var affectedRows = await connection.ExecuteAsync(UserQueries.SoftDelete, new 
    { 
        Id = id, 
        UpdatedAt = DateTimeHelper.UtcNow,
        UpdatedBy = "current_user" // TODO: Get from context
    });
    
    return affectedRows > 0;
}
```

## Configuration

### Connection String Setup
Configure MariaDB connection in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MainConnection": "Server=localhost;Database=coderocket;User=root;Password=password;Allow User Variables=true;"
  }
}
```

### Dependency Injection Registration
Register data access components in `Program.cs` using the extension method:
```csharp
// Using the DI.cs extension method (Recommended)
builder.Services.AddDataAccess();

// Or register services manually
builder.Services.AddScoped<ConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**Benefits of using AddDataAccess():**
- Centralized service registration
- Consistent lifetime management (Scoped)
- Easy to extend when adding new repositories
- Clean separation of concerns

## Testing Considerations

### 1. Repository Testing
Use in-memory database or test containers for integration tests:
```csharp
[Test]
public async Task GetByIdAsync_ExistingUser_ReturnsUser()
{
    // Arrange
    var repository = new UserRepository(_connectionFactory);
    
    // Act
    var user = await repository.GetByIdAsync(1);
    
    // Assert
    Assert.That(user, Is.Not.Null);
    Assert.That(user.Id, Is.EqualTo(1));
}
```

### 2. Mocking Repositories
Mock repository interfaces in unit tests:
```csharp
[Test]
public async Task GetUserAsync_UserExists_ReturnsUser()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var expectedUser = new User { Id = 1, Email = "test@example.com" };
    mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expectedUser);
    
    var service = new UserService(mockRepository.Object);
    
    // Act
    var result = await service.GetUserAsync(1);
    
    // Assert
    Assert.That(result, Is.EqualTo(expectedUser));
}
```

## Performance Optimization

### 1. Database Indexing
Ensure proper indexes for frequently queried columns:
- Primary keys (automatic)
- Foreign keys
- Unique constraints (email, external IDs)
- Frequently filtered columns (role, status)

### 2. Query Optimization
- Use `SELECT` with specific column lists instead of `SELECT *`
- Implement proper pagination with `LIMIT` and `OFFSET`
- Use `QueryFirstOrDefaultAsync` for single entity retrieval
- Use `QueryAsync` for multiple entity retrieval

### 3. Connection Pooling
MariaDB connector automatically handles connection pooling. Configure pool settings in connection string if needed:
```
Server=localhost;Database=coderocket;User=root;Password=password;Maximum Pool Size=100;Minimum Pool Size=5;
```

## Conclusion

The `CodeRocket.DataAccess` layer provides a robust, performant, and maintainable foundation for database operations. Key principles:

- **Separation of Concerns** - SQL queries separated from business logic
- **Type Safety** - strongly typed operations with compile-time safety
- **Performance** - optimized queries with proper indexing and connection management
- **Maintainability** - clean interfaces and consistent patterns
- **Testability** - mockable interfaces and dependency injection support

All developers should follow the established patterns and use the provided base interfaces when implementing new repositories.
