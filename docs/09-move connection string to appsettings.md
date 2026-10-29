# Doc 09 — Move Connection String to `appsettings.json`

**Project:** StudentCourseEFCore  
**Framework:** .NET 10 Console Application  
**Database:** SQL Server Express

## 1. Objective

Previously, the SQL Server connection string was hardcoded in `Program.cs` and/or `AppDbContext.cs`.

In this phase, we move the connection string into `appsettings.json` and configure EF Core using Dependency Injection (DI).

### New Configuration Flow

```text
appsettings.json
       ↓
Host Configuration
       ↓
Program.cs
       ↓
AddDbContext<AppDbContext>()
       ↓
Dependency Injection
       ↓
AppDbContext
       ↓
SQL Server
```

## 2. Create `appsettings.json`

Create `appsettings.json` in the project root, alongside `Program.cs` and the `.csproj` file.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=StudentCourseEFCoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Important:** Use the SQL Server instance and database name configured for your existing project.

### Configure File Properties

In Visual Studio, select `appsettings.json` and open Properties.

Set:

- **Build Action:** Content
- **Copy to Output Directory:** Copy if newer

This ensures that `appsettings.json` is available when the application runs.

## 3. Install the Configuration Package

Open the terminal in your project directory and execute:

```powershell
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.14
```

The project already uses `Microsoft.Extensions.Hosting`.

Therefore, `Host.CreateApplicationBuilder(args)` automatically loads `appsettings.json` from the application's content root.

We do not need to create another `ConfigurationBuilder`.

## 4. Update `Program.cs`

### Previous Implementation

The connection string was hardcoded directly in the database registration:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        "Server=.\\SQLEXPRESS;Database=StudentCourseEFCoreDB;Trusted_Connection=True;TrustServerCertificate=True;"));
```

### Updated Implementation

Replace the hardcoded connection string with:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Understanding the Code

- `builder.Configuration` accesses the application's configuration.
- `GetConnectionString("DefaultConnection")` retrieves the connection string associated with that name.
- `AddDbContext<AppDbContext>()` registers the EF Core context with Dependency Injection.
- `UseSqlServer()` configures EF Core to use SQL Server.

The remaining repository, service, and menu registrations can stay unchanged.

## 5. Verify the Connection String

Temporarily add this code immediately after creating the builder:

```csharp
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine(
    $"Connection string loaded: {!string.IsNullOrWhiteSpace(connectionString)}");
```

Expected output:

```text
Connection string loaded: True
```

After verification, remove this diagnostic code.

**Never print the actual connection string in application logs**, because connection strings can contain sensitive information.

### If the Output Is `False`

Check the following:

1. The file is named exactly `appsettings.json`.
2. The file is located in the project root.
3. The JSON syntax is valid.
4. The configuration key is spelled correctly.
5. The file is available in the application's output directory.

## 6. Update `AppDbContext.cs`

### Keep This Constructor

```csharp
public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
{
}
```

This constructor receives the database configuration supplied by Dependency Injection.

### Remove the Parameterless Constructor

Remove:

```csharp
public AppDbContext()
{
}
```

Without this constructor, code that attempts to create an unconfigured context using `new AppDbContext()` will fail to compile.

This helps identify repositories that have not yet been updated to use Dependency Injection.

### Remove the Old `OnConfiguring()` Method

If `OnConfiguring()` was only used to configure the hardcoded SQL Server connection string, remove it.

**Do not remove `OnModelCreating()` or the existing Fluent API configurations.**

Preserve all entity mappings, relationships, indexes, check constraints, default values, and delete behaviors.

## 7. Update Repository Classes

Search the entire project for:

```csharp
new AppDbContext()
```

Repositories should receive `AppDbContext` through constructor injection rather than creating their own context instances.

### Example: `StudentRepository.cs`

```csharp
private readonly AppDbContext _context;

public StudentRepository(AppDbContext context)
{
    _context = context;
}
```

Use `_context` inside repository methods.

For example:

```csharp
public async Task<List<Student>> GetAllAsync()
{
    return await _context.Students
        .AsNoTracking()
        .OrderBy(s => s.StudentId)
        .ToListAsync();
}
```

The injected context receives its configuration from the registration in `Program.cs`.

Apply the same pattern to:

- `DepartmentRepository`
- `StudentRepository`
- `CourseRepository`
- `EnrollmentRepository`

## 8. Build and Run the Application

Open the terminal in your project directory.

Execute:

```powershell
dotnet clean
dotnet build
dotnet run
```

Verify that:

- The project builds successfully.
- The console application starts.
- The connection string is loaded successfully.
- Department operations work.
- Student operations work.
- Course operations work.
- Enrollment operations work.

Existing migrations and Fluent API configurations should remain intact.

## 9. Common Errors and Solutions

### Error 1: Connection String Loaded as `False`

**Cause:** The configuration key was not found.

**Solution:** Check the JSON file location, filename, syntax, configuration key, and output-directory settings.

### Error 2: No Database Provider Has Been Configured

**Cause:** A context instance was created without the options configured by Dependency Injection.

**Solution:** Search for `new AppDbContext()` and update the relevant repository to use constructor injection.

### Error 3: The ConnectionString Property Has Not Been Initialized

**Cause:** A database operation reached a connection that did not have a valid connection string.

**Solution:** Verify that the connection string is loaded correctly and that the repository uses the injected `AppDbContext`.

## 10. Key Concepts Learned

### Configuration

Configuration values should be stored separately from application logic.

### `appsettings.json`

Stores application configuration, including named connection strings.

### Dependency Injection

Provides configured dependencies to classes that need them.

### `AddDbContext<AppDbContext>()`

Registers the EF Core context and its configuration with the dependency injection container.

### Constructor Injection

Allows repositories to receive an already-configured `AppDbContext`.

### `OnModelCreating()`

Defines how entities map to the database. It is separate from connection string configuration.

## 11. Completion Checklist

- [ ] Created `appsettings.json`.
- [ ] Added the connection string under `ConnectionStrings`.
- [ ] Installed the JSON configuration package.
- [ ] Updated `Program.cs` to read the connection string from configuration.
- [ ] Removed the parameterless `AppDbContext` constructor.
- [ ] Removed the hardcoded connection configuration from `OnConfiguring()`.
- [ ] Updated repositories to use constructor injection.
- [ ] Confirmed that the connection string loads successfully.
- [ ] Built and ran the application successfully.
- [ ] Tested the existing CRUD operations.

## Final Outcome

The console application now reads its SQL Server connection string from `appsettings.json` and configures EF Core through Dependency Injection.

The existing menu-based workflow, services, repositories, entity models, Fluent API configuration, and database schema remain in place.

**Next Phase:** Continue improving the existing EF Core application before introducing the ASP.NET Core Web API layer.