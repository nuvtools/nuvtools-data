﻿using Microsoft.EntityFrameworkCore;
using NuvTools.Common.ResultWrapper;
using System.Linq.Expressions;

namespace NuvTools.Data.EntityFrameworkCore.Extensions;

public static class DbContextWithListExtensions
{
    /// <summary>
    /// Performs add, update, or remove operations on a DbSet based on the provided list and configuration.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance to operate on.</param>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <param name="allowAdd">Determines whether new entities in the list should be added to the database.</param>
    /// <param name="allowUpdate">Determines whether existing entities in the database should be updated with values from the list.</param>
    /// <param name="allowRemove">Determines whether entities in the database that are not in the list should be removed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
    private static async Task<IResult> ModifyEntitiesAsync<TEntity, TKey>(
        this DbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? filter,
        bool allowAdd,
        bool allowUpdate,
        bool allowRemove,
        CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(keySelector);

        var dbSet = context.Set<TEntity>();
        var entityKeys = entities.Select(keySelector).ToHashSet();

        // Apply filter criteria if provided
        IQueryable<TEntity> query = dbSet.AsQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }

        var dbEntities = await query.AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
        var dbEntityKeys = dbEntities.Select(keySelector).ToHashSet();

        // Find entities to add, update, or remove
        var toAdd = allowAdd ? entities.Where(e => !dbEntityKeys.Contains(keySelector(e))).ToList() : [];
        var toUpdate = allowUpdate ? entities.Where(e => dbEntityKeys.Contains(keySelector(e))).ToList() : [];
        var toRemove = allowRemove ? dbEntities.Where(e => !entityKeys.Contains(keySelector(e))).ToList() : [];

        // Perform remove operation
        if (allowRemove && toRemove.Count != 0)
        {
            dbSet.RemoveRange(toRemove);
        }

        // Perform add operation
        if (allowAdd && toAdd.Count != 0)
        {
            dbSet.AddRange(toAdd);
        }

        // Perform update operation
        if (allowUpdate && toUpdate.Count != 0)
        {
            foreach (var entity in toUpdate)
            {
                var key = keySelector(entity);
                var dbEntity = dbEntities.First(e => keySelector(e).Equals(key));
                context.Entry(dbEntity).CurrentValues.SetValues(entity);
                context.Entry(dbEntity).State = EntityState.Modified;
            }
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex);
        }
    }

    /// <summary>
    /// Synchronizes a DbSet with a provided list by adding, updating, or removing entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance to operate on.</param>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method adds entities in the list that are not in the database, updates entities that exist in both,
    /// and removes entities in the database that are not in the list.
    /// </remarks>
    /// <example>
    /// await context.SyncFromListAsync(
    ///     updatedEmployees,
    ///     e => e.Id,
    ///     e => e.IsActive
    /// );
    /// </example>
    public static Task<IResult> SyncFromListAsync<TEntity, TKey>(
        this DbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        return context.ModifyEntitiesAsync(entities, keySelector, filter, allowAdd: true, allowUpdate: true, allowRemove: true, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Synchronizes a DbSet with a provided list by adding or updating entities, without removing any entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance to operate on.</param>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method adds entities in the list that are not in the database and updates entities that exist in both.
    /// It does not remove any entities from the database.
    /// </remarks>
    /// <example>
    /// await context.AddOrUpdateFromListAsync(
    ///     updatedProducts,
    ///     p => p.ProductCode,
    ///     p => p.IsActive
    /// );
    /// </example>
    public static Task<IResult> AddOrUpdateFromListAsync<TEntity, TKey>(
        this DbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        return context.ModifyEntitiesAsync(entities, keySelector, filter, allowAdd: true, allowUpdate: true, allowRemove: false, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Synchronizes a DbSet with a provided list by adding or removing entities, without updating any entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key used to uniquely identify entities.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance to operate on.</param>
    /// <param name="entities">The list of entities to synchronize with the database.</param>
    /// <param name="keySelector">A function to select the key for matching entities.</param>
    /// <param name="filter">An optional filter predicate to apply to the database entities.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method adds entities in the list that are not in the database and removes entities in the database
    /// that are not in the list. It does not update any entities.
    /// </remarks>
    /// <example>
    /// await context.AddOrRemoveFromListAsync(
    ///     updatedOrders,
    ///     o => o.OrderId,
    ///     o => o.IsPending
    /// );
    /// </example>
    public static Task<IResult> AddOrRemoveFromListAsync<TEntity, TKey>(
        this DbContext context,
        IEnumerable<TEntity> entities,
        Func<TEntity, TKey> keySelector,
        Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        where TEntity : class
        where TKey : notnull
    {
        return context.ModifyEntitiesAsync(entities, keySelector, filter, allowAdd: true, allowUpdate: false, allowRemove: true, cancellationToken: cancellationToken);
    }
}