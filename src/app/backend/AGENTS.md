# CodeRocket Backend

## Documentation Language Policy

**IMPORTANT**: All documentation in this repository MUST be written in English. This includes code comments, XML documentation, README files, technical documentation, API specifications, and .md files.

## Technology Stack

### Core Technologies
- **.NET 9.0** - Modern cross-platform framework
- **ASP.NET Core** - Web API framework with built-in dependency injection
- **C# 12** - Latest language features and performance improvements

### Database & Data Access
- **MariaDB 11.8 LTS** - Primary database server
- **Dapper** - Lightweight ORM for high-performance data access
- **MySQL Connector/NET** - Database connectivity

### Infrastructure & DevOps
- **Docker & Docker Compose** - Containerization and orchestration
- **Health Checks** - Built-in application and database health monitoring

## Application Architecture

The backend project structure is organized into multiple layers:

```
CodeRocket.sln
├── CodeRocket.Api/           # Web API layer - REST endpoints and controllers
├── CodeRocket.Services/      # Business logic layer - application services
├── CodeRocket.DataAccess/    # Data access layer - repositories and database operations
├── CodeRocket.Common/        # Shared library - models, DTOs, helpers, extensions
├── CodeRocket.Bots/          # Bot integrations - Telegram, Discord bots
└── CodeRocket.DbTools/       # Database tools - migrations and schema management
```

### Project Responsibilities

#### CodeRocket.Api
REST API layer providing HTTP endpoints for client applications.
- Controllers for handling HTTP requests/responses
- Middleware configuration and request pipeline
- API documentation and validation
- **→ [Detailed Documentation](CodeRocket.Api/AGENTS.md)**

#### CodeRocket.Services
Business logic layer containing application services and domain logic.
- Service implementations for business operations
- Business rule validation and processing
- Integration between API and data layers
- **→ [Detailed Documentation](CodeRocket.Services/AGENTS.md)**

#### CodeRocket.DataAccess
Data access layer implementing repository pattern with Dapper ORM.
- Repository interfaces and implementations
- Database connection management
- SQL query organization and execution
- **→ [Detailed Documentation](CodeRocket.DataAccess/AGENTS.md)**

#### CodeRocket.Common
Shared library containing common components used across all layers.
- Base models, DTOs, and interfaces
- Helper functions and extension methods
- Constants, enums, and validation utilities
- **→ [Detailed Documentation](CodeRocket.Common/AGENTS.md)**

#### CodeRocket.Bots
Bot integration layer for external platforms.
- Telegram bot implementation
- Discord bot integration
- Shared bot utilities and handlers
- **→ [Detailed Documentation](CodeRocket.Bots/AGENTS.md)**

#### CodeRocket.DbTools
Database management tools and utilities.
- Database migrations and schema updates
- Data seeding and maintenance scripts
- Database version management
- **→ [Detailed Documentation](CodeRocket.DbTools/AGENTS.md)**

## Database Schema

The application uses **MariaDB 11.8 LTS** as the primary database with the following key characteristics:

- **UTF8MB4** character set for full Unicode support
- **Soft delete pattern** for data integrity
- **Audit trails** with CreatedAt/UpdatedAt timestamps
- **Indexed foreign keys** for optimal query performance
- **Connection pooling** for scalability

> **Note**: Detailed schema documentation is available in [CodeRocket.DataAccess/AGENTS.md](CodeRocket.DataAccess/AGENTS.md)

## Quick Start with Docker

### Prerequisites
- Docker Desktop or Docker Engine
- Docker Compose v3.9+

### Launch Application

1. **Navigate to backend directory:**
   ```bash
   cd backend
   ```

2. **Start all services:**
   ```bash
   docker compose up -d --build
   ```

3. **Verify services are running:**
   ```bash
   docker compose ps
   ```

### Service Endpoints

After successful startup, the following services will be available:

- **API Server**: http://localhost:8000 (HTTP) / https://localhost:8001 (HTTPS)
- **MariaDB**: localhost:3306 (accessible only from localhost for security)
- **Health Check**: http://localhost:8000/health

### Database Connection

The application connects to MariaDB using these default development credentials:
- **Host**: mariadb (internal Docker network)
- **Database**: coderocket
- **User**: coderocket_user
- **Password**: coderocket_pass

> **⚠️ Security Note**: Change default passwords before production deployment. Use Docker secrets or environment-specific configuration for production.

### Useful Docker Commands

```bash
# View application logs
docker compose logs -f coderocket.api

# View database logs
docker compose logs -f mariadb

# Stop all services
docker compose down

# Stop and remove volumes (⚠️ data loss)
docker compose down -v

# Rebuild API container
docker compose build coderocket.api

# Connect to MariaDB CLI
docker compose exec mariadb mysql -u coderocket_user -p coderocket
```

### Data Persistence

Database data is stored in a Docker volume named `mariadb_data` which persists between container restarts:

```bash
# List volumes
docker volume ls

# Inspect volume location
docker volume inspect backend_mariadb_data
```

## Development Workflow

### Local Development Setup

1. **Clone repository and navigate to backend:**
   ```bash
   git clone <repository-url>
   cd backend
   ```

2. **Start infrastructure (database only):**
   ```bash
   docker compose up -d mariadb
   ```

3. **Run API locally** using your IDE (Rider/Visual Studio) or CLI:
   ```bash
   dotnet run --project CodeRocket.Api
   ```

### Database Migrations

Database schema changes are managed through `CodeRocket.DbTools`:

```bash
# Run migrations
dotnet run --project CodeRocket.DbTools

# Create new migration
dotnet run --project CodeRocket.DbTools -- create-migration "MigrationName"
```

### Health Monitoring

The application includes built-in health checks:

- **Application Health**: `GET /health`
- **Database Health**: Automatic dependency monitoring
- **Ready/Live Probes**: Available for Kubernetes deployments

## Configuration

### Environment Variables

Key configuration options via environment variables:

```bash
# Database connection
ConnectionStrings__DbConnection="Server=localhost;Database=coderocket;..."

# ASP.NET Core environment
ASPNETCORE_ENVIRONMENT=Development|Staging|Production

# Logging level
Logging__LogLevel__Default=Information
```

### Configuration Files

- `appsettings.json` - Base application configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test CodeRocket.Services.Tests

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

- **Unit Tests** - Individual component testing
- **Integration Tests** - Database and API endpoint testing
- **Health Check Tests** - Infrastructure monitoring validation

## Troubleshooting

### Common Issues

**Port Conflicts:**
```bash
# Check if ports 8000/8001/3306 are in use
netstat -tulpn | grep :8000
```

**Database Connection Issues:**
```bash
# Verify MariaDB is running and accessible
docker compose exec mariadb mysqladmin ping
```

**Container Build Failures:**
```bash
# Clean Docker cache and rebuild
docker system prune -a
docker compose build --no-cache
```

### Log Analysis

```bash
# Follow all service logs
docker compose logs -f

# API-specific logs with timestamps
docker compose logs -f -t coderocket.api

# Filter logs by severity
docker compose logs | grep ERROR
```

## Production Deployment

### Security Checklist

- [ ] Change default database passwords
- [ ] Use Docker secrets for sensitive data
- [ ] Configure HTTPS certificates
- [ ] Set up proper firewall rules
- [ ] Enable audit logging
- [ ] Configure backup strategies

### Performance Optimization

- [ ] Configure connection pooling limits
- [ ] Set up database indexing strategy
- [ ] Implement caching layers
- [ ] Configure load balancing
- [ ] Set resource limits in Docker

### Monitoring

- [ ] Application Performance Monitoring (APM)
- [ ] Database performance monitoring
- [ ] Log aggregation and analysis
- [ ] Health check alerting
- [ ] Resource utilization tracking

## Contributing

When working on the backend:

1. **Follow Documentation Standards**: All documentation must be in English
2. **Update AGENTS.md Files**: Keep project-specific documentation current
3. **Test Docker Setup**: Verify `docker compose up` works after changes
4. **Database Changes**: Use proper migrations through `CodeRocket.DbTools`
5. **API Changes**: Update OpenAPI documentation and health checks

For detailed contribution guidelines, see individual project AGENTS.md files linked above.

---

**Need Help?** Check the detailed documentation for each project component or contact the development team.
