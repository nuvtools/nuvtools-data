using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Extensions;

/// <summary>
/// Extension methods for configuring Entity Framework Core models for PostgreSQL databases.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies snake_case naming convention to all database objects (tables, columns, keys, foreign keys, and indexes).
    /// This is a common convention in PostgreSQL databases where names are lowercase with underscores.
    /// </summary>
    /// <param name="builder">The model builder to configure.</param>
    /// <example>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     modelBuilder.UseSnakeCaseNamingConvention();
    ///     base.OnModelCreating(modelBuilder);
    /// }
    /// </example>
    /// <remarks>
    /// Converts PascalCase names like "OrderDetails" to snake_case like "order_details".
    /// This affects table names, column names, schema names, key names, foreign key constraints, and index names.
    /// </remarks>
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            var schema = entity.GetSchema();

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                tableName = ToSnakeCase(tableName);
                entity.SetTableName(tableName);
            }

            if (!string.IsNullOrWhiteSpace(schema))
            {
                schema = ToSnakeCase(schema);
                entity.SetSchema(schema);
            }

            var storeObject = StoreObjectIdentifier.Table(tableName!, schema);

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName(storeObject);
                if (!string.IsNullOrWhiteSpace(columnName))
                    property.SetColumnName(ToSnakeCase(columnName));
            }

            foreach (var key in entity.GetKeys())
            {
                var name = key.GetName();
                if (!string.IsNullOrWhiteSpace(name))
                    key.SetName(ToSnakeCase(name));
            }

            foreach (var fk in entity.GetForeignKeys())
            {
                var name = fk.GetConstraintName();
                if (!string.IsNullOrWhiteSpace(name))
                    fk.SetConstraintName(ToSnakeCase(name));
            }

            foreach (var index in entity.GetIndexes())
            {
                var name = index.GetDatabaseName();
                if (!string.IsNullOrWhiteSpace(name))
                    index.SetDatabaseName(ToSnakeCase(name));
            }
        }
    }

    /// <summary>
    /// Converts a PascalCase or camelCase string to snake_case.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The snake_case version of the input string.</returns>
    /// <example>
    /// ToSnakeCase("OrderDetails") returns "order_details"
    /// ToSnakeCase("customerId") returns "customer_id"
    /// ToSnakeCase("ProductID2") returns "product_id_2"
    /// </example>
    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var sb = new StringBuilder(input.Length + 10);
        sb.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c) && (char.IsLower(input[i - 1]) || char.IsDigit(input[i - 1])))
                sb.Append('_');

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}