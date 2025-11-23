# AGENTS.md

## 1. Overview
The application follows a **layered architecture**, separating concerns across multiple projects:

- **Common** – data models, DTOs, shared types  
- **DataAccess** – database access layer using ADO.NET + Dapper  
- **Services** – business logic and validation layer  
- **Api** – REST API project with Swagger documentation  
- **DbMigrator** – console application for database initialization and migrations  

---

## 2. Database

### 2.1 Platform
- **MariaDB 11.8 LTS** or later (with **VECTOR** data type support)  
- **Engine:** `InnoDB`

### 2.2 Deployment
Database migrations are handled via the **DbMigrator** console app.  
Each migration is stored in a separate versioned folder:

```
/migrations
 ├── /1.0.0
 │    ├── create_schema.sql
 │    └── seed_data.sql
 ├── /1.0.1
 │    └── alter_tasks_add_index.sql
 └── /1.1.0
      └── create_vector_embeddings.sql
```

Migrations are executed in order of version number, and the latest applied version is recorded in the `__migrations` table.

To run migrations:
```bash
dotnet run --project DbMigrator
```

---

## 3. Data Access Layer (DataAccess)

### 3.1 Technologies
- **ADO.NET** – for connection and transaction control  
- **Dapper** – lightweight ORM for object mapping  

### 3.2 Repository Example

```csharp
using System.Data;
using MySqlConnector;
using Dapper;
using Common.Models;

namespace DataAccess.Repositories;

public class TaskRepository
{
    private readonly string _connectionString;
    private IDbConnection Connection => new MySqlConnection(_connectionString);

    public TaskRepository(string connectionString) => _connectionString = connectionString;

    public IEnumerable<Task> GetAll() =>
        Connection.Query<Task>("SELECT id, description, completed FROM tasks");

    public void Add(Task task) =>
        Connection.Execute("INSERT INTO tasks (description, completed) VALUES (@Description, @Completed)", task);

    public void Update(Task task) =>
        Connection.Execute("UPDATE tasks SET description=@Description, completed=@Completed WHERE id=@Id", task);

    public void Delete(int id) =>
        Connection.Execute("DELETE FROM tasks WHERE id=@Id", new { Id = id });
}
```

---

## 4. Common (Models)

The **Common** project contains data models, DTOs, and shared contracts used across all layers.

```csharp
namespace Common.Models;

public class Task
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public bool Completed { get; set; }
}
```

---

## 5. Services Layer

Implements business logic, validation, and data consistency rules.

```csharp
using Common.Models;
using DataAccess.Repositories;

namespace Services;

public class TaskService
{
    private readonly TaskRepository _repository;

    public TaskService(TaskRepository repository) => _repository = repository;

    public IEnumerable<Task> GetAll() => _repository.GetAll();

    public void Add(Task task)
    {
        if (string.IsNullOrWhiteSpace(task.Description))
            throw new ArgumentException("Task description cannot be empty");
        _repository.Add(task);
    }

    public void MarkComplete(int id)
    {
        var task = _repository.GetAll().First(t => t.Id == id);
        task.Completed = true;
        _repository.Update(task);
    }
}
```

---

## 6. REST API

The **Api** project provides a REST interface via **ASP.NET Core Minimal API**, with automatic documentation using **Swagger / Swashbuckle**.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TaskRepository>();
builder.Services.AddSingleton<TaskService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/tasks", (TaskService service) => service.GetAll());
app.MapPost("/tasks", (TaskService service, Task task) => { service.Add(task); return Results.Ok(); });

app.Run();
```

Swagger endpoint:  
➡️ `http://localhost:8080/swagger/index.html`

---

## 7. Docker Compose

### 7.1 Structure
`docker-compose.yml` defines two main services:
- **api** – .NET 8/9 REST API  
- **mariadb** – MariaDB 11.8 LTS instance  

### 7.2 Example
```yaml
version: '3.9'
services:
  mariadb:
    image: mariadb:11.8
    container_name: mariadb
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: todo
      MYSQL_USER: app_user
      MYSQL_PASSWORD: Password123!
    volumes:
      - ./data/mariadb:/var/lib/mysql
    ports:
      - "3306:3306"

  api:
    build: ./Api
    container_name: todo_api
    depends_on:
      - mariadb
    environment:
      - ConnectionStrings__Default=server=mariadb;port=3306;user=app_user;password=Password123!;database=todo
    ports:
      - "8080:8080"
```

> 💾 **MariaDB data** is stored outside the container (`./data/mariadb`) for persistence.

---

## 8. Key Benefits
- Transparent versioned migrations  
- Minimal dependencies (Dapper + MySqlConnector)  
- Highly testable isolated layers  
- Full containerization support (Docker Compose)  
- Ready for AI extensions using VECTOR search and Semantic Kernel  

---

## 9. AI / RAG Architecture (Semantic Kernel Integration)

