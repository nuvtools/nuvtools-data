# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NuvTools.Data is a multi-library .NET solution providing data access and manipulation utilities with Entity Framework Core integration. The solution targets .NET 8, .NET 9, and .NET 10, and consists of four main libraries organized in a layered architecture:

1. **NuvTools.Data**: Core library with ORM-agnostic helpers for paging, filtering, and sorting
2. **NuvTools.Data.EntityFrameworkCore**: EF Core extensions for DbContext management and repository patterns
3. **NuvTools.Data.EntityFrameworkCore.SqlServer**: SQL Server-specific DbContext registration helpers
4. **NuvTools.Data.EntityFrameworkCore.PostgreSQL**: PostgreSQL-specific DbContext registration helpers with snake_case naming conventions

## Build Commands

```bash
# Build the entire solution
dotnet build NuvTools.Data.slnx

# Build in Release mode (generates NuGet packages)
dotnet build NuvTools.Data.slnx -c Release

# Restore dependencies
dotnet restore

# Clean build artifacts
dotnet clean
```

## Testing

```bash
# Run all tests
dotnet test

# Run tests in a specific project
dotnet test tests/NuvTools.Data.Test/NuvTools.Data.Test.csproj
dotnet test tests/NuvTools.Data.EntityFrameworkCore.Test/NuvTools.Data.EntityFrameworkCore.Test.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

The test projects use NUnit as the testing framework (version 4.4.0) and target .NET 10 only.

## Project Structure

```
src/
  NuvTools.Data/                              # Core library (no dependencies)
    Paging/                                   # Paging abstractions and helpers
    Sorting/                                  # Sorting utilities
  NuvTools.Data.EntityFrameworkCore/          # EF Core base functionality
    Context/                                  # DbContextBase and interfaces
    Extensions/                               # DbContext extension methods
    Paging/                                   # Async paging for IQueryable
  NuvTools.Data.EntityFrameworkCore.SqlServer/
    Extensions/
      ServiceCollectionExtensions.cs          # SQL Server DI registration
  NuvTools.Data.EntityFrameworkCore.PostgreSQL/
    Extensions/
      ServiceCollectionExtensions.cs          # PostgreSQL DI registration
      ModelBuilderExtensions.cs               # Snake case naming convention helper

tests/
  NuvTools.Data.Test/
  NuvTools.Data.EntityFrameworkCore.Test/
```

## Key Architecture Patterns

### Paging System

The paging system has two variants:
- `PagingWithQueryableList<T>`: For deferred execution with IQueryable (EF Core)
- `PagingWithEnumerableList<T>`: For in-memory collections with IEnumerable

Both inherit from `PagingBase<T, R>` which provides:
- `PageIndex`: Current page (0-indexed, validated via `PagingHelper.GetPageIndex`)
- `Total`: Total record count
- `List`: The paged data

Use `PagingWrapAsync()` for async EF Core queries and `PagingWrap()` for synchronous in-memory collections.

**Note:** The paging system uses 0-based indexing. The first page is `pageIndex: 0`, the second page is `pageIndex: 1`, etc.

### DbContext Extensions

The `DbContextBase` class (in NuvTools.Data.EntityFrameworkCore) provides a standard base implementation that:
- Implements `IDbContextCommands` and `IDbContextWithListCommands` interfaces
- Wraps transaction management (Begin/Commit/Rollback)
- Provides `AddAndSaveAsync`, `UpdateAndSaveAsync`, `RemoveAndSaveAsync` helpers that return `IResult` from NuvTools.Common
- Supports composite keys via `AddAndSaveWithCompositeKeyAsync` and `FindPrimaryKeyValues`
- Includes bulk operations: `SyncFromListAsync`, `AddOrUpdateFromListAsync`, `AddOrRemoveFromListAsync`
- Wraps execution strategies for connection resiliency via `ExecuteWithStrategyAsync`

All operations use the Result pattern (from NuvTools.Common.ResultWrapper) for error handling.

### Database Registration

SQL Server and PostgreSQL libraries provide similar extension methods for DI registration:

**SQL Server:**
```csharp
services.AddDatabaseByConnectionName<MyContext>("ConnectionName");
services.AddDatabase<MyContext>(connectionString);
```

**PostgreSQL:**
```csharp
services.AddDatabaseByConnectionName<MyContext>("ConnectionName");
services.AddDatabase<MyContext>(connectionString);
// Snake case naming convention
modelBuilder.UseSnakeCaseNamingConvention();
```

Both support:
- Loading from appsettings.json (default) or IConfiguration
- Custom options builder actions (SqlServerDbContextOptionsBuilder/NpgsqlDbContextOptionsBuilder)
- ServiceLifetime configuration (default: Scoped)

## Code Standards

### Assembly Configuration
- `ImplicitUsings` enabled
- `Nullable` reference types enabled
- XML documentation generation enabled (`GenerateDocumentationFile`)
- Code style enforcement enabled in build (`EnforceCodeStyleInBuild`)

### EditorConfig Rules
- CS1591 (missing XML comments): Suppressed
- IDE0063 (simple using statements): Suppressed

### NuGet Package Settings
- Version: 10.0.2 (keep synchronized across all projects)
- All packages include icon.png, LICENSE, and README.md
- Repository: https://github.com/nuvtools/nuvtools-data

## Dependencies

### Core Library (NuvTools.Data)
- No external dependencies (ORM-agnostic)

### EF Core Libraries
- Microsoft.EntityFrameworkCore 10.0.2
- Microsoft.Extensions.Configuration.Json 10.0.2
- NuvTools.Common 10.0.2

### SQL Server Library
- Microsoft.EntityFrameworkCore.SqlServer 10.0.2

### PostgreSQL Library
- Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0

### Test Libraries
- NUnit 4.4.0
- NUnit3TestAdapter 6.1.0
- Microsoft.NET.Test.Sdk 18.0.1

## Development Guidelines

When modifying this codebase:
- Maintain multi-targeting for net8, net9, and net10
- Keep version numbers synchronized across all .csproj files
- Use the Result pattern for all data operations (IResult/IResult<T>)
- Prefer async operations with CancellationToken support
- Use `ConfigureAwait(false)` in library code for async calls
- Null validation via ArgumentNullException.ThrowIfNull for public APIs
- Follow existing naming conventions (snake_case for PostgreSQL tables/columns via UseSnakeCaseNamingConvention)
