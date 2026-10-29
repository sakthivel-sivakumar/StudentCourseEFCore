# Doc 10 — Asynchronous EF Core Operations

## 1. Objective

In this phase, we introduce asynchronous database operations into our Student Course Management console application using Entity Framework Core.

We also standardize dependency injection and database connection management so that repositories use the `AppDbContext` provided by the application's dependency injection container.

By completing this phase, students will understand how asynchronous operations flow through the application layers.

## 2. Why Do We Need Asynchronous Operations?

Consider a database query:

```csharp
var students = _context.Students.ToList();
```

This is a synchronous operation. The current thread waits for the database query to complete before continuing.

Now consider:

```csharp
var students = await _context.Students.ToListAsync();
```

This is an asynchronous operation. While waiting for the database response, the thread can perform other work instead of remaining blocked.

Asynchronous operations are particularly useful when an application performs database queries, inserts, updates, or deletes.

**Important:** Asynchronous operations do not automatically make every query faster. Their main benefit is improving how efficiently the application handles waiting for I/O operations.

## 3. Synchronous vs. Asynchronous EF Core Methods

| Synchronous method | Asynchronous method |
|---|---|
| `ToList()` | `ToListAsync()` |
| `FirstOrDefault()` | `FirstOrDefaultAsync()` |
| `SingleOrDefault()` | `SingleOrDefaultAsync()` |
| `Any()` | `AnyAsync()` |
| `Count()` | `CountAsync()` |
| `SaveChanges()` | `SaveChangesAsync()` |
| `Find()` | `FindAsync()` |

The asynchronous versions allow the application to await database I/O instead of blocking the current thread while waiting.

**Note:** Not every EF Core method has a direct asynchronous equivalent. For example, `Where()` builds a query; it does not execute the query against the database by itself.

## 4. Understanding Task and Task<T>

Asynchronous methods commonly return either `Task` or `Task<T>`.

### Task

Use `Task` when an asynchronous method does not need to return a result.

```csharp
public async Task AddStudentAsync(Student student)
{
    _context.Students.Add(student);

    await _context.SaveChangesAsync();
}
```

The method performs an operation but does not return a value to its caller.

### Task<T>

Use `Task<T>` when an asynchronous method returns a result.

```csharp
public async Task<List<Student>> GetAllStudentsAsync()
{
    return await _context.Students.ToListAsync();
}
```

Here, `T` is `List<Student>`.

The caller awaits the operation to obtain the list:

```csharp
List<Student> students =
    await studentService.GetAllStudentsAsync();
```

### Quick Reference

| Return type | Purpose |
|---|---|
| `Task` | Asynchronous operation without a returned value |
| `Task<T>` | Asynchronous operation that returns a value of type `T` |
| `async` | Allows a method to use `await` |
| `await` | Waits asynchronously for an operation to complete and obtains its result when applicable |

An `async` method does not have to return a value directly. For example, an asynchronous method returning `Task<T>` can return a `T` value, as shown above.

## 5. Understanding the Application Flow

Our application follows this flow:

```text
Program.cs
    |
    v
Console Menu
    |
    v
Service Layer
    |
    v
Repository Layer
    |
    v
AppDbContext
    |
    v
SQL Server Database
```

When a user requests a database operation:

1. The menu calls the appropriate service method.
2. The service calls the repository method.
3. The repository performs the EF Core operation.
4. The repository awaits the database operation.
5. The result returns through the service to the menu.

Asynchronous execution should be maintained throughout the call chain.

For example:

```csharp
// Menu
await studentService.AddStudentAsync(student);

// Service
await studentRepository.AddStudentAsync(student);

// Repository
public async Task AddStudentAsync(Student student)
{
    _context.Students.Add(student);

    await _context.SaveChangesAsync();
}
```

These snippets illustrate the flow. Use the actual method names and signatures in the project.

**Key principle:** If a method calls an asynchronous operation and needs its result or completion before continuing, it should normally await that operation.

## 6. Dependency Injection for AppDbContext

Instead of creating a new context manually inside every repository, our application uses dependency injection.

### AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSets and model configuration
}
```

The context receives its configuration through `DbContextOptions<AppDbContext>`.

### Program.cs

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));
```

The connection string is stored in `appsettings.json`, rather than being hardcoded in the context.

### Repository

```csharp
public class StudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }
}
```

The dependency injection container supplies the context when creating the repository.

This provides consistent configuration and avoids manually constructing a context in each repository.

## 7. Important Rules When Using Async EF Core

1. **Use asynchronous database methods:** Prefer methods such as `ToListAsync()` and `SaveChangesAsync()` for database I/O.
2. **Await operations:** Await asynchronous calls when the current method depends on their result or completion.
3. **Propagate asynchronous return types:** Use `Task` or `Task<T>` where appropriate across the menu, service, and repository layers.
4. **Avoid `.Result` and `.Wait()`:** These can block threads and may cause deadlocks in some application environments.
5. **Do not use `Task.Run()` to wrap EF Core database calls:** Use EF Core's asynchronous APIs directly.
6. **Do not run concurrent operations on the same DbContext:** A single context does not support multiple simultaneous operations.
7. **Keep DbContext lifetime controlled:** Use dependency injection and avoid retaining a context beyond its intended lifetime.
8. **Preserve existing business logic:** Introducing asynchronous methods should not change the application's intended behavior.

## 8. Testing Checklist

After implementing this phase, verify the following:

- [ ] The application builds successfully.
- [ ] The connection string loads from `appsettings.json`.
- [ ] Repositories receive `AppDbContext` through constructor injection.
- [ ] Student CRUD operations work correctly.
- [ ] Department and course operations work correctly.
- [ ] Enrollment operations work correctly.
- [ ] Database queries and save operations use asynchronous APIs where appropriate.
- [ ] Asynchronous calls are awaited through the application layers.
- [ ] Validation and error handling continue to work.
- [ ] Existing relationships, constraints, and Fluent API configurations remain unchanged.

## 9. Final Outcome

After completing this phase, the application uses:

- Centralized connection string configuration.
- Dependency injection for `AppDbContext`.
- Constructor injection in repositories.
- Asynchronous EF Core database operations.
- `Task` and `Task<T>` for asynchronous method contracts.
- An asynchronous call chain from the console menu through the service and repository layers.

This prepares the application for more maintainable database access and provides a foundation for asynchronous operations in ASP.NET Core Web API applications.