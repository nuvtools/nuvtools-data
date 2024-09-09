using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Data.EntityFrameworkCore.SqlServer.Extensions;

public static class ServiceCollectionExtensions
{

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

    public static IServiceCollection AddDatabaseByConnectionName<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
    {
        return services.AddDatabase<TContext>(configuration.GetConnectionString(connectionName), sqlServerOptionsAction, contextLifetime);
    }

    public static IServiceCollection AddDatabase<TContext>(
        this IServiceCollection services,
        string? connectionString,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContext
    {
        return services
            .AddDbContext<TContext>(options => options
                .UseSqlServer(connectionString, sqlServerOptionsAction), contextLifetime: contextLifetime);
    }
}