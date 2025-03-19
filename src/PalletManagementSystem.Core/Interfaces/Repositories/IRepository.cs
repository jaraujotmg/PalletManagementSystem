using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Generic repository interface with enhanced querying capabilities
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by its identifier
        /// </summary>
        /// <param name="id">The entity identifier</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The entity, or null if not found</returns>
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of all entities</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of all entities
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of entities</returns>
        Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets entities matching the specified predicate
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching entities</returns>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of entities matching the specified predicate
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A paged result of matching entities</returns>
        Task<PagedResult<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets entities matching the specified specification
        /// </summary>
        /// <param name="specification">The specification</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching entities</returns>
        Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the specified predicate
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The matching entity, or null if not found</returns>
        Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single entity matching the specified specification
        /// </summary>
        /// <param name="specification">The specification</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The matching entity, or null if not found</returns>
        Task<T> FindFirstAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a range of new entities
        /// </summary>
        /// <param name="entities">The entities to add</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a range of existing entities
        /// </summary>
        /// <param name="entities">The entities to update</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a range of entities
        /// </summary>
        /// <param name="entities">The entities to delete</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any entity matches the specified predicate
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>True if any matching entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the count of all entities
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The count of all entities</returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the count of entities matching the specified predicate
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    }
}