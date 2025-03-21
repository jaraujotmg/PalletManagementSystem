using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Services;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Extension methods for ITransactionManager
    /// </summary>
    public static class TransactionManagerExtensions
    {
        /// <summary>
        /// Executes a database operation with retry logic
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="transactionManager">The transaction manager</param>
        /// <param name="action">The action to execute</param>
        /// <param name="operationName">The name of the operation for logging</param>
        /// <param name="logger">The logger</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <param name="retryCount">The number of times to retry on failure</param>
        /// <returns>The result of the action</returns>
        public static async Task<TResult> ExecuteWithRetryAsync<TResult>(
            this ITransactionManager transactionManager,
            Func<CancellationToken, Task<TResult>> action,
            string operationName,
            ILogger logger,
            CancellationToken cancellationToken = default,
            int retryCount = 3)
        {
            int attempts = 0;

            while (true)
            {
                attempts++;

                try
                {
                    return await transactionManager.ExecuteInTransactionAsync(action, cancellationToken);
                }
                catch (Exception ex) when (attempts <= retryCount && IsTransientException(ex))
                {
                    // Calculate exponential backoff delay (100ms, 200ms, 400ms, etc.)
                    var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempts - 1) * 100);

                    logger.LogWarning(ex,
                        "Transient error during {OperationName}. Retry attempt {Attempt}/{MaxRetries} after {Delay}ms. Error: {Error}",
                        operationName, attempts, retryCount, delay.TotalMilliseconds, ex.Message);

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Executes a database operation with retry logic
        /// </summary>
        /// <param name="transactionManager">The transaction manager</param>
        /// <param name="action">The action to execute</param>
        /// <param name="operationName">The name of the operation for logging</param>
        /// <param name="logger">The logger</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <param name="retryCount">The number of times to retry on failure</param>
        /// <returns>A task representing the operation</returns>
        public static async Task ExecuteWithRetryAsync(
            this ITransactionManager transactionManager,
            Func<CancellationToken, Task> action,
            string operationName,
            ILogger logger,
            CancellationToken cancellationToken = default,
            int retryCount = 3)
        {
            int attempts = 0;

            while (true)
            {
                attempts++;

                try
                {
                    await transactionManager.ExecuteInTransactionAsync(action, cancellationToken);
                    return;
                }
                catch (Exception ex) when (attempts <= retryCount && IsTransientException(ex))
                {
                    // Calculate exponential backoff delay (100ms, 200ms, 400ms, etc.)
                    var delay = TimeSpan.FromMilliseconds(Math.Pow(2, attempts - 1) * 100);

                    logger.LogWarning(ex,
                        "Transient error during {OperationName}. Retry attempt {Attempt}/{MaxRetries} after {Delay}ms. Error: {Error}",
                        operationName, attempts, retryCount, delay.TotalMilliseconds, ex.Message);

                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        private static bool IsTransientException(Exception ex)
        {
            // Detect transient database errors that should be retried
            // For SQL Server, this might include connection errors, deadlocks, etc.
            if (ex is System.Data.SqlClient.SqlException sqlEx)
            {
                return sqlEx.Number == 1205 // Deadlock
                    || sqlEx.Number == -2   // Timeout
                    || sqlEx.Number == 53   // Connection error
                    || sqlEx.Number == 40613; // Database unavailable
            }

            // Check for Entity Framework exceptions
            if (ex.GetType().FullName == "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException")
            {
                return true;
            }

            // Check for transient connection issues
            if (ex is System.IO.IOException || ex is System.TimeoutException)
            {
                return true;
            }

            return false;
        }
    }
}