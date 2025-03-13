using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Generic repository interface
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>The entity if found, null otherwise</returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>A collection of all entities</returns>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Finds entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">The predicate expression</param>
        /// <returns>A collection of matching entities</returns>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Checks if any entity matches the given predicate
        /// </summary>
        /// <param name="predicate">The predicate expression</param>
        /// <returns>True if a match exists, false otherwise</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets the count of entities
        /// </summary>
        /// <returns>The count of entities</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Gets the count of entities that match the given predicate
        /// </summary>
        /// <param name="predicate">The predicate expression</param>
        /// <returns>The count of matching entities</returns>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}