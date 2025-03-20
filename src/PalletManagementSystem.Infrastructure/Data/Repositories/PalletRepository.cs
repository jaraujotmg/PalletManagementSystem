using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    public class PalletRepository : Repository<Pallet>, IPalletRepository
    {
        public PalletRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<PalletListDto> GetPalletListByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Id == id)
                .ProjectToListDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PalletDetailDto> GetPalletDetailByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Id == id)
                .ProjectToDetailDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PalletListDto> GetPalletListByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            return await _dbSet
                .Where(p => p.PalletNumber.Value == palletNumber)
                .ProjectToListDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PalletDetailDto> GetPalletDetailByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            return await _dbSet
                .Where(p => p.PalletNumber.Value == palletNumber)
                .ProjectToDetailDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletListDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Division == division && p.Platform == platform)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletDetailDto>> GetPalletDetailsByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Division == division && p.Platform == platform)
                .ProjectToDetailDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<PalletListDto>> GetPalletsByStatusAsync(
            bool isClosed,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.IsClosed == isClosed)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
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

            return await _dbSet
                .Where(p => p.ManufacturingOrder == manufacturingOrder)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> GetNextTemporarySequenceNumberAsync(CancellationToken cancellationToken = default)
        {
            var maxNumber = await _dbSet
                .Where(p => p.PalletNumber.IsTemporary)
                .Select(p => p.PalletNumber.Value)
                .Where(p => p.StartsWith("TEMP-", StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    if (int.TryParse(p.Substring(5), out int num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(0)
                .MaxAsync(cancellationToken);

            return maxNumber + 1;
        }

        /// <inheritdoc/>
        public async Task<int> GetNextPermanentSequenceNumberAsync(
            Division division,
            CancellationToken cancellationToken = default)
        {
            var maxNumber = 0;

            switch (division)
            {
                case Division.MA:
                    maxNumber = await _dbSet
                        .Where(p => !p.PalletNumber.IsTemporary && p.Division == division)
                        .Select(p => p.PalletNumber.Value)
                        .Where(p => p.StartsWith("P8"))
                        .Select(p =>
                        {
                            if (int.TryParse(p.Substring(2), out int num))
                                return num;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .MaxAsync(cancellationToken);
                    break;

                case Division.TC:
                    maxNumber = await _dbSet
                        .Where(p => !p.PalletNumber.IsTemporary && p.Division == division)
                        .Select(p => p.PalletNumber.Value)
                        .Where(p => p.StartsWith("47"))
                        .Select(p =>
                        {
                            if (int.TryParse(p.Substring(2), out int num))
                                return num;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .MaxAsync(cancellationToken);
                    break;

                default:
                    throw new ArgumentException($"Unsupported division: {division}", nameof(division));
            }

            return maxNumber + 1;
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

            return await _dbSet
                .Where(p =>
                    p.PalletNumber.Value.Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword)
                )
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
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
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);

            return new PagedResultDto<PalletListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}