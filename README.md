# Nuv Tools Data Libraries

[![NuGet](https://img.shields.io/nuget/v/NuvTools.Data.svg)](https://www.nuget.org/packages/NuvTools.Data/)
[![License](https://img.shields.io/github/license/nuvtools/nuvtools-data)](https://github.com/nuvtools/nuvtools-data/blob/main/LICENSE)

Nuv Tools Data Libraries are a set of .NET libraries designed to simplify and standardize data access and manipulation. They provide abstractions and helpers for common data operations, as well as seamless integration with Entity Framework Core for SQL Server and PostgreSQL.

## 🎯 Key Features

- **Multi-Framework Support**: .NET 8, .NET 9, and .NET 10
- **ORM-Agnostic Core**: Base paging and sorting utilities work with any data source
- **EF Core Integration**: First-class support for Entity Framework Core 10.0
- **Result Pattern**: All database operations return `IResult` or `IResult<T>` for consistent error handling
- **Async-First**: All EF Core operations are async with `CancellationToken` support
- **Bulk Operations**: Efficient bulk add, update, and remove operations
- **Well-Documented**: Comprehensive XML documentation for IntelliSense support

## 📦 Libraries Overview

### NuvTools.Data

[![NuGet](https://img.shields.io/nuget/v/NuvTools.Data.svg)](https://www.nuget.org/packages/NuvTools.Data/)

Core library with generic helpers for data manipulation:
- **Paging**: Abstracts paging logic for both `IQueryable` and `IEnumerable` collections
- **Sorting**: Extension methods for sorting with ascending/descending direction
- **Filtering**: Paging filters with optional sorting capabilities
- **No dependencies**: Works with any data source or ORM

```bash
dotnet add package NuvTools.Data
```

### NuvTools.Data.EntityFrameworkCore

[![NuGet](https://img.shields.io/nuget/v/NuvTools.Data.EntityFrameworkCore.svg)](https://www.nuget.org/packages/NuvTools.Data.EntityFrameworkCore/)

Extensions for Entity Framework Core:
- **DbContextBase**: Base class with CRUD operations using Result pattern
- **Transaction Management**: Built-in transaction support with rollback/commit
- **Execution Strategies**: Connection resiliency with automatic retry
- **Bulk Operations**: `SyncFromListAsync`, `AddOrUpdateFromListAsync`, `AddOrRemoveFromListAsync`
- **Async Paging**: `PagingWrapAsync` for efficient database paging

```bash
dotnet add package NuvTools.Data.EntityFrameworkCore
```

### NuvTools.Data.EntityFrameworkCore.SqlServer

[![NuGet](https://img.shields.io/nuget/v/NuvTools.Data.EntityFrameworkCore.SqlServer.svg)](https://www.nuget.org/packages/NuvTools.Data.EntityFrameworkCore.SqlServer/)

Specialized helpers for SQL Server with EF Core:
- **Simple Registration**: `AddDatabase` and `AddDatabaseByConnectionName` extension methods
- **Configuration Integration**: Load connection strings from `appsettings.json`
- **SQL Server Options**: Configure SQL Server-specific options via `SqlServerDbContextOptionsBuilder`

```bash
dotnet add package NuvTools.Data.EntityFrameworkCore.SqlServer
```

### NuvTools.Data.EntityFrameworkCore.PostgreSQL

[![NuGet](https://img.shields.io/nuget/v/NuvTools.Data.EntityFrameworkCore.PostgreSQL.svg)](https://www.nuget.org/packages/NuvTools.Data.EntityFrameworkCore.PostgreSQL/)

Specialized helpers for PostgreSQL with EF Core:
- **Simple Registration**: `AddDatabase` and `AddDatabaseByConnectionName` extension methods
- **Snake Case Convention**: `UseSnakeCaseNamingConvention` for PostgreSQL naming standards
- **Npgsql Options**: Configure PostgreSQL-specific options via `NpgsqlDbContextOptionsBuilder`

```bash
dotnet add package NuvTools.Data.EntityFrameworkCore.PostgreSQL
```

## 🚀 Quick Start

### Basic Paging (NuvTools.Data)

```csharp
using NuvTools.Data.Paging;

// In-memory paging
var items = new List<Product> { /* ... */ };
var pagedResult = items.PagingWrap(pageNumber: 1, pageSize: 20);

Console.WriteLine($"Page {pagedResult.PageNumber} of {Math.Ceiling(pagedResult.Total / 20.0)}");
foreach (var item in pagedResult.List)
{
    Console.WriteLine(item.Name);
}
```

### Sorting (NuvTools.Data)

```csharp
using NuvTools.Data.Sorting;
using NuvTools.Data.Sorting.Enumerations;

var sortedProducts = products
    .Sort(p => p.Category, SortDirection.ASC)
    .ThenSort(p => p.Price, SortDirection.DESC);
```

### SQL Server Registration (NuvTools.Data.EntityFrameworkCore.SqlServer)

```csharp
using NuvTools.Data.EntityFrameworkCore.SqlServer.Extensions;

// In Program.cs or Startup.cs
builder.Services.AddDatabaseByConnectionName<MyDbContext>("DefaultConnection");

// Or with connection string directly
builder.Services.AddDatabase<MyDbContext>(connectionString);

// With SQL Server-specific options
builder.Services.AddDatabaseByConnectionName<MyDbContext>(
    "DefaultConnection",
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
);
```

### PostgreSQL with Snake Case (NuvTools.Data.EntityFrameworkCore.PostgreSQL)

```csharp
using NuvTools.Data.EntityFrameworkCore.PostgreSQL.Extensions;

// Register DbContext
builder.Services.AddDatabaseByConnectionName<MyDbContext>("PostgresConnection");

// In your DbContext
public class MyDbContext : DbContextBase
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply snake_case naming convention for PostgreSQL
        modelBuilder.UseSnakeCaseNamingConvention();
        base.OnModelCreating(modelBuilder);
    }
}
```

### Using DbContextBase

```csharp
using NuvTools.Data.EntityFrameworkCore.Context;

public class MyDbContext : DbContextBase
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}

// In your service
public class ProductService
{
    private readonly MyDbContext _context;

    public async Task<IResult<int>> CreateProductAsync(Product product)
    {
        // AddAndSaveAsync returns IResult<TKey>
        return await _context.AddAndSaveAsync<Product, int>(product);
    }

    public async Task<IResult> UpdateProductAsync(Product product, int id)
    {
        // UpdateAndSaveAsync returns IResult
        return await _context.UpdateAndSaveAsync(product, id);
    }

    public async Task<IResult> DeleteProductAsync(int id)
    {
        // RemoveAndSaveAsync returns IResult
        return await _context.RemoveAndSaveAsync<Product>(id);
    }
}
```

### Async Paging with EF Core

```csharp
using NuvTools.Data.EntityFrameworkCore.Paging;

public async Task<PagingWithEnumerableList<Product>> GetProductsAsync(int pageNumber, int pageSize)
{
    var query = _context.Products.Where(p => p.IsActive);

    // PagingWrapAsync materializes the data
    var pagedResult = await query.PagingWrapWithEnumerableListAsync(pageNumber, pageSize);

    return pagedResult;
}
```

### Bulk Operations

```csharp
// Sync from list: Add new, update existing, remove missing
var result = await _context.SyncFromListAsync(
    updatedProducts,
    p => p.Id,
    p => p.IsActive // Optional filter
);

// Add or update only (no removal)
await _context.AddOrUpdateFromListAsync(products, p => p.Id);

// Add or remove only (no updates)
await _context.AddOrRemoveFromListAsync(products, p => p.Id);
```

## 📋 Requirements

- **.NET SDK**: 8.0, 9.0, or 10.0
- **Entity Framework Core**: 10.0.2 (for EF Core libraries)
- **Database Providers**:
  - SQL Server: Microsoft.EntityFrameworkCore.SqlServer 10.0.2
  - PostgreSQL: Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0

## 🔧 Development

### Building the Solution

```bash
dotnet build NuvTools.Data.slnx
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/NuvTools.Data.Test/NuvTools.Data.Test.csproj
```

### Creating NuGet Packages

```bash
dotnet build NuvTools.Data.slnx -c Release
```

Packages are automatically generated in `bin/Release` folders when building in Release configuration.

## 📝 Version History

### Version 10.0.2
- Updated to Entity Framework Core 10.0.2
- Updated all Microsoft packages to version 10.0.2
- Improved package descriptions and tags
- Copyright updated to 2026

### Version 10.0.0
- Added .NET 10 support
- Updated to Entity Framework Core 10.0.0
- Updated all Microsoft packages to version 10.0.0
- Comprehensive XML documentation added to all public APIs
- Migrated solution to .slnx format

## 📄 License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 🔗 Links

- [GitHub Repository](https://github.com/nuvtools/nuvtools-data)
- [NuGet Packages](https://www.nuget.org/profiles/nuvtools)
- [Official Website](https://nuvtools.com)