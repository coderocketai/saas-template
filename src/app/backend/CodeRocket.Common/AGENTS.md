# CodeRocket.Common - Shared Project Library

## Documentation Language Policy

**IMPORTANT**: All documentation, comments, specification files, and .md files in this project MUST be written in English. This includes:
- Code comments and XML documentation
- README files and technical documentation
- API specifications and schemas
- Configuration documentation
- Commit messages and pull request descriptions

This policy ensures consistency, improves code maintainability, and facilitates collaboration in international development teams.

## Library Goals and Purpose

**CodeRocket.Common** is the core shared library of the project that contains base components, data models, helper functions, and interfaces used across all application layers (API, DataAccess, Services, Bots).

### Main Goals:
- **Unification** - providing unified base classes and interfaces
- **Code Reuse** - avoiding duplication of common logic
- **Type Safety** - ensuring strong typing throughout the project
- **Standardization** - unified approaches to working with data and operation results

## Library Structure

```
CodeRocket.Common/
├── Constants/              # Application constants
│   └── ApplicationConstants.cs
├── Dto/                   # Data Transfer Objects and operation results
│   ├── Pagination.cs      # Pagination classes
│   └── Result.cs          # Operation results
├── Enums/                 # Enumerations
│   └── CommonEnums.cs
├── Extensions/            # Extension methods
│   ├── DateTimeExtensions.cs
│   └── StringExtensions.cs
├── Helpers/               # Helper functions
│   ├── DateTimeHelper.cs
│   ├── StringHelper.cs
│   └── ValidationHelper.cs
├── Interfaces/            # Interfaces
│   └── IBaseEntity.cs
└── Models/                # Data models
    ├── ModelBase.cs       # Base model
    └── Users/             # User models
        └── User.cs
```

## Detailed Component Description

### 1. Common.Models - Project Data Models

#### ModelBase.cs
Base class for all models in the project. Contains common properties:
- `Id` - unique identifier
- `CreatedAt` - creation date (automatically set to UTC)
- `UpdatedAt` - last update date (automatically set to UTC)
- `IsDeleted` - soft delete flag (default false)

Implements the `IBaseEntity` interface, which combines:
- `IIdentity` - for entities with unique identifier
- `IAuditable` - for entities with audit information (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
- `ISoftDeletable` - for entities with soft delete support

### 2. Common.Dto - Data Transfer Objects and Operation Results

#### Pagination.cs
Contains classes for pagination:
- `PaginationRequest` - pagination request with sorting parameters (Page, PageSize, SortBy, SortDescending)
- `PaginationResponse<T>` - pagination response with metadata (Items, TotalCount, Page, PageSize, TotalPages, HasNextPage, HasPreviousPage)

#### Result.cs
Universal wrappers for operation results:
- `Result<T>` - result with data of type T
- `Result` - result without data
- Support for successful and failed operations
- Collection of errors and messages
- Static Success() and Failure() methods for creating results

### 3. Common.Helpers - Helper Functions

#### DateTimeHelper.cs
Utilities for working with dates and times:
- `UtcNow` - current UTC time
- `ToUnixTimestamp()` / `FromUnixTimestamp()` - Unix timestamp conversion
- `IsInPast()` - check for past date
- `StartOfDay()` / `EndOfDay()` - start/end of day

#### StringHelper.cs
Utilities for working with strings:
- `IsNullOrEmpty()` / `IsNullOrWhiteSpace()` - null/empty checks
- `Truncate()` - truncate string to specified length
- `ToMd5Hash()` - generate MD5 hash
- `ToCamelCase()` / `ToPascalCase()` - case conversion

#### ValidationHelper.cs
Utilities for data validation:
- `IsValidEmail()` - email address validation
- `IsValidLength()` - string length validation
- `IsRequired()` - required field check
- `IsValidPhoneNumber()` - Russian phone number validation

### 4. Common.Extensions - Extension Methods

#### StringExtensions.cs
String extensions:
- `IsNullOrEmpty()` / `IsNullOrWhiteSpace()` - null/empty checks
- `ToNullableInt()` - safe conversion to int?
- `Capitalize()` - capitalize first letter

#### DateTimeExtensions.cs
DateTime extensions:
- `ToUnixTimestamp()` - convert to Unix timestamp
- `IsToday()` - check if date is today
- `GetAge()` - calculate age from birth date

### 5. Common.Interfaces - Interfaces

#### IBaseEntity.cs
Set of interfaces for entity typing:
- `IIdentity` - entity with unique identifier Id
- `ISoftDeletable` - soft delete support (IsDeleted)
- `IAuditable` - audit changes (CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
- `IBaseEntity` - base interface inheriting from IIdentity, IAuditable, and ISoftDeletable

### 6. Common.Constants - Application Constants

#### ApplicationConstants.cs
Common application constants:
- Localization and culture settings (DefaultCulture)
- Default pagination parameters (DefaultPageSize, MaxPageSize)
- Date and time formats (DateFormats)
- Validation messages (ValidationMessages)

### 7. Common.Enums - Enumerations

#### CommonEnums.cs
Main project enumerations:
- `UserRole` - user roles in the system (Guest, User, Moderator, Administrator, SuperAdmin)
- `EntityStatus` - entity statuses (Active, Inactive, Pending, Blocked, Deleted)
- `OperationType` - CRUD operation types

## Usage in Other Projects

### In API Layer:
```csharp
// Controllers return Result<T>
public async Task<Result<UserDto>> GetUser(int id)
{
    var user = await _userService.GetByIdAsync(id);
    return user != null
        ? Result<UserDto>.Success(user)
        : Result<UserDto>.Failure("User not found");
}

// Using pagination
public async Task<PaginationResponse<UserDto>> GetUsers(PaginationRequest request)
{
    var users = await _userService.GetUsersPagedAsync(request);
    return users;
}
```

### In DataAccess Layer:
```csharp
// Models inherit from ModelBase
public class Product : ModelBase
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### In Services Layer:
```csharp
// Using helpers and validation
if (!ValidationHelper.IsValidEmail(email))
{
    return Result.Failure("Invalid email format");
}

// Using extensions
if (user.CreatedAt.IsToday())
{
    // logic for new users
}
```

## Best Practices

### 1. Inheriting from ModelBase
All domain entities should inherit from `ModelBase`:
```csharp
public class Order : ModelBase
{
    public decimal Amount { get; set; }
    public int UserId { get; set; }
}
```

### 2. Using Result<T>
All service methods should return `Result<T>` for unified error handling:
```csharp
public async Task<Result<User>> CreateUserAsync(CreateUserDto dto)
{
    try
    {
        // creation logic
        return Result<User>.Success(user);
    }
    catch (Exception ex)
    {
        return Result<User>.Failure(ex.Message);
    }
}
```

### 3. Data Validation
Use `ValidationHelper` for data validation:
```csharp
var errors = new List<string>();
if (!ValidationHelper.IsRequired(model.Name))
    errors.Add("Name is required");
if (!ValidationHelper.IsValidEmail(model.Email))
    errors.Add("Invalid email format");

if (errors.Any())
    return Result.Failure(errors);
```

### 4. Working with Dates
Always use UTC for storing dates:
```csharp
entity.CreatedAt = DateTimeHelper.UtcNow;
entity.UpdatedAt = DateTimeHelper.UtcNow;
```

### 5. Pagination
Use standard classes for pagination:
```csharp
public async Task<PaginationResponse<UserDto>> GetUsersAsync(PaginationRequest request)
{
    // pagination logic
    return new PaginationResponse<UserDto>
    {
        Items = users,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
}
```

### 6. Using Extensions
Actively use extension methods to simplify code:
```csharp
// Instead of string.IsNullOrEmpty(value)
if (value.IsNullOrEmpty()) { ... }

// Instead of complex age calculations
var age = birthDate.GetAge();

// Instead of manual timestamp conversion
var timestamp = dateTime.ToUnixTimestamp();
```

## Conclusion

The `CodeRocket.Common` library ensures a unified approach to developing all system components, reduces code duplication, and improves project maintainability.

The current project structure is optimized for:
- Logical separation of components (DTOs separated from models)
- Ease of use (extensions separated by types)
- Navigation simplicity (interfaces in one file)

All developers should follow the patterns established in the library and use the provided base classes and utilities.

