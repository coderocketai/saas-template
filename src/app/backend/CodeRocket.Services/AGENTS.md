# CodeRocket.Services - Business Logic Layer

## Documentation Language Policy

**IMPORTANT**: All documentation, comments, specification files, and .md files in this project MUST be written in English. This includes:
- Code comments and XML documentation
- README files and technical documentation
- API specifications and schemas
- Configuration documentation
- Commit messages and pull request descriptions

This policy ensures consistency, improves code maintainability, and facilitates collaboration in international development teams.

## Library Goals and Purpose

**CodeRocket.Services** is the business logic layer of the project that implements core business rules, data validation, and orchestrates operations between the API layer and Data Access layer. This layer provides service interfaces and their implementations following the Service pattern.

### Main Goals:
- **Business Logic Isolation** - separating business rules from API and data layers
- **Validation** - ensuring data integrity and business rule compliance
- **Orchestration** - coordinating complex operations across multiple repositories
- **Abstraction** - providing clean interfaces for the API layer to consume
- **Testability** - easily mockable service interfaces for unit testing
- **Reusability** - sharing business logic across different application entry points (API, Bots)

## Library Structure

```
CodeRocket.Services/
├── Interfaces/              # Service interfaces
│   └── IUserService.cs     # User service interface
├── Users/                   # User domain services
│   └── UserService.cs      # User service implementation
├── DI.cs                   # Dependency injection configuration
└── AGENTS.md              # This documentation file
```

## Detailed Component Description

### 1. Service Interfaces

#### IUserService.cs
Defines the contract for user-related business operations:
- **GetAllUsersAsync** - retrieves paginated list of users
- **GetUserByIdAsync** - retrieves a single user by ID
- **CreateUserAsync** - creates a new user with validation
- **UpdateUserAsync** - updates existing user data
- **DeleteUserAsync** - performs soft delete on a user

All methods are asynchronous and return appropriate types (Task<T>, Task<bool>).

### 2. Service Implementations

#### Users/UserService.cs
Implements business logic for user management:

**Dependencies:**
- `IUserRepository` - for data access operations

**Key Features:**
- **Pagination Support** - uses PaginationRequest/PaginationResponse DTOs
- **Timestamp Management** - automatically sets CreatedAt/UpdatedAt fields
- **Soft Delete** - marks records as deleted without physical removal
- **Data Validation** - ensures business rules are followed (can be extended)

**Methods:**

1. **GetAllUsersAsync(PaginationRequest request)**
   - Returns paginated list of users
   - Delegates to repository for data retrieval
   - Returns: `Task<PaginationResponse<User>>`

2. **GetUserByIdAsync(int id)**
   - Retrieves single user by ID
   - Returns null if not found
   - Returns: `Task<User?>`

3. **CreateUserAsync(User user)**
   - Creates new user in the system
   - Sets CreatedAt, UpdatedAt timestamps
   - Sets IsDeleted = false
   - Returns created user with generated ID
   - Returns: `Task<User>`

4. **UpdateUserAsync(User user)**
   - Updates existing user data
   - Updates UpdatedAt timestamp
   - Returns updated user
   - Returns: `Task<User>`

5. **DeleteUserAsync(int id)**
   - Performs soft delete (sets IsDeleted = true)
   - Returns success status
   - Returns: `Task<bool>`

### 3. Dependency Injection Configuration

#### DI.cs
Extension methods for registering services in the DI container:

**Method:** `AddServices(IServiceCollection services)`
- Registers all service interfaces with their implementations
- Uses Scoped lifetime for service instances
- Currently registers:
  - `IUserService` → `UserService`

**Usage Example:**
```csharp
// In Program.cs or Startup.cs
services.AddServices();
```

## Design Patterns and Best Practices

### 1. Service Pattern
- Clear separation between business logic and data access
- Services orchestrate operations and enforce business rules
- Repositories handle only data persistence

### 2. Dependency Injection
- Constructor injection for all dependencies
- Interfaces for loose coupling and testability
- Scoped lifetime for service instances

### 3. Async/Await Pattern
- All service methods are asynchronous
- Improves scalability and responsiveness
- Proper async propagation through the call stack

### 4. Data Validation
- Services are responsible for validating business rules
- Can be extended with FluentValidation or custom validators
- Timestamp management handled automatically

### 5. Soft Delete Pattern
- Records are marked as deleted, not physically removed
- Maintains data integrity and audit trail
- Can be filtered at repository level

## Future Extensions

### Planned Features:
1. **Validation Layer**
   - Add FluentValidation for complex business rules
   - Email format validation
   - Unique constraint checking

2. **Additional Services**
   - AuthenticationService for user authentication
   - NotificationService for sending notifications
   - AuditService for tracking changes

3. **Error Handling**
   - Custom exceptions for business rule violations
   - Result pattern for operation outcomes
   - Centralized error handling

4. **Caching**
   - Add caching layer for frequently accessed data
   - Cache invalidation strategies
   - Distributed caching support

5. **Authorization**
   - Role-based access control
   - Permission checking in service methods
   - User context propagation

## Dependencies

**NuGet Packages:**
- `Microsoft.Extensions.DependencyInjection.Abstractions` (v9.0.10) - for DI support

**Project References:**
- `CodeRocket.DataAccess` - for repository interfaces and data access
- `CodeRocket.Common` (transitive) - for models, DTOs, and shared types

## Usage Examples

### Example 1: Getting Users with Pagination
```csharp
var request = new PaginationRequest 
{ 
    Page = 1, 
    PageSize = 20,
    SortBy = "CreatedAt",
    SortDescending = true
};

var response = await userService.GetAllUsersAsync(request);
Console.WriteLine($"Total users: {response.TotalCount}");
Console.WriteLine($"Page {response.Page} of {response.TotalPages}");
```

### Example 2: Creating a New User
```csharp
var newUser = new User
{
    Email = "user@example.com",
    FirstName = "John",
    LastName = "Doe",
    DisplayName = "JohnD",
    Role = UserRole.User
};

var createdUser = await userService.CreateUserAsync(newUser);
Console.WriteLine($"User created with ID: {createdUser.Id}");
```

### Example 3: Updating a User
```csharp
var user = await userService.GetUserByIdAsync(1);
if (user != null)
{
    user.DisplayName = "NewDisplayName";
    user.Email = "newemail@example.com";
    
    var updatedUser = await userService.UpdateUserAsync(user);
    Console.WriteLine($"User updated at: {updatedUser.UpdatedAt}");
}
```

### Example 4: Soft Deleting a User
```csharp
var success = await userService.DeleteUserAsync(1);
if (success)
{
    Console.WriteLine("User successfully deleted");
}
```

## Testing Considerations

### Unit Testing:
- Mock `IUserRepository` for testing service logic
- Test timestamp management
- Test validation rules
- Test error handling

### Integration Testing:
- Test with real repository implementation
- Verify database interactions
- Test transaction handling
- Verify pagination works correctly

## Architecture Integration

```
┌─────────────────────┐
│   API Controllers   │ ← Presentation Layer
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Service Layer      │ ← Business Logic (THIS LAYER)
│  - UserService      │
│  - Validation       │
│  - Orchestration    │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│  Repository Layer   │ ← Data Access
│  - UserRepository   │
└──────────┬──────────┘
           │
           ↓
┌─────────────────────┐
│   MariaDB Database  │ ← Data Storage
└─────────────────────┘
```

## Notes for AI Agents

When working with this layer:
1. **Always use interfaces** - never reference concrete implementations in API layer
2. **Validate input** - check for null values and business rule violations
3. **Manage timestamps** - ensure CreatedAt/UpdatedAt are properly set
4. **Use async/await** - all methods should be asynchronous
5. **Return appropriate types** - use nullable types when entity might not exist
6. **Document changes** - add XML comments to all public methods
7. **Follow naming conventions** - method names should clearly indicate their purpose
8. **Keep services focused** - each service should handle one domain area
9. **Don't expose repository details** - services should abstract data access
10. **Consider error handling** - plan for validation failures and exceptions