### 9.1 Purpose
The **RAG (Retrieval-Augmented Generation)** layer enables:
- integration of intelligent agents into business logic;  
- access to private data as part of the model’s context;  
- semantic search and contextual Q&A over internal content.

---

### 9.2 High-Level Architecture

```
+----------------------------------------------------------+
|                   REST API (ASP.NET)                     |
|      + Swagger / ChatController / AgentController        |
+--------------------------▲-------------------------------+
                           │
                           ▼
+----------------------------------------------------------+
|                    Services (Business Layer)             |
|  ├─ Core Services (TaskService, etc.)                    |
|  ├─ AgentService ← integrates Semantic Kernel             |
|  └─ Validation / DTO mapping                             |
+--------------------------▲-------------------------------+
                           │
                           ▼
+----------------------------------------------------------+
|                 DataAccess (Dapper + ADO.NET)            |
|  └─ Repositories → MariaDB (VECTOR + InnoDB)             |
+--------------------------▲-------------------------------+
                           │
                           ▼
+----------------------------------------------------------+
|                   Common (Models, DTOs)                  |
+----------------------------------------------------------+
                           │
                           ▼
+----------------------------------------------------------+
|          AI Layer (Semantic Kernel + RAG Components)     |
|  ├─ Kernel (LLM connectors, prompts, plugins)            |
|  ├─ Memory (MariaDB VECTOR / external vector store)      |
|  ├─ Planner / Orchestration                             |
|  └─ Embedding Service                                   |
+----------------------------------------------------------+
```

---

### 9.3 Core Components

#### 1. Kernel Initialization
```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IKernel>(sp =>
{
    var kernel = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion(
            modelId: "gpt-4o-mini",
            apiKey: builder.Configuration["OpenAI:ApiKey"])
        .Build();
    return kernel;
});
```

#### 2. Vector Storage (MariaDB VECTOR)
MariaDB 11.8 supports vector data types:
```sql
CREATE TABLE document_embeddings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    content TEXT,
    embedding VECTOR(1536) NOT NULL,
    metadata JSON
) ENGINE=InnoDB;
```
Custom `IMemoryStore` can use Dapper to interact with this table.

#### 3. Embedding Service
```csharp
using Microsoft.SemanticKernel.Embeddings;

public class EmbeddingService
{
    private readonly ITextEmbeddingGenerationService _embeddingGen;
    private readonly EmbeddingRepository _repo;

    public EmbeddingService(ITextEmbeddingGenerationService embeddingGen, EmbeddingRepository repo)
    {
        _embeddingGen = embeddingGen;
        _repo = repo;
    }

    public async Task StoreAsync(string text, object meta)
    {
        var vector = await _embeddingGen.GenerateEmbeddingAsync(text);
        await _repo.InsertAsync(vector, text, meta);
    }
}
```

#### 4. Agent Service
Handles RAG flow:
1. Retrieve context documents via vector search;  
2. Compose prompt with contextual info;  
3. Invoke LLM through Semantic Kernel.

```csharp
public class AgentService
{
    private readonly IKernel _kernel;
    private readonly EmbeddingRepository _repo;

    public AgentService(IKernel kernel, EmbeddingRepository repo)
    {
        _kernel = kernel;
        _repo = repo;
    }

    public async Task<string> AskAsync(string question)
    {
        var context = await _repo.SearchSimilarAsync(question);
        var input = $"Context: {context}\n\nQuestion: {question}";
        return await _kernel.InvokePromptAsync("default", input);
    }
}
```

---

### 9.4 REST API Endpoint

```csharp
app.MapPost("/ask", async (AgentService agent, [FromBody] string question) =>
{
    var answer = await agent.AskAsync(question);
    return Results.Ok(new { answer });
});
```

Swagger route:  
➡️ `http://localhost:8080/swagger/index.html#/Agent/ask`

---

### 9.5 Containerization
```yaml
services:
  agent:
    build: ./AgentService
    environment:
      - OpenAI__ApiKey=${OPENAI_API_KEY}
      - ConnectionStrings__Default=server=mariadb;user=app_user;password=Password123!;database=ai_store
    depends_on:
      - mariadb
    ports:
      - "8081:8080"
```

---

### 9.6 Integration Possibilities
- Expose **Semantic Kernel plugins** or **MCP Server tools** for developer-facing AI capabilities.  
- Enable **vector search** directly inside MariaDB without external vector DBs.  
- Extend to hybrid solutions (Azure AI Search, Qdrant, Milvus, etc.) through `IMemoryStore` abstraction.

---

### 9.7 Example RAG Flow
1. User asks a question via API/UI  
2. `AgentService` searches vector embeddings in MariaDB  
3. Semantic Kernel composes contextual prompt  
4. LLM generates the answer  
5. Answer and embeddings are optionally stored for reuse  

---

### 9.8 Implementation Steps
To integrate the RAG agent:
- Add a new project `AgentService`  
- Register Semantic Kernel in DI  
- Extend Docker Compose with an `agent` container  
- Add `/ask` endpoint to Swagger  
