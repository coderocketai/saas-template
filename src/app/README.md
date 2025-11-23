# CodeRocket SaaS Application Template

## Technology Stack
- **Database:** PostgreSQL 18 + Pgvector
- **Backend:** .NET 9, C#, Dapper ORM, REST API, Telegram.Bot ([NuGet](https://www.nuget.org/packages/Telegram.Bot))
- **Frontend:** React 18, Vite, TypeScript, shadcn/ui

## Project Sctructure

```shell
app/
├── backend/
│   ├── CodeRocket.Api/           # REST API Application
│   ├── CodeRocket.Bot/           # Telegram Bot Application
│   ├── CodeRocket.Bots/          # Bots integrations
│   ├── CodeRocket.Common/        # Shared models, DTOs, helpers
│   ├── CodeRocket.DataAccess/    # Data access layer (Dapper)
│   ├── CodeRocket.DbTools/       # Database migration tools
│   ├── CodeRocket.Services/      # Business logic
│   ├── tests/                    # Unit and integration tests
│   ├── .editorconfig             # Code style configuration
│   ├── CodeRocket.sln            # Solution file
│   ├── docker-compose.yaml       # Backend Docker Compose
│   └── AGENTS.md                 # Backend documentation
├── frontend/
└── README.md                     # Project root documentation
```