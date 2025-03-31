using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Extensions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    public class PalletRepository : Repository<Pallet>, IPalletRepository
    {
        public PalletRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(id));
            }

            // Use the standardized approach with expression-based includes
            return await GetByIdWithIncludesAsync(id, new Expression<Func<Pallet, object>>[]
            {
                p => p.Items
            }, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PalletListDto> GetPalletListByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var query = GetQuery().Where(p => p.Id == id);
            var pallet = query.FirstOrDefault();

            if (pallet == null)
                return null;

            var mapper = PalletMapper.ProjectToListDto().Compile();
            return mapper(pallet);
        }

        /// <inheritdoc/>
        public async Task<PalletListDto> GetPalletListByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            var query = GetQuery().Where(p => p.PalletNumber.Value == palletNumber);
            var pallet = query.FirstOrDefault();

            if (pallet == null)
                return null;

            var mapper = PalletMapper.ProjectToListDto().Compile();
            return mapper(pallet);
        }

        /// <inheritdoc/>
        public async Task<PalletDetailDto> GetPalletDetailByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var palletEntity = await _dbSet
                .Include(p => p.Items)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (palletEntity == null) return null;

            // Step 2: Manually project the fetched entity into PalletDetailDto in memory

            // Create the DTO first (its Items collection is already initialized as new List<...>())
            var palletDetailDto = new PalletDetailDto
            {
                Id = palletEntity.Id,
                PalletNumber = palletEntity.PalletNumber.Value,
                IsTemporary = palletEntity.PalletNumber.IsTemporary,
                ManufacturingOrder = palletEntity.ManufacturingOrder,
                Division = palletEntity.Division.ToString(),
                Platform = palletEntity.Platform.ToString(),
                UnitOfMeasure = palletEntity.UnitOfMeasure.ToString(),
                Quantity = palletEntity.Quantity,
                ItemCount = palletEntity.Items.Count,
                IsClosed = palletEntity.IsClosed,
                CreatedDate = palletEntity.CreatedDate,
                ClosedDate = palletEntity.ClosedDate,
                CreatedBy = palletEntity.CreatedBy,
                // Items = new List<ItemDetailDto>() // Remove direct assignment here
            };

            // --- MODIFIED PART: Add items to the existing collection ---
            if (palletEntity.Items != null && palletEntity.Items.Any())
            {
                foreach (var itemEntity in palletEntity.Items)
                {
                    palletDetailDto.Items.Add(new ItemDetailDto // Use .Add()
                    {
                        Id = itemEntity.Id,
                        ItemNumber = itemEntity.ItemNumber,
                        PalletId = itemEntity.PalletId,
                        ManufacturingOrder = itemEntity.ManufacturingOrder,
                        ManufacturingOrderLine = itemEntity.ManufacturingOrderLine,
                        ServiceOrder = itemEntity.ServiceOrder,
                        ServiceOrderLine = itemEntity.ServiceOrderLine,
                        FinalOrder = itemEntity.FinalOrder,
                        FinalOrderLine = itemEntity.FinalOrderLine,
                        ClientCode = itemEntity.ClientCode,
                        ClientName = itemEntity.ClientName,
                        Reference = itemEntity.Reference,
                        Finish = itemEntity.Finish,
                        Color = itemEntity.Color,
                        Quantity = itemEntity.Quantity,
                        QuantityUnit = itemEntity.QuantityUnit,
                        Weight = itemEntity.Weight,
                        WeightUnit = itemEntity.WeightUnit,
                        Width = itemEntity.Width,
                        WidthUnit = itemEntity.WidthUnit,
                        Quality = itemEntity.Quality,
                        Batch = itemEntity.Batch,
                        CreatedDate = itemEntity.CreatedDate,
                        CreatedBy = itemEntity.CreatedBy,
                        Pallet = new PalletInfo { Id = palletEntity.Id, PalletNumber = palletEntity.PalletNumber.Value, IsClosed = palletEntity.IsClosed }
                    });
                }
            }
            // ----------------------------------------------------------

            return palletDetailDto;
        }


        /// <inheritdoc/>
        public async Task<PalletDetailDto> GetPalletDetailByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            // For complex includes like this, we'll use the EF Core version for now
            var pallet = await _dbSet
                .Include(p => p.Items)
                .Where(p => p.PalletNumber.Value == palletNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (pallet == null)
                return null;

            var mapper = PalletMapper.ProjectToDetailDto().Compile();
            return mapper(pallet);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletListDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            CancellationToken cancellationToken = default)
        {
            // For backward compatibility and to ensure good performance with
            // complex projections, we'll use EF Core directly for this
            var pallets = await _dbSet
                .Where(p => p.Division == division && p.Platform == platform)
                .ToListAsync(cancellationToken);

            var mapper = PalletMapper.ProjectToListDto().Compile();
            return pallets.Select(mapper).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletDetailDto>> GetPalletDetailsByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            CancellationToken cancellationToken = default)
        {
            // For backward compatibility and to ensure good performance with
            // complex projections, we'll use EF Core directly for this
            var pallets = await _dbSet
                .Include(p => p.Items)
                .Where(p => p.Division == division && p.Platform == platform)
                .ToListAsync(cancellationToken);

            var mapper = PalletMapper.ProjectToDetailDto().Compile();
            return pallets.Select(mapper).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletListDto>> GetPalletsByStatusAsync(
            bool isClosed,
            CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Where(p => p.IsClosed == isClosed)
                .ToListAsync(cancellationToken);

            var mapper = PalletMapper.ProjectToListDto().Compile();
            return pallets.Select(mapper).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletListDto>> GetPalletsByManufacturingOrderAsync(
            string manufacturingOrder,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            var pallets = await _dbSet
                .Where(p => p.ManufacturingOrder == manufacturingOrder)
                .ToListAsync(cancellationToken);

            var mapper = PalletMapper.ProjectToListDto().Compile();
            return pallets.Select(mapper).ToList();
        }

        /// <inheritdoc/>
        public async Task<int> GetNextTemporarySequenceNumberAsync(CancellationToken cancellationToken = default)
        {
            // Get all temporary pallet numbers that start with TEMP-
            var tempPalletNumbers = await _dbSet
                .Where(p => p.PalletNumber.IsTemporary)
                .Select(p => p.PalletNumber.Value)
                .Where(p => p.StartsWith("TEMP-", StringComparison.OrdinalIgnoreCase))
                .ToListAsync(cancellationToken);

            // Process the numbers in memory
            var maxNumber = tempPalletNumbers
                .Select(p => {
                    if (int.TryParse(p.Substring(5), out int num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }

        /// <inheritdoc/>
        public async Task<int> GetNextPermanentSequenceNumberAsync(
            Division division,
            CancellationToken cancellationToken = default)
        {
            switch (division)
            {
                case Division.MA:
                    // Get all permanent pallet numbers for Manufacturing division
                    var maPalletNumbers = await _dbSet
                        .Where(p => !p.PalletNumber.IsTemporary && p.Division == division)
                        .Select(p => p.PalletNumber.Value)
                        .Where(p => p.StartsWith("P8"))
                        .ToListAsync(cancellationToken);

                    // Process the numbers in memory
                    var maMaxNumber = maPalletNumbers
                        .Select(p => {
                            if (int.TryParse(p.Substring(2), out int num))
                                return num;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();

                    return maMaxNumber + 1;

                case Division.TC:
                    // Get all permanent pallet numbers for Technical Center division
                    var tcPalletNumbers = await _dbSet
                        .Where(p => !p.PalletNumber.IsTemporary && p.Division == division)
                        .Select(p => p.PalletNumber.Value)
                        .Where(p => p.StartsWith("47"))
                        .ToListAsync(cancellationToken);

                    // Process the numbers in memory
                    var tcMaxNumber = tcPalletNumbers
                        .Select(p => {
                            if (int.TryParse(p.Substring(2), out int num))
                                return num;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();

                    return tcMaxNumber + 1;

                default:
                    throw new ArgumentException($"Unsupported division: {division}", nameof(division));
            }
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletListDto>> SearchPalletsAsync(
            string keyword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<PalletListDto>();
            }

            keyword = keyword.Trim();

            var pallets = await _dbSet
                .Where(p =>
                    p.PalletNumber.Value.Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword)
                )
                .ToListAsync(cancellationToken);

            var mapper = PalletMapper.ProjectToListDto().Compile();
            return pallets.Select(mapper).ToList();
        }

        /// <inheritdoc/>
        public async Task<PagedResultDto<PalletListDto>> GetPagedPalletsAsync(
            int pageNumber,
            int pageSize,
            Division? division = null,
            Platform? platform = null,
            bool? isClosed = null,
            string keyword = null,
            bool orderByCreatedDate = false,
            bool descending = false,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentException("Page number must be greater than or equal to 1", nameof(pageNumber));
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("Page size must be greater than or equal to 1", nameof(pageSize));
            }

            var query = _dbSet.AsQueryable();

            if (division.HasValue)
            {
                query = query.Where(p => p.Division == division.Value);
            }

            if (platform.HasValue)
            {
                query = query.Where(p => p.Platform == platform.Value);
            }

            if (isClosed.HasValue)
            {
                query = query.Where(p => p.IsClosed == isClosed.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    p.PalletNumber.Value.Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (orderByCreatedDate)
            {
                query = descending
                    ? query.OrderByDescending(p => p.CreatedDate)
                    : query.OrderBy(p => p.CreatedDate);
            }
            else
            {
                query = descending
                    ? query.OrderByDescending(p => p.PalletNumber.Value)
                    : query.OrderBy(p => p.PalletNumber.Value);
            }

            var skip = (pageNumber - 1) * pageSize;
            var palletEntities = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var mapper = PalletMapper.ProjectToListDto().Compile();
            var items = palletEntities.Select(mapper).ToList();

            return new PagedResultDto<PalletListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchResultDto>> GetPalletSearchResultsAsync(
            string keyword,
            int maxResults = 0,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<SearchResultDto>();
            }

            // Build query
            var query = _dbSet.AsQueryable()
                .Where(p =>
                    p.PalletNumber.Value.Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword));

            // Apply max results limit if specified
            if (maxResults > 0)
            {
                query = query.Take(maxResults);
            }

            var pallets = await query.ToListAsync(cancellationToken);

            return pallets.Select(p => new SearchResultDto
            {
                Id = p.Id,
                EntityType = "Pallet",
                Identifier = p.PalletNumber.Value,
                AdditionalInfo = $"MO: {p.ManufacturingOrder}",
                ViewUrl = $"/Pallets/Details/{p.Id}"
            });
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchSuggestionDto>> GetPalletSearchSuggestionsAsync(
            string keyword,
            int maxResults = 0,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<SearchSuggestionDto>();
            }

            // Build query
            var query = _dbSet.AsQueryable()
                .Where(p =>
                    p.PalletNumber.Value.Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword));

            // Apply max results limit if specified
            if (maxResults > 0)
            {
                query = query.Take(maxResults);
            }

            var pallets = await query.ToListAsync(cancellationToken);

            return pallets.Select(p => new SearchSuggestionDto
            {
                Text = p.PalletNumber.Value,
                Type = "Pallet",
                Url = $"/Pallets/Details/{p.Id}",
                EntityId = p.Id,
                IsViewAll = false
            });
        }
    }
}