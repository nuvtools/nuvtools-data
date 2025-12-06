using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace NuvTools.Data.EntityFrameworkCore.Design;

/// <summary>
/// Base class for implementing design-time DbContext factories.
/// Used by Entity Framework Core tools (like migrations) to create DbContext instances at design time.
/// </summary>
/// <typeparam name="TContext">The type of DbContext to create.</typeparam>
public abstract class DesignTimeDbContextFactoryBase<TContext> :
    IDesignTimeDbContextFactory<TContext> where TContext : DbContext
{

    /// <summary>
    /// Contains the configuration settings loaded from JSON files.
    /// </summary>
    /// <seealso cref="InitializeConfiguration(JsonConfigurationSource[])"/>
    protected IConfiguration? Configuration { get; private set; }

    /// <summary>
    /// Initializes the configuration settings from one or more JSON files.
    /// The base path is set to the current directory.
    /// </summary>
    /// <param name="files">One or more JSON configuration sources to load settings from.</param>
    protected void InitializeConfiguration(params JsonConfigurationSource[] files)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        foreach (var path in files)
            builder.AddJsonFile((a) =>
            {
                a.Path = path.Path!;
                a.Optional = path.Optional;
                a.ReloadOnChange = path.ReloadOnChange;
                a.ReloadDelay = path.ReloadDelay;
            });

        Configuration = builder.Build();
    }

    /// <summary>
    /// Creates a new instance of the DbContext. Called by Entity Framework Core tools.
    /// </summary>
    /// <param name="args">Command-line arguments passed by the tooling.</param>
    /// <returns>A new instance of the DbContext.</returns>
    public TContext CreateDbContext(string[] args)
    {
        return CreateNewInstance(new DbContextOptionsBuilder<TContext>());
    }

    /// <summary>
    /// Creates a new instance of the DbContext with the specified options builder.
    /// Override this method to configure the DbContext options (e.g., connection string, database provider).
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    /// <returns>A new instance of the DbContext.</returns>
    protected abstract TContext CreateNewInstance(DbContextOptionsBuilder<TContext> optionsBuilder);
}
