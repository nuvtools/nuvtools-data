using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text;

namespace NuvTools.Data.EntityFrameworkCore.PostgreSQL.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            var schema = entity.GetSchema();

            if (!string.IsNullOrWhiteSpace(tableName))
                entity.SetTableName(ToSnakeCase(tableName));

            if (!string.IsNullOrWhiteSpace(schema))
                entity.SetSchema(ToSnakeCase(schema));

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
