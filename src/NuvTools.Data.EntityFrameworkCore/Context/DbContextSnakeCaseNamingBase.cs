using Microsoft.EntityFrameworkCore;

namespace NuvTools.Data.EntityFrameworkCore.Context;

public abstract class DbContextSnakeCaseNamingBase : DbContextBase
{
    protected DbContextSnakeCaseNamingBase()
    {
    }

    protected DbContextSnakeCaseNamingBase(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (entity.GetTableName() is { } tableName)
                entity.SetTableName(ToSnakeCase(tableName));

            if (entity.GetSchema() is { } schemaName)
                entity.SetSchema(ToSnakeCase(schemaName));
        }
    }

    private static string ToSnakeCase(string name)
    {
        return string.Concat(
            name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString())
        );
    }
}
