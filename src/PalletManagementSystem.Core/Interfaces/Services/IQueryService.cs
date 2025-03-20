using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for performing queryable projections across different entity types
    /// </summary>
    public interface IQueryService
    {
        /// <summary>
        /// Projects a queryable to a collection of DTOs
        /// </summary>
        /// <typeparam name="TEntity">The source entity type</typeparam>
        /// <typeparam name="TDto">The DTO type</typeparam>
        /// <param name="query">The source queryable</param>
        /// <param name="includeRelatedEntities">Whether to include related entities</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A collection of DTOs</returns>
        Task<IEnumerable<TDto>> ProjectToDtoAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            bool includeRelatedEntities = false,
            CancellationToken cancellationToken = default)
            where TEntity : class;

        /// <summary>
        /// Projects a queryable to a paged result of DTOs
        /// </summary>
        /// <typeparam name="TEntity">The source entity type</typeparam>
        /// <typeparam name="TDto">The DTO type</typeparam>
        /// <param name="query">The source queryable</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeRelatedEntities">Whether to include related entities</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A paged result of DTOs</returns>
        Task<PagedResultDto<TDto>> ProjectToPagedResultAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            int pageNumber,
            int pageSize,
            bool includeRelatedEntities = false,
            CancellationToken cancellationToken = default)
            where TEntity : class;
    }
}