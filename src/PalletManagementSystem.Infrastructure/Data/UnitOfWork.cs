using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Infrastructure.Data
{
    /// <summary>
    /// Implementation of the unit of work pattern
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly Lazy<IPalletRepository> _palletRepository;
        private readonly Lazy<IItemRepository> _itemRepository;
        private IDbContextTransaction _transaction;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="palletRepository">The pallet repository</param>
        /// <param name="itemRepository">The item repository</param>
        public UnitOfWork(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IPalletRepository palletRepository,
            IItemRepository itemRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = loggerFactory?.CreateLogger<UnitOfWork>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            // Using Lazy to defer repository instantiation until needed
            _palletRepository = new Lazy<IPalletRepository>(() => palletRepository);
            _itemRepository = new Lazy<IItemRepository>(() => itemRepository);
        }

        /// <inheritdoc/>
        public IPalletRepository PalletRepository => _palletRepository.Value;

        /// <inheritdoc/>
        public IItemRepository ItemRepository => _itemRepository.Value;

        /// <inheritdoc/>
        public IRepository<T> Repository<T>() where T : class
        {
            // This method is intended to be used when the specific repository
            // type is not known at compile time or for generic operations

            // For known entity types, return the specialized repository
            if (typeof(T) == typeof(Core.Models.Pallet))
                return (IRepository<T>)PalletRepository;

            if (typeof(T) == typeof(Core.Models.Item))
                return (IRepository<T>)ItemRepository;

            // For other entity types, we'd need to register them with the DI container
            // and return them here, but for now throw an exception
            throw new NotSupportedException($"No repository is registered for entity type {typeof(T).Name}");
        }

        /// <inheritdoc/>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Could add auditing, validation, etc. here before saving changes
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving changes to the database");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            // Ensure we don't already have an active transaction
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            try
            {
                _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                _logger.LogInformation("Database transaction begun");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning database transaction");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            // Ensure we don't already have an active transaction
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress");
            }

            try
            {
                // For .NET Standard 2.0 and EF Core 2.x, we need to use the synchronous method 
                // and wrap it in a Task.Run to make it async
                _transaction = await Task.Run(() => _context.Database.BeginTransaction(isolationLevel), cancellationToken);
                _logger.LogInformation("Database transaction begun with isolation level {IsolationLevel}", isolationLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error beginning database transaction with isolation level {IsolationLevel}", isolationLevel);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction is in progress");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                _transaction.Commit(); // Using Commit instead of CommitAsync for EF Core 2.x
                _logger.LogInformation("Database transaction committed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error committing database transaction");
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <inheritdoc/>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                return;
            }

            try
            {
                _transaction.Rollback(); // Using Rollback instead of RollbackAsync for EF Core 2.x
                _logger.LogInformation("Database transaction rolled back");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rolling back database transaction");
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the resources used by this unit of work
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Dispose the transaction if one exists
                _transaction?.Dispose();

                // Dispose the context
                _context.Dispose();

                _disposed = true;
            }
        }
    }
}