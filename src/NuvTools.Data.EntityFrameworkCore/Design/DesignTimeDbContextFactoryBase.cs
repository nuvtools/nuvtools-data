using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace NuvTools.Data.EntityFrameworkCore.Design;

public abstract class DesignTimeDbContextFactoryBase<TContext> :
    IDesignTimeDbContextFactory<TContext> where TContext : DbContext
{

    /// <summary>
    /// Contains the configuration settings loaded from files.
    /// <para><seealso cref="InitializeConfiguration(JsonConfigurationSource[])"/></para>
    /// </summary>
    protected IConfiguration Configuration { get; private set; }

    /// <summary>
    /// Allows initialize the Configuration settings for later use.
    /// </summary>
    /// <param name="files">Json file to be added into Configuration settings. Note: The base path is set to the current directory application.</param>
    protected void InitializeConfiguration(params JsonConfigurationSource[] files)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());

        foreach (var path in files)
            builder.AddJsonFile((a) =>
            {
                a.Path = path.Path;
                a.Optional = path.Optional;
                a.ReloadOnChange = path.ReloadOnChange;
                a.ReloadDelay = path.ReloadDelay;
            });

        Configuration = builder.Build();
    }

    public TContext CreateDbContext(string[] args)
    {
        return CreateNewInstance(new DbContextOptionsBuilder<TContext>());
    }

    protected abstract TContext CreateNewInstance(DbContextOptionsBuilder<TContext> optionsBuilder);
}
