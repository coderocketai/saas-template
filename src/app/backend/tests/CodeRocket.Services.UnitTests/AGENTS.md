# CodeRocket.Services.UnitTests - Service Layer Unit Tests

## Documentation Language Policy

**IMPORTANT**: All documentation, comments, specification files, and .md files in this project MUST be written in English. This includes:
- Code comments and XML documentation
- README files and technical documentation
- API specifications and schemas
- Configuration documentation
- Commit messages and pull request descriptions

This policy ensures consistency, improves code maintainability, and facilitates collaboration in international development teams.

## Project Goals and Purpose

**CodeRocket.Services.UnitTests** is the unit testing project for the business logic layer (CodeRocket.Services). It provides comprehensive test coverage for service implementations using the MSTest framework and Moq for mocking dependencies.

### Main Goals:
- **Test Coverage** - ensure all service methods are thoroughly tested
- **Isolation** - test business logic in isolation from data access layer
- **Regression Prevention** - catch bugs early before they reach production
- **Documentation** - tests serve as living documentation of expected behavior
- **Confidence** - enable safe refactoring and feature development
- **Fast Feedback** - provide quick test execution for rapid development cycles

## Project Structure

```
CodeRocket.Services.UnitTests/
├── UserServiceTests.cs          # Unit tests for UserService
├── MSTestSettings.cs            # MSTest configuration
├── CodeRocket.Services.UnitTests.csproj  # Project file
└── AGENTS.md                    # This documentation file
```

## Testing Framework and Tools

### MSTest (v3.6.4)
- Microsoft's official testing framework
- Attributes: [TestClass], [TestMethod], [TestInitialize]
- Assertions: Assert.AreEqual, Assert.IsNotNull, Assert.IsTrue, etc.
- Parallel test execution support

### Moq (v4.20.72)
- Popular mocking library for .NET
- Creates mock implementations of interfaces
- Setup expected behavior and verify calls
- Supports It.IsAny<T>() for flexible matching

### Microsoft.NET.Test.Sdk (v17.12.0)
- Test platform for discovering and running tests
- Integration with Visual Studio and CLI

## Test Classes

### UserServiceTests.cs

Comprehensive unit tests for UserService covering all CRUD operations.

**Test Setup:**
- Uses `[TestInitialize]` to set up fresh mocks before each test
- Creates `Mock<IUserRepository>` to isolate UserService from data layer
- Initializes `UserService` with mocked repository

**Test Coverage (10 tests total):**

#### 1. GetAllUsersAsync Tests (2 tests)

**GetAllUsersAsync_WithValidRequest_ReturnsPaginatedUsers**
- **Purpose:** Verify pagination works correctly with users
- **Arrange:** Creates pagination request and expected response with 2 users
- **Act:** Calls GetAllUsersAsync
- **Assert:** Verifies correct count, page number, and repository call

**GetAllUsersAsync_WithEmptyResult_ReturnsEmptyPaginatedResponse**
- **Purpose:** Verify empty result handling
- **Arrange:** Creates pagination request with empty response
- **Act:** Calls GetAllUsersAsync
- **Assert:** Verifies empty items and zero count

#### 2. GetUserByIdAsync Tests (2 tests)

**GetUserByIdAsync_WithExistingId_ReturnsUser**
- **Purpose:** Verify successful user retrieval
- **Arrange:** Mocks repository to return user with ID 1
- **Act:** Calls GetUserByIdAsync(1)
- **Assert:** Verifies user data and repository call

**GetUserByIdAsync_WithNonExistingId_ReturnsNull**
- **Purpose:** Verify null handling for missing users
- **Arrange:** Mocks repository to return null for ID 999
- **Act:** Calls GetUserByIdAsync(999)
- **Assert:** Verifies null result

#### 3. CreateUserAsync Tests (2 tests)

**CreateUserAsync_WithValidUser_SetsTimestampsAndCreatesUser**
- **Purpose:** Verify user creation sets timestamps correctly
- **Arrange:** Creates new user without timestamps
- **Act:** Calls CreateUserAsync
- **Assert:** Verifies CreatedAt is set and IsDeleted is false

**CreateUserAsync_SetsIsDeletedToFalse**
- **Purpose:** Verify IsDeleted is always false on creation
- **Arrange:** Creates user with IsDeleted = true
- **Act:** Calls CreateUserAsync
- **Assert:** Verifies IsDeleted is overridden to false

#### 4. UpdateUserAsync Tests (2 tests)

**UpdateUserAsync_WithValidUser_UpdatesTimestampAndSaves**
- **Purpose:** Verify UpdatedAt timestamp is refreshed
- **Arrange:** Creates existing user with old timestamps
- **Act:** Calls UpdateUserAsync
- **Assert:** Verifies UpdatedAt is updated

**UpdateUserAsync_UpdatesOnlyUpdatedAtTimestamp**
- **Purpose:** Verify CreatedAt remains unchanged
- **Arrange:** Creates user with CreatedAt 5 days ago
- **Act:** Calls UpdateUserAsync
- **Assert:** Verifies CreatedAt unchanged, UpdatedAt updated

#### 5. DeleteUserAsync Tests (2 tests)

**DeleteUserAsync_WithExistingId_ReturnsTrue**
- **Purpose:** Verify successful deletion
- **Arrange:** Mocks repository to return true
- **Act:** Calls DeleteUserAsync(1)
- **Assert:** Verifies true result and repository call

**DeleteUserAsync_WithNonExistingId_ReturnsFalse**
- **Purpose:** Verify deletion failure for missing user
- **Arrange:** Mocks repository to return false
- **Act:** Calls DeleteUserAsync(999)
- **Assert:** Verifies false result

## Test Patterns and Best Practices

### 1. AAA Pattern (Arrange-Act-Assert)
All tests follow the AAA pattern:
- **Arrange:** Set up test data and mock behavior
- **Act:** Execute the method under test
- **Assert:** Verify the expected outcome

### 2. Test Naming Convention
Format: `MethodName_Scenario_ExpectedBehavior`
- Clear and descriptive
- Easy to understand what is being tested
- Examples:
  - `GetUserByIdAsync_WithExistingId_ReturnsUser`
  - `CreateUserAsync_SetsIsDeletedToFalse`

### 3. Test Isolation
- Each test is independent
- Fresh mocks created in `[TestInitialize]`
- No shared state between tests
- Tests can run in any order

### 4. Mock Verification
- Use `Verify()` to ensure repository methods are called
- Use `Times.Once` to verify call count
- Use `It.Is<T>()` for complex parameter matching

### 5. Meaningful Assertions
- Test multiple aspects of the result
- Verify both data and behavior
- Check timestamps, flags, and IDs

## Running Tests

### Visual Studio
1. Open Test Explorer (Test → Test Explorer)
2. Click "Run All" to execute all tests
3. View results in Test Explorer

### Command Line
```bash
# Run all tests in the project
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run tests and collect code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### JetBrains Rider
1. Right-click on project or test class
2. Select "Run Unit Tests"
3. View results in Unit Tests window

## Code Coverage Goals

### Current Coverage:
- **UserService:** 100% method coverage
  - GetAllUsersAsync: ✅ Covered
  - GetUserByIdAsync: ✅ Covered
  - CreateUserAsync: ✅ Covered
  - UpdateUserAsync: ✅ Covered
  - DeleteUserAsync: ✅ Covered

### Target Coverage:
- Maintain **minimum 80%** code coverage
- Aim for **100%** on critical business logic
- Cover all happy paths and edge cases

## Test Results

Latest Test Run:
- **Total Tests:** 10
- **Passed:** 10 ✅
- **Failed:** 0
- **Skipped:** 0
- **Duration:** ~2.3 seconds

## Dependencies

**NuGet Packages:**
- `Microsoft.NET.Test.Sdk` (v17.12.0) - Test platform
- `MSTest` (v3.6.4) - Testing framework
- `Moq` (v4.20.72) - Mocking library

**Project References:**
- `CodeRocket.Services` - Service layer being tested
- `CodeRocket.DataAccess` (transitive) - For interfaces
- `CodeRocket.Common` (transitive) - For models and DTOs

## Adding New Tests

### Template for New Test Class:
```csharp
using Moq;
using CodeRocket.Services.Interfaces;
using CodeRocket.DataAccess.Interfaces;

namespace CodeRocket.Services.UnitTests;

[TestClass]
public sealed class YourServiceTests
{
    private Mock<IYourRepository> _mockRepository = null!;
    private YourService _yourService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IYourRepository>();
        _yourService = new YourService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        // ... setup test data and mocks

        // Act
        var result = await _yourService.MethodName();

        // Assert
        Assert.IsNotNull(result);
        _mockRepository.Verify(r => r.Method(), Times.Once);
    }
}
```

### Checklist for New Tests:
- ✅ Use AAA pattern
- ✅ Follow naming convention
- ✅ Test happy path
- ✅ Test edge cases (null, empty, invalid data)
- ✅ Verify mock calls
- ✅ Add XML comments if complex
- ✅ Ensure test isolation
- ✅ Run and verify test passes

## Common Moq Patterns

### Setup Return Value:
```csharp
_mockRepository.Setup(r => r.GetByIdAsync(1))
    .ReturnsAsync(new User { Id = 1 });
```

### Setup with Any Parameter:
```csharp
_mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
    .ReturnsAsync((User u) => u);
```

### Setup with Specific Condition:
```csharp
_mockRepository.Setup(r => r.CreateAsync(It.Is<User>(u => u.Email == "test@example.com")))
    .ReturnsAsync(new User { Id = 1 });
```

### Verify Method Called:
```csharp
_mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
```

### Verify with Parameter Matching:
```csharp
_mockRepository.Verify(r => r.CreateAsync(
    It.Is<User>(u => u.IsDeleted == false)), Times.Once);
```

## Testing Anti-Patterns to Avoid

### ❌ DON'T:
- Test implementation details
- Create tests that depend on execution order
- Use Thread.Sleep for timing issues
- Mock concrete classes (mock interfaces only)
- Write tests that require external resources (DB, APIs)
- Ignore test failures
- Write tests without assertions

### ✅ DO:
- Test public behavior and contracts
- Ensure test independence
- Use async/await properly
- Mock interfaces and dependencies
- Keep tests fast and isolated
- Fix failures immediately
- Assert expected outcomes clearly

## Future Test Scenarios

### Planned Test Coverage:
1. **Validation Tests**
   - Test email format validation
   - Test required field validation
   - Test business rule violations

2. **Error Handling Tests**
   - Test exception scenarios
   - Test null parameter handling
   - Test repository failures

3. **Edge Cases**
   - Test pagination edge cases (page 0, negative page)
   - Test large datasets
   - Test concurrent updates

4. **Integration with Other Services**
   - Test service orchestration
   - Test transaction handling
   - Test cross-service operations

## Continuous Integration

Tests should run automatically:
- On every commit (pre-commit hooks)
- On pull request creation
- Before deployment
- On scheduled builds (nightly)

**CI/CD Integration:**
```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --no-build --verbosity normal
  
- name: Publish Test Results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: TestResults/
```

## Notes for AI Agents

When writing or modifying tests:
1. **Always mock data repositories** - never use real repository implementations
2. **Test one thing per test** - keep tests focused and simple
3. **Use descriptive names** - follow the MethodName_Scenario_ExpectedBehavior pattern
4. **Verify mock calls** - ensure methods are called with correct parameters
5. **Test edge cases** - null, empty, invalid inputs
6. **Keep tests fast** - no Thread.Sleep or external dependencies
7. **Use [TestInitialize]** - set up fresh state for each test
8. **Follow AAA pattern** - Arrange, Act, Assert
9. **Assert multiple things** - verify data, behavior, and side effects
10. **Keep tests maintainable** - avoid complex setup or hard-to-read assertions

## Troubleshooting

### Tests Fail Intermittently
- Check for shared state between tests
- Verify test isolation
- Look for timing issues with DateTime

### Mock Not Working as Expected
- Verify Setup matches actual call signature
- Check parameter matching (It.IsAny vs specific values)
- Ensure mock is passed to service constructor

### Compilation Errors
- Check using statements
- Verify NuGet packages are restored
- Ensure project references are correct

### Tests Run Slowly
- Remove Thread.Sleep calls
- Mock all external dependencies
- Check for database or network calls
