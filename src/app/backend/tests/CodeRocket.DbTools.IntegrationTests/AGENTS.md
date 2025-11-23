# CodeRocket.DbTools.IntegrationTests

## Overview
This project contains integration tests for the CodeRocket.DbTools module, which handles database migrations, setup, and data operations for the CodeRocket application.

## Test Architecture

### Docker-based Testing
The integration tests use Docker containers to provide isolated database environments for each test execution:

- **MariaDB Container**: Tests run against a real MariaDB 11.8.3 instance in Docker
- **Isolated Environment**: Each test gets a fresh database container
- **Automatic Cleanup**: Containers are automatically created and destroyed

### Test Structure

#### Basic Tests (`DbToolsBasicTests.cs`)
Core functionality tests covering:
- Database setup and initialization
- Migration execution and tracking
- Version management
- Connection testing
- Idempotent operations (repeated setup calls)

#### Advanced Tests (`DbToolsAdvancedTests.cs`)
Extended functionality tests covering:
- Database schema validation
- Test data insertion and verification
- Performance testing
- Error handling with invalid connections
- Index creation and optimization
- Character set and collation validation

### Infrastructure Components

#### `DockerMariaDbContainer` Class
Manages Docker container lifecycle for testing:
- **Container Creation**: Automatically pulls and creates MariaDB containers
- **Port Management**: Finds available ports to avoid conflicts
- **Health Checks**: Waits for database readiness before tests
- **Cleanup**: Safely stops and removes containers after tests

## Key Features Tested

### Database Operations
- Database creation and configuration
- UTF8MB4 character set support
- Connection string validation
- Transaction handling

### Migration System
- SQL migration file execution
- Version tracking in `db_versions` table
- Rollback protection
- Migration history preservation

### Performance
- Setup operation timing (< 10 seconds requirement)
- Large dataset handling
- Index performance optimization

### Error Handling
- Invalid connection parameters
- Missing migration files
- Container startup failures
- Network connectivity issues

## Test Data

### Users Table Schema
Tests validate the creation and structure of:
```sql
CREATE TABLE users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    Role INT NOT NULL,
    TelegramId BIGINT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);
```

### Test Users
Integration tests insert sample data including:
- Admin user (`admin@coderocket.com`) with SuperAdmin role (4)
- Regular test users for validation
- Deleted users for soft-delete testing

### Indexes
Validates creation of performance indexes on:
- `email` field for user lookup
- `telegram_id` for Telegram integration
- `role` field for authorization queries

## Running Tests

### Prerequisites
- Docker Desktop installed and running
- .NET 9.0 SDK
- MariaDB Docker image access

### Execution
```bash
# Run all integration tests
dotnet test CodeRocket.DbTools.IntegrationTests

# Run specific test class
dotnet test --filter "FullyQualifiedName~DbToolsBasicTests"

# Run with verbose output
dotnet test CodeRocket.DbTools.IntegrationTests --logger "console;verbosity=detailed"
```

### Test Isolation
- Each test method gets a fresh MariaDB container
- Containers use random ports to avoid conflicts
- Automatic cleanup prevents resource leaks
- Tests can run in parallel safely

## Configuration

### Database Settings
- **Image**: MariaDB 11.8.3
- **Charset**: UTF8MB4 with Unicode collation
- **Root Password**: `test_password_123` (test only)
- **Database Name**: `coderocket_test_db`
- **Connection Timeout**: 60 seconds

### Container Settings
- **Port Range**: Automatically assigned available ports
- **Memory**: Default Docker limits
- **Startup Timeout**: 60 seconds (30 attempts × 2 seconds)
- **Cleanup**: Force removal after tests

## Troubleshooting

### Common Issues

#### Docker Not Available
```
Error: Docker API responded with status code=NotFound
```
**Solution**: Ensure Docker Desktop is running and accessible

#### Port Conflicts
```
Error: Port already in use
```
**Solution**: Tests automatically find available ports, but ensure no manual port assignments conflict

#### Container Startup Timeout
```
TimeoutException: MariaDB container is not ready within the specified time
```
**Solution**: Check Docker resources and network connectivity

#### Image Pull Failures
```
Error: No such image: mariadb:11.8.3
```
**Solution**: Run command `docker pull mariadb:11.8.3` to **pull** MariaDb image locally.

### Debug Information
Tests output container information including:
- Assigned port numbers
- Container IDs
- Connection strings (without passwords)
- Execution timing

## Maintenance

### Updating MariaDB Version
To update the MariaDB version:
1. Update `_mariaDbImageName` in `DockerMariaDbContainer.cs`
2. Test compatibility with existing migrations
3. Update this documentation

### Adding New Tests
When adding new integration tests:
1. Follow the existing naming convention (`Test##_Description`)
2. Use the established setup/cleanup pattern
3. Include appropriate assertions and logging
4. Update this documentation with new test scenarios

### Performance Monitoring
Monitor test execution times:
- Database setup should complete in < 10 seconds
- Individual tests should complete in < 30 seconds
- Full test suite should complete in < 5 minutes

## Security Notes

### Test Environment Only
- Passwords and configurations are for testing only
- Containers are isolated and temporary
- No production data should be used in tests
- All test data is automatically destroyed

### Network Security
- Containers only expose necessary ports
- Connections are limited to localhost
- No external network access required for basic tests
