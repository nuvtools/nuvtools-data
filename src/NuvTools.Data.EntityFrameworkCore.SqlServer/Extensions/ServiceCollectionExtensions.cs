using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Data.EntityFrameworkCore.SqlServer.Extensions;

/// <summary>
/// Extension methods for registering SQL Server DbContext instances in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Loads configuration from a JSON settings file.
    /// </summary>
    /// <param name="settingsFileName">The name of the settings file.</param>
    /// <returns>The loaded configuration.</returns>
    private static IConfiguration GetConfiguration(string settingsFileName)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        var path = new JsonConfigurationSource { Path = settingsFileName };

        builder.AddJsonFile((a) =>
            {
                a.Path = path.Path;
                a.Optional = path.Optional;
                a.ReloadOnChange = path.ReloadOnChange;
                a.ReloadDelay = path.ReloadDelay;
            });

        return builder.Build();
    }

    /// <summary>
    /// Registers a SQL Server DbContext in the service collection using a connection name from a settings file.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionName">The name of the connection string in the settings file.</param>
    /// <param name="settingsFileName">The settings file name. Defaults to "appsettings.json".</param>
    /// <param name="sqlServerOptionsAction">Optional action to configure SQL Server-specific options.</param>
    /// <param name="contextLifetime">The lifetime of the DbContext in the DI container. Defaults to Scoped.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDatabaseByConnectionName<TContext>(
        this IServiceCollection services,
        string connectionName,
        string settingsFileName = "appsettings.json",
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
    {
        IConfiguration configuration = GetConfiguration(settingsFileName);
        return services.AddDatabaseByConnectionName<TContext>(configuration, connectionName, sqlServerOptionsAction, contextLifetime);
    }

    /// <summary>
    /// Registers a SQL Server DbContext in the service collection using a connection name from an existing configuration.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing connection strings.</param>
    /// <param name="connectionName">The name of the connection string in the configuration.</param>
    /// <param name="sqlServerOptionsAction">Optional action to configure SQL Server-specific options.</param>
    /// <param name="contextLifetime">The lifetime of the DbContext in the DI container. Defaults to Scoped.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDatabaseByConnectionName<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
    {
        return services.AddDatabase<TContext>(configuration.GetConnectionString(connectionName), sqlServerOptionsAction, contextLifetime);
    }

    /// <summary>
    /// Registers a SQL Server DbContext in the service collection using a connection string.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="sqlServerOptionsAction">Optional action to configure SQL Server-specific options.</param>
    /// <param name="contextLifetime">The lifetime of the DbContext in the DI container. Defaults to Scoped.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDatabase<TContext>(
        this IServiceCollection services,
        string? connectionString,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options => options
            .UseSqlServer(connectionString, sqlServerOptionsAction), contextLifetime: contextLifetime);

        return services;
    }
}