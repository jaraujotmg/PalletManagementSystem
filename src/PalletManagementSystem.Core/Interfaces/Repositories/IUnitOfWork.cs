using System;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Represents a unit of work for managing transactions across multiple repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets the repository for the specified entity type
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>The repository</returns>
        IRepository<T> Repository<T>() where T : class;

        /// <summary>
        /// Gets the pallet repository
        /// </summary>
        IPalletRepository PalletRepository { get; }

        /// <summary>
        /// Gets the item repository
        /// </summary>
        IItemRepository ItemRepository { get; }

        /// <summary>
        /// Saves all changes made in this unit of work to the database
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a transaction
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the transaction
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}