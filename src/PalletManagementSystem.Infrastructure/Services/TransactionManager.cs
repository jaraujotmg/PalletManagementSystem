using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Manages database transactions
    /// </summary>
    public class TransactionManager : ITransactionManager
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<TResult> ExecuteInTransactionWithIsolationAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);
                _logger.LogDebug("Transaction started with isolation level {IsolationLevel}", isolationLevel);

                var result = await action(cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction committed successfully");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transaction. Rolling back...");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ExecuteInTransactionWithIsolationAsync(
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync(isolationLevel, cancellationToken);
                _logger.LogDebug("Transaction started with isolation level {IsolationLevel}", isolationLevel);

                await action(cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                _logger.LogDebug("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing transaction. Rolling back...");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}