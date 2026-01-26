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
- **Approximate Count**: Auto-wired approximate count using `sys.dm_db_partition_stats` via `PagingWrapAsync`

```bash
dotnet add package NuvTools.Data.EntityFrameworkCore.SqlServer
```

### NuvTools.Data.EntityFrameworkCore.PostgreSQL

[![NuGet](https://img.shields.io/nuget/v/NuvTools.Data.EntityFrameworkCore.PostgreSQL.svg)](https://www.nuget.org/packages/NuvTools.Data.EntityFrameworkCore.PostgreSQL/)

Specialized helpers for PostgreSQL with EF Core:
- **Simple Registration**: `AddDatabase` and `AddDatabaseByConnectionName` extension methods
- **Snake Case Convention**: `UseSnakeCaseNamingConvention` for PostgreSQL naming standards
- **Npgsql Options**: Configure PostgreSQL-specific options via `NpgsqlDbContextOptionsBuilder`
- **Approximate Count**: Auto-wired approximate count using `pg_class.reltuples` via `PagingWrapAsync`

```bash
dotnet add package NuvTools.Data.EntityFrameworkCore.PostgreSQL
```

## 🚀 Quick Start

### Basic Paging (NuvTools.Data)

```csharp
using NuvTools.Data.Paging;

// In-memory paging (0-indexed)
var items = new List<Product> { /* ... */ };
var pagedResult = items.PagingWrap(pageIndex: 0, pageSize: 20);

Console.WriteLine($"Page {pagedResult.PageIndex + 1} of {Math.Ceiling(pagedResult.Total / 20.0)}");
foreach (var item in pagedResult.List)
{
    Console.WriteLine(item.Name);
}
```

### Advanced Paging with Count Strategies

The paging system supports multiple count strategies via `PagingOptions` to optimize performance for large datasets:

| Count Mode | Description | Use Case |
|------------|-------------|----------|
| `Always` | Execute COUNT query (default) | Small to medium datasets |
| `Skip` | No count query, Total = null | Infinite scroll, "Load more" |
| `HasMore` | Fetch N+1 to detect more pages | Mobile apps, simple pagination |
| `Threshold` | Count up to a limit (e.g., 10,000+) | Large datasets with UI limit |
| `Approximate` | Use database metadata for instant count | Very large tables (millions of rows) |

#### Skip Count (Infinite Scroll)

```csharp
using NuvTools.Data.Paging;

// For infinite scroll - no count query executed
var result = await query.PagingWrapAsync(pageIndex, pageSize, PagingOptions.SkipCount);

// Total is null, just iterate through results
while (result.List.Any())
{
    foreach (var item in result.List) { /* process */ }
    pageIndex++;
    result = await query.PagingWrapAsync(pageIndex, pageSize, PagingOptions.SkipCount);
}
```

#### HasMore Pattern (Fetch N+1)

```csharp
using NuvTools.Data.Paging;

// Fetches pageSize + 1 records to determine if more exist
var result = await query.PagingWrapAsync(pageIndex, pageSize, PagingOptions.UseHasMore);

// Check if there are more pages without running a count query
if (result.HasMore == true)
{
    Console.WriteLine("Load more...");
}
```

#### Threshold Counting

```csharp
using NuvTools.Data.Paging;

// Count only up to 10,000 - useful for "10,000+ results" UI
var result = await query.PagingWrapAsync(pageIndex, pageSize, PagingOptions.WithThreshold(10000));

if (result.HasMore == true)
{
    Console.WriteLine($"Showing {result.Total:N0}+ results");
}
else
{
    Console.WriteLine($"Showing {result.Total:N0} results");
}
```

#### Approximate Count (Database Metadata)

For very large tables, use database system metadata to get an instant approximate row count. Use the database-specific `PagingWrapAsync` extension that automatically wires up the approximate count query.

**SQL Server (Recommended)**

```csharp
using NuvTools.Data.EntityFrameworkCore.SqlServer.Paging;
using NuvTools.Data.Paging;

public class ProductService
{
    private readonly MyDbContext _context;

    public ProductService(MyDbContext context) => _context = context;

    public async Task<PagingQueryableResult<Product>> GetProductsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Products.Where(p => p.IsActive);

        // Auto-wires approximate count using sys.dm_db_partition_stats
        return await query.PagingWrapAsync(_context, pageIndex, pageSize, PagingOptions.UseApproximate);
    }

    public async Task<long> GetApproximateProductCountAsync()
    {
        // Get approximate count directly
        return await PagingExtensions.GetApproximateCountAsync<Product>(_context);
    }
}
```

**PostgreSQL**

```csharp
using NuvTools.Data.EntityFrameworkCore.PostgreSQL.Paging;
using NuvTools.Data.Paging;

public class ProductService
{
    private readonly MyDbContext _context;

    public ProductService(MyDbContext context) => _context = context;

    public async Task<PagingQueryableResult<Product>> GetProductsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Products.Where(p => p.IsActive);

        // Auto-wires approximate count using pg_class.reltuples
        return await query.PagingWrapAsync(_context, pageIndex, pageSize, PagingOptions.UseApproximate);
    }
}
```

**Alternative: Custom Provider Delegate**

For custom scenarios, you can provide your own approximate count delegate:

```csharp
using NuvTools.Data.Paging;

var options = PagingOptions.WithApproximateCount(async cancellationToken =>
{
    // Custom approximate count logic
    return await _context.Database
        .SqlQueryRaw<long>("SELECT your_custom_count_query")
        .FirstOrDefaultAsync(cancellationToken);
});

var result = await query.PagingWrapAsync(pageIndex, pageSize, options);
```

#### Pre-Calculated Total (Cached Count)

```csharp
using NuvTools.Data.Paging;

// Use a cached total to skip the count query
var cachedTotal = await _cache.GetOrCreateAsync("products_count", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return await _context.Products.CountAsync();
});

var result = await query.PagingWrapAsync(pageIndex, pageSize, PagingOptions.WithTotal(cachedTotal));
```

#### Using PagingFilter with Sorting

```csharp
using NuvTools.Data.Paging;
using NuvTools.Data.Sorting.Enumerations;

public enum ProductSortColumn { Name, Price, CreatedAt }

// Define a filter with paging and sorting
var filter = new PagingFilter<ProductSortColumn>
{
    PageIndex = 0,
    PageSize = 20,
    SortColumn = ProductSortColumn.Price,
    SortDirection = SortDirection.DESC,
    Options = PagingOptions.UseHasMore  // Optional: configure count strategy
};

// Apply in your service
var query = _context.Products
    .Where(p => p.IsActive)
    .Sort(filter.SortColumn, filter.SortDirection);

var result = filter.Options != null
    ? await query.PagingWrapAsync(filter.PageIndex, filter.PageSize, filter.Options)
    : await query.PagingWrapAsync(filter.PageIndex, filter.PageSize);
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
using NuvTools.Data.Paging;

public class ProductService
{
    private readonly MyDbContext _context;

    // Basic paging with exact count (0-indexed)
    public async Task<PagingWithEnumerableList<Product>> GetProductsAsync(int pageIndex, int pageSize)
    {
        var query = _context.Products.Where(p => p.IsActive);
        return await query.PagingWrapWithEnumerableListAsync(pageIndex, pageSize);
    }

    // Optimized paging for large datasets using HasMore pattern
    public async Task<PagingQueryableResult<Product>> GetProductsOptimizedAsync(int pageIndex, int pageSize)
    {
        var query = _context.Products.Where(p => p.IsActive);
        return await query.PagingWrapAsync(pageIndex, pageSize, PagingOptions.UseHasMore);
    }

    // Paging with approximate count for very large tables (SQL Server)
    // Use NuvTools.Data.EntityFrameworkCore.SqlServer.Paging namespace
    public async Task<PagingQueryableResult<Product>> GetProductsWithApproxCountAsync(int pageIndex, int pageSize)
    {
        var query = _context.Products.Where(p => p.IsActive);

        // Auto-wires approximate count - just pass the context and PagingOptions.UseApproximate
        return await query.PagingWrapAsync(_context, pageIndex, pageSize, PagingOptions.UseApproximate);
    }
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
- **Entity Framework Core**: 10.0.0 (for EF Core libraries)
- **Database Providers**:
  - SQL Server: Microsoft.EntityFrameworkCore.SqlServer 10.0.0
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