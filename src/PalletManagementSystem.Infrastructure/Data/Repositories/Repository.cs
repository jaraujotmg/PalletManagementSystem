using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Extensions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class Repository<T> : IRepository<T>, IQueryableRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{T}"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Gets a queryable view of the repository
        /// </summary>
        public IQueryable<T> Queryable => _dbSet.AsQueryable();

        /// <inheritdoc/>
        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        /// <inheritdoc/>
        public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1", nameof(pageSize));

            var totalCount = await _dbSet.CountAsync(cancellationToken);
            var skip = (pageNumber - 1) * pageSize;
            var items = await _dbSet
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<PagedResult<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1", nameof(pageSize));

            var query = _dbSet.Where(predicate);
            var totalCount = await query.CountAsync(cancellationToken);
            var skip = (pageNumber - 1) * pageSize;
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        /// <inheritdoc/>
        public virtual async Task<IReadOnlyList<T>> FindAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<T> FindFirstAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            // Note: We don't save changes here; that's the responsibility of the UnitOfWork
            return entity;
        }

        /// <inheritdoc/>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            // Note: We don't save changes here; that's the responsibility of the UnitOfWork
        }

        /// <inheritdoc/>
        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Entry(entity).State = EntityState.Modified;
            // Note: We don't save changes here; that's the responsibility of the UnitOfWork
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
            // Note: We don't save changes here; that's the responsibility of the UnitOfWork
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            // Note: We don't save changes here; that's the responsibility of the UnitOfWork
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.RemoveRange(entities);
            // Note: We don't save changes here; that's the responsibility of the UnitOfWork
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Applies a specification to the query
        /// </summary>
        /// <param name="specification">The specification to apply</param>
        /// <returns>The query with the specification applied</returns>
        protected virtual IQueryable<T> ApplySpecification(ISpecification<T> specification)
        {
            return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), specification);
        }

        /// <summary>
        /// Gets an entity by ID with specified navigation properties
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <param name="includes">The navigation properties to include</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The entity with included navigation properties</returns>
        public virtual async Task<T> GetByIdWithIncludesAsync(int id, IEnumerable<string> includes, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            query = query.IncludeMultiple(includes);
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);
        }

        /// <summary>
        /// Gets an entity by ID with specified navigation properties
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <param name="includes">The navigation properties to include as expressions</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The entity with included navigation properties</returns>
        public virtual async Task<T> GetByIdWithIncludesAsync(int id, IEnumerable<Expression<Func<T, object>>> includes, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            query = query.IncludeMultiple(includes);
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, cancellationToken);
        }

        /// <summary>
        /// Finds entities matching a predicate with specified navigation properties
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="includes">The navigation properties to include</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching entities with includes</returns>
        public virtual async Task<IReadOnlyList<T>> FindWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            IEnumerable<string> includes,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            query = query.IncludeMultiple(includes);
            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Finds entities matching a predicate with specified navigation properties
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="includes">The navigation properties to include as expressions</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>A read-only list of matching entities with includes</returns>
        public virtual async Task<IReadOnlyList<T>> FindWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            IEnumerable<Expression<Func<T, object>>> includes,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            query = query.IncludeMultiple(includes);
            return await query.Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Finds the first entity matching a predicate with specified navigation properties
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="includes">The navigation properties to include</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The first matching entity with included navigation properties, or null if not found</returns>
        public virtual async Task<T> FindFirstWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            IEnumerable<string> includes,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            query = query.IncludeMultiple(includes);
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Finds the first entity matching a predicate with specified navigation properties
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="includes">The navigation properties to include as expressions</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>The first matching entity with included navigation properties, or null if not found</returns>
        public virtual async Task<T> FindFirstWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            IEnumerable<Expression<Func<T, object>>> includes,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();
            query = query.IncludeMultiple(includes);
            return await query.FirstOrDefaultAsync(predicate, cancellationToken);
        }


    }
}