using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        public ItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<ItemListDto> GetItemListByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.Id == id)
                .ProjectToListDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ItemDetailDto> GetItemDetailByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.Id == id)
                .ProjectToDetailDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ItemListDto> GetItemListByNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            return await _dbSet
                .Where(i => i.ItemNumber == itemNumber)
                .ProjectToListDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ItemDetailDto> GetItemDetailByNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            return await _dbSet
                .Where(i => i.ItemNumber == itemNumber)
                .ProjectToDetailDto()
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ItemListDto>> GetItemsByPalletIdAsync(
            int palletId,
            CancellationToken cancellationToken = default)
        {
            if (palletId <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(palletId));
            }

            return await _dbSet
                .Where(i => i.PalletId == palletId)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ItemListDto>> GetItemsByClientCodeAsync(
            string clientCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
            {
                throw new ArgumentException("Client code cannot be null or empty", nameof(clientCode));
            }

            return await _dbSet
                .Where(i => i.ClientCode == clientCode)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ItemListDto>> GetItemsByManufacturingOrderAsync(
            string manufacturingOrder,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            return await _dbSet
                .Where(i => i.ManufacturingOrder == manufacturingOrder)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> GetNextItemNumberAsync(CancellationToken cancellationToken = default)
        {
            // Fetch all item numbers to process in memory
            var itemNumbers = await _dbSet
                .Select(i => i.ItemNumber)
                .ToListAsync(cancellationToken);

            // Process the conversion in memory where statement lambdas are allowed
            var maxNumber = itemNumbers
                .Select(i => {
                    if (int.TryParse(i, out int num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(100000)
                .Max();

            return (maxNumber + 1).ToString();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ItemListDto>> SearchItemsAsync(
            string keyword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<ItemListDto>();
            }

            keyword = keyword.Trim();

            return await _dbSet
                .Where(i =>
                    i.ItemNumber.Contains(keyword) ||
                    i.ManufacturingOrder.Contains(keyword) ||
                    i.ServiceOrder.Contains(keyword) ||
                    i.FinalOrder.Contains(keyword) ||
                    i.ClientCode.Contains(keyword) ||
                    i.ClientName.Contains(keyword) ||
                    i.Reference.Contains(keyword) ||
                    i.Batch.Contains(keyword)
                )
                .ProjectToListDto()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PagedResultDto<ItemListDto>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool orderByCreatedDate = true,
            bool descending = true,
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

            // Apply filters
            if (palletId.HasValue)
            {
                query = query.Where(i => i.PalletId == palletId.Value);
            }

            if (!string.IsNullOrWhiteSpace(clientCode))
            {
                query = query.Where(i => i.ClientCode == clientCode);
            }

            if (!string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                query = query.Where(i => i.ManufacturingOrder == manufacturingOrder);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(i =>
                    i.ItemNumber.Contains(keyword) ||
                    i.ManufacturingOrder.Contains(keyword) ||
                    i.ServiceOrder.Contains(keyword) ||
                    i.FinalOrder.Contains(keyword) ||
                    i.ClientCode.Contains(keyword) ||
                    i.ClientName.Contains(keyword) ||
                    i.Reference.Contains(keyword) ||
                    i.Batch.Contains(keyword));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering
            if (orderByCreatedDate)
            {
                query = descending
                    ? query.OrderByDescending(i => i.CreatedDate)
                    : query.OrderBy(i => i.CreatedDate);
            }
            else
            {
                query = descending
                    ? query.OrderByDescending(i => i.ItemNumber)
                    : query.OrderBy(i => i.ItemNumber);
            }

            var skip = (pageNumber - 1) * pageSize;
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ProjectToListDto()
                .ToListAsync(cancellationToken);

            return new PagedResultDto<ItemListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<PagedResultDto<ItemDetailDto>> GetPagedItemDetailsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool orderByCreatedDate = true,
            bool descending = true,
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

            // Apply filters
            if (palletId.HasValue)
            {
                query = query.Where(i => i.PalletId == palletId.Value);
            }

            if (!string.IsNullOrWhiteSpace(clientCode))
            {
                query = query.Where(i => i.ClientCode == clientCode);
            }

            if (!string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                query = query.Where(i => i.ManufacturingOrder == manufacturingOrder);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(i =>
                    i.ItemNumber.Contains(keyword) ||
                    i.ManufacturingOrder.Contains(keyword) ||
                    i.ServiceOrder.Contains(keyword) ||
                    i.FinalOrder.Contains(keyword) ||
                    i.ClientCode.Contains(keyword) ||
                    i.ClientName.Contains(keyword) ||
                    i.Reference.Contains(keyword) ||
                    i.Batch.Contains(keyword));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply ordering
            if (orderByCreatedDate)
            {
                query = descending
                    ? query.OrderByDescending(i => i.CreatedDate)
                    : query.OrderBy(i => i.CreatedDate);
            }
            else
            {
                query = descending
                    ? query.OrderByDescending(i => i.ItemNumber)
                    : query.OrderBy(i => i.ItemNumber);
            }

            var skip = (pageNumber - 1) * pageSize;
            var items = await query
                .Skip(skip)
                .Take(pageSize)
                .ProjectToDetailDto()
                .ToListAsync(cancellationToken);

            return new PagedResultDto<ItemDetailDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<Item> GetByIdWithPalletAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid item ID", nameof(id));
            }

            return await _dbSet
                .Include(i => i.Pallet)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }
    }
}