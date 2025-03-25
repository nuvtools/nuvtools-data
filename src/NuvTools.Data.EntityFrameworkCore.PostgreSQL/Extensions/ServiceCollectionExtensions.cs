using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using NuvTools.Data.EntityFrameworkCore.Context;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Extensions;

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
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContextSnakeCaseNamingBase
    {
        IConfiguration configuration = GetConfiguration(settingsFileName);
        return services.AddDatabaseByConnectionName<TContext>(configuration, connectionName, npgsqlOptionsAction, contextLifetime);
    }

    public static IServiceCollection AddDatabaseByConnectionName<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContextSnakeCaseNamingBase
    {
        return services.AddDatabase<TContext>(configuration.GetConnectionString(connectionName), npgsqlOptionsAction, contextLifetime);
    }

    public static IServiceCollection AddDatabase<TContext>(
        this IServiceCollection services,
        string? connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped) where TContext : DbContextSnakeCaseNamingBase
    {
        return services
            .AddDbContext<TContext>(options => options.UseNpgsql(connectionString, npgsqlOptionsAction), 
                                    contextLifetime: contextLifetime);
    }
}