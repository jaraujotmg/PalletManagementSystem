using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Manages database transactions
    /// </summary>
    public class TransactionManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class
        /// </summary>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public TransactionManager(IUnitOfWork unitOfWork, ILogger<TransactionManager> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes an action within a transaction
        /// </summary>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The result of the action</returns>
        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var result = await action(cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transaction");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Executes an action within a transaction
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A task representing the operation</returns>
        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                await action(cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transaction");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}