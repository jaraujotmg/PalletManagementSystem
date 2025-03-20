using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Provides queryable projection services using Entity Framework Core
    /// </summary>
    public class QueryService : IQueryService
    {
        private readonly ILogger<QueryService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryService"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public QueryService(ILogger<QueryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TDto>> ProjectToDtoAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            bool includeRelatedEntities = false,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            try
            {
                // Pattern matching for different entity types
                switch (typeof(TEntity).Name)
                {
                    case nameof(Pallet) when typeof(TDto) == typeof(PalletDto):
                        if (includeRelatedEntities)
                        {
                            return (IEnumerable<TDto>)await (query as IQueryable<Pallet>)
                                .ProjectToDtoWithItems()
                                .ToListAsync(cancellationToken);
                        }
                        return (IEnumerable<TDto>)await (query as IQueryable<Pallet>)
                            .ProjectToDto()
                            .ToListAsync(cancellationToken);

                    case nameof(Item) when typeof(TDto) == typeof(ItemDto):
                        if (includeRelatedEntities)
                        {
                            return (IEnumerable<TDto>)await (query as IQueryable<Item>)
                                .ProjectToDtoWithPallet()
                                .ToListAsync(cancellationToken);
                        }
                        return (IEnumerable<TDto>)await (query as IQueryable<Item>)
                            .ProjectToDto()
                            .ToListAsync(cancellationToken);

                    default:
                        _logger.LogWarning($"No projection found for {typeof(TEntity).Name} to {typeof(TDto).Name}");
                        return Enumerable.Empty<TDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error projecting {typeof(TEntity).Name} to {typeof(TDto).Name}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PagedResultDto<TDto>> ProjectToPagedResultAsync<TEntity, TDto>(
            IQueryable<TEntity> query,
            int pageNumber,
            int pageSize,
            bool includeRelatedEntities = false,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            try
            {
                // Calculate total count before pagination
                var totalCount = await query.CountAsync(cancellationToken);
                var skip = (pageNumber - 1) * pageSize;

                // Pattern matching for different entity types
                switch (typeof(TEntity).Name)
                {
                    case nameof(Pallet) when typeof(TDto) == typeof(PalletDto):
                        var palletQuery = query as IQueryable<Pallet>;
                        var palletItems = includeRelatedEntities
                            ? await palletQuery
                                .Skip(skip)
                                .Take(pageSize)
                                .ProjectToDtoWithItems()
                                .ToListAsync(cancellationToken)
                            : await palletQuery
                                .Skip(skip)
                                .Take(pageSize)
                                .ProjectToDto()
                                .ToListAsync(cancellationToken);

                        return new PagedResultDto<TDto>
                        {
                            Items = (IEnumerable<TDto>)palletItems,
                            TotalCount = totalCount,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };

                    case nameof(Item) when typeof(TDto) == typeof(ItemDto):
                        var itemQuery = query as IQueryable<Item>;
                        var itemItems = includeRelatedEntities
                            ? await itemQuery
                                .Skip(skip)
                                .Take(pageSize)
                                .ProjectToDtoWithPallet()
                                .ToListAsync(cancellationToken)
                            : await itemQuery
                                .Skip(skip)
                                .Take(pageSize)
                                .ProjectToDto()
                                .ToListAsync(cancellationToken);

                        return new PagedResultDto<TDto>
                        {
                            Items = (IEnumerable<TDto>)itemItems,
                            TotalCount = totalCount,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };

                    default:
                        _logger.LogWarning($"No paged projection found for {typeof(TEntity).Name} to {typeof(TDto).Name}");
                        return new PagedResultDto<TDto>
                        {
                            Items = Enumerable.Empty<TDto>(),
                            TotalCount = 0,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error paging projection for {typeof(TEntity).Name} to {typeof(TDto).Name}");
                throw;
            }
        }
    }
}