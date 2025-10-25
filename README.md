# Nuv Tools Data Libraries

Nuv Tools Data Libraries are a set of .NET libraries designed to simplify and standardize data access and manipulation. They provide abstractions and helpers for common data operations, as well as seamless integration with Entity Framework Core for SQL Server and PostgreSQL. All libraries support .NET 8 and .NET 9.

## Libraries Overview

### NuvTools.Data

Core library with generic helpers for data manipulation:
- **Paging**: Abstracts paging logic for collections, making it easy to implement paged results in APIs and UIs.
- **Filtering and Utilities**: Provides reusable components for filtering, sorting, and transforming data.
- **No direct dependency on any ORM or database provider.**

### NuvTools.Data.EntityFrameworkCore

Extensions for Entity Framework Core:
- **DbContext Registration**: Simplifies the registration of EF Core contexts in DI containers.
- **Connection Management**: Helpers for managing connection strings and configuration.
- **Repository Patterns**: Utilities to implement repository and unit-of-work patterns with EF Core.
- **Works with any EF Core-supported database provider.**

### NuvTools.Data.EntityFrameworkCore.SqlServer

Specialized helpers for SQL Server with EF Core:
- **DbContext Registration**: Extension methods to register SQL Server contexts using connection names or strings.
- **Configuration Integration**: Supports loading connection strings from configuration files (e.g., `appsettings.json`).
- **Custom Options**: Allows customization of SQL Server-specific EF Core options.

### NuvTools.Data.EntityFrameworkCore.PostgreSQL

Specialized helpers for PostgreSQL with EF Core:
- **DbContext Registration**: Extension methods to register PostgreSQL contexts using connection names or strings.
- **Configuration Integration**: Supports loading connection strings from configuration files (e.g., `appsettings.json`).
- **Npgsql Options**: Allows customization of PostgreSQL-specific EF Core options via `NpgsqlDbContextOptionsBuilder`.