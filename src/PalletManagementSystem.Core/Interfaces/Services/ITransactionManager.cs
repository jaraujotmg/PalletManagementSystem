using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for managing database transactions
    /// </summary>
    public interface ITransactionManager
    {
        /// <summary>
        /// Executes an action within a transaction
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The result of the action</returns>
        Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an action within a transaction
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the operation</returns>
        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an action within a transaction with specified isolation level
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <param name="isolationLevel">The transaction isolation level</param>
        /// <returns>The result of the action</returns>
        Task<TResult> ExecuteInTransactionWithIsolationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Executes an action within a transaction with specified isolation level
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <param name="isolationLevel">The transaction isolation level</param>
        /// <returns>A task representing the operation</returns>
        Task ExecuteInTransactionWithIsolationAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}