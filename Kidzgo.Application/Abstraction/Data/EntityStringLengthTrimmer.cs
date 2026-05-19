using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kidzgo.Application.Abstraction.Data;

internal static class EntityStringLengthTrimmer
{
    public static void TrimToModelLimits<TEntity>(IDbContext context, TEntity entity)
        where TEntity : class
    {
        if (context is not DbContext dbContext)
        {
            return;
        }

        var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
        if (entityType is null)
        {
            return;
        }

        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType != typeof(string))
            {
                continue;
            }

            var propertyInfo = property.PropertyInfo;
            if (propertyInfo is null || !propertyInfo.CanRead || !propertyInfo.CanWrite)
            {
                continue;
            }

            if (propertyInfo.GetValue(entity) is not string value)
            {
                continue;
            }

            var normalized = NormalizeForLimitedColumn(value);
            var maxLength = property.GetMaxLength();
            if (maxLength.HasValue && normalized.Length > maxLength.Value)
            {
                normalized = normalized[..maxLength.Value].Trim();
            }

            if (!string.Equals(value, normalized, StringComparison.Ordinal))
            {
                propertyInfo.SetValue(entity, normalized);
            }
        }
    }

    public static void TrimToModelLimits<TEntity>(IDbContext context, IEnumerable<TEntity> entities)
        where TEntity : class
    {
        foreach (var entity in entities)
        {
            TrimToModelLimits(context, entity);
        }
    }

    private static string NormalizeForLimitedColumn(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
