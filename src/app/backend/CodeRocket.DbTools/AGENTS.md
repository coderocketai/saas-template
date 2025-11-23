# CodeRocket.DbTools - Database Migration Tool

## Overview

**CodeRocket.DbTools** is a console application for managing database migrations with version control. It provides a robust, file-based migration system that tracks database schema changes and ensures consistent deployment across environments.

## Key Features

- **Version-based migrations**: Organized by folders with semantic versioning
- **SQL file execution**: Direct execution of SQL scripts with transaction support
- **Migration tracking**: Maintains version history in `db_versions` table
- **MariaDB support**: Optimized for MariaDB 11.8+ with VECTOR support
- **Environment configuration**: Flexible connection string management
- **Rollback protection**: Prevents accidental re-execution of completed migrations

---

## Architecture

### Project Structure

```
CodeRocket.DbTools/
├── Program.cs              # Main console application entry point
├── appsettings.json        # Configuration (connection strings, settings)
├── Services/
│   ├── DatabaseService.cs  # Core database operations using ADO.NET
│   └── MigrationService.cs # Migration logic and version management
├── Models/
│   └── DatabaseModels.cs   # Migration and version tracking models
└── Migrations/
    └── Initial/             # Migration folder (version-based)
        └── *.sql           # SQL scripts to execute
```

### Core Components

#### 1. **DatabaseService**
- **Purpose**: Low-level database operations using pure ADO.NET
- **Responsibilities**:
  - Database connection management
  - Database creation if not exists
  - SQL script execution with transactions
  - Version tracking table management

```csharp
// Key methods:
Task<bool> EnsureDatabaseExistsAsync()
Task ExecuteSqlFileAsync(string filePath)
Task<List<DatabaseVersion>> GetExecutedVersionsAsync()
Task RecordVersionAsync(string version, string description)
```

#### 2. **MigrationService**
- **Purpose**: High-level migration orchestration
- **Responsibilities**:
  - Migration discovery from file system
  - Version comparison and execution planning
  - Migration execution workflow

```csharp
// Key methods:
List<Migration> GetAvailableMigrations()
Task<List<Migration>> GetPendingMigrationsAsync()
Task ExecuteMigrationAsync(Migration migration)
```

#### 3. **Models**
```csharp
// DatabaseVersion: Represents executed migration record
public class DatabaseVersion
{
    public int Id { get; set; }
    public string Version { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string? Description { get; set; }
}

// Migration: Represents file-system migration
public class Migration
{
    public string Version { get; set; }
    public string FolderPath { get; set; }
    public List<string> SqlFiles { get; set; }
    public string? Description { get; set; }
}
```

---

## Migration System

### 1. **Migration Organization**

Migrations are organized in **version-based folders**:

```
Migrations/
├── Initial/                 # Version: "Initial"
│   ├── 1_create_users_table.sql
│   ├── 2_create_indexes.sql
│   └── description.txt      # Optional migration description
├── 1.0.1/                  # Version: "1.0.1"
│   └── 1_add_user_preferences.sql
└── 1.1.0/                  # Version: "1.1.0"
    ├── 1_create_vector_embeddings.sql
    └── 2_seed_initial_data.sql
```

### 2. **Version Tracking**

The system maintains a `db_versions` table:

```sql
CREATE TABLE db_versions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Version VARCHAR(50) NOT NULL UNIQUE,
    ExecutedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Description VARCHAR(500) NULL
);
```

### 3. **Execution Flow**

1. **Discover Migrations**: Scan `/Migrations` folder for version directories
2. **Check Executed**: Query `db_versions` table for completed migrations
3. **Identify Pending**: Compare available vs executed versions
4. **Execute Sequentially**: Run pending migrations in order
5. **Record Success**: Insert version record after successful execution

---

## Usage

### Running Migrations

```bash
# Navigate to DbTools project
cd backend/CodeRocket.DbTools

# Run migrations
dotnet run

# Or build and run
dotnet build
dotnet run --project CodeRocket.DbTools.csproj
```

### Configuration

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;user=root;password=root;database=coderocket_dev"
  }
}
```

**Environment Variables** (alternative):
```bash
export ConnectionStrings__DefaultConnection="server=mariadb;port=3306;user=app_user;password=Password123!;database=coderocket"
```

### Sample Output

```
CodeRocket Database Tools
========================
✅ Database connection verified
✅ Database 'coderocket_dev' exists
✅ Migration tracking table ready

📋 Available migrations:
  - Initial (3 SQL files)
  - 1.0.1 (1 SQL files)

📋 Executed migrations:
  - Initial (executed: 2023-10-20 10:30:00)

🚀 Pending migrations:
  - 1.0.1

⏳ Executing migration: 1.0.1
  ✅ 1_add_user_preferences.sql
✅ Migration '1.0.1' completed successfully

🎉 All migrations completed!
```

---

## Adding New Migrations

### Step 1: Create Migration Folder
```bash
mkdir Migrations/1.2.0
```

### Step 2: Add SQL Scripts
```sql
-- Migrations/1.2.0/1_create_documents_table.sql
CREATE TABLE documents (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Content TEXT,
    UserId INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES users(Id)
);
```

### Step 3: Add Description (Optional)
```
-- Migrations/1.2.0/description.txt
Add documents table for user content management
```

### Step 4: Run Migrations
```bash
dotnet run --project CodeRocket.DbTools
```

---

## Database Schema

### Core Tables

#### **users** (created by Initial migration)
```sql
CREATE TABLE users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Role INT NOT NULL DEFAULT 0,           -- UserRole enum
    Email VARCHAR(255) NULL UNIQUE,
    TelegramId VARCHAR(50) NULL UNIQUE,
    DiscordId VARCHAR(50) NULL UNIQUE,
    FirstName VARCHAR(100) NULL,
    LastName VARCHAR(100) NULL,
    DisplayName VARCHAR(100) NULL,
    CreatedBy VARCHAR(100) NULL,
    UpdatedBy VARCHAR(100) NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE
);
```

#### **db_versions** (auto-created by DbTools)
```sql
CREATE TABLE db_versions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Version VARCHAR(50) NOT NULL UNIQUE,
    ExecutedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    Description VARCHAR(500) NULL
);
```

---

## Technology Stack

- **.NET 9.0**: Console application framework
- **ADO.NET**: Pure database access (no ORM overhead)
- **MySqlConnector**: MariaDB/MySQL driver
- **Configuration**: Microsoft.Extensions.Configuration
- **File System**: Migration discovery and SQL file reading

---

## Best Practices

### 1. **Migration Naming**
- Use **semantic versioning** for folder names: `1.0.0`, `1.1.0`, `2.0.0`
- Use **sequential numbering** for SQL files: `1_create_table.sql`, `2_add_indexes.sql`
- Use **descriptive names**: `1_create_users_table.sql` vs `users.sql`

### 2. **SQL Best Practices**
- Always use `IF NOT EXISTS` for CREATE statements
- Include proper indexes for performance
- Add comments explaining complex logic
- Use transactions for multi-statement operations

### 3. **Version Control**
- **Never modify executed migrations** in production
- Create new migrations for schema changes
- Include rollback scripts when possible

### 4. **Environment Management**
- Use different connection strings per environment
- Test migrations on staging before production
- Backup database before major migrations

---

## Troubleshooting

### Common Issues

**1. Connection String Not Found**
```
❌ Connection string not found. Check appsettings.json or environment variables.
```
- Verify `appsettings.json` exists with valid connection string
- Check environment variable format: `ConnectionStrings__DefaultConnection`

**2. Database Access Denied**
```
❌ Failed to connect to database: Access denied for user 'app'@'localhost'
```
- Verify database credentials
- Ensure user has CREATE/ALTER/INSERT permissions
- Check MariaDB/MySQL service is running

**3. Migration Already Executed**
```
⚠️ Migration 'Initial' already executed, skipping
```
- This is normal behavior - indicates migration was previously run
- Check `db_versions` table to see execution history

### Development Commands

```bash
# Check current database state
mysql -u root -p -e "SELECT * FROM coderocket_dev.db_versions;"

# Reset migrations (⚠️ DESTRUCTIVE - dev only)
mysql -u root -p -e "DROP DATABASE IF EXISTS coderocket_dev; CREATE DATABASE coderocket_dev;"

# View migration status
dotnet run --project CodeRocket.DbTools -- --status
```

---

## Integration

### Docker Usage

**docker-compose.yaml**:
```yaml
services:
  mariadb:
    image: mariadb:11.8
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: coderocket_dev
    
  db-migrator:
    build: ./CodeRocket.DbTools
    depends_on:
      - mariadb
    environment:
      - ConnectionStrings__DefaultConnection=server=mariadb;port=3306;user=root;password=root;database=coderocket_dev
```

### CI/CD Integration

```yaml
# GitHub Actions example
- name: Run Database Migrations
  run: |
    cd backend/CodeRocket.DbTools
    dotnet run
  env:
    ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION_STRING }}
```

---

## Future Enhancements

- **Rollback support**: Add down-migration scripts
- **Seeding support**: Separate data seeding from schema migrations  
- **Parallel execution**: Support for independent migration branches
- **Validation**: Schema validation and drift detection
- **Reporting**: Migration execution reports and timing
