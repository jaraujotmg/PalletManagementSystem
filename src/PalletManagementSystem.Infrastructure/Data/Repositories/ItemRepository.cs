using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    public class ItemRepository : Repository<Item>, IItemRepository, IQueryableRepository<Item>
    {
        public ItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public IQueryable<Item> Queryable => _dbSet.AsQueryable();

        /// <inheritdoc/>
        public async Task<Item> GetByItemNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            return await _dbSet
                .FirstOrDefaultAsync(i => i.ItemNumber == itemNumber, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByPalletIdAsync(int palletId, CancellationToken cancellationToken = default)
        {
            if (palletId <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(palletId));
            }

            return await _dbSet
                .Where(i => i.PalletId == palletId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByManufacturingOrderAsync(string manufacturingOrder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            return await _dbSet
                .Where(i => i.ManufacturingOrder == manufacturingOrder)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
            {
                throw new ArgumentException("Client code cannot be null or empty", nameof(clientCode));
            }

            return await _dbSet
                .Where(i => i.ClientCode == clientCode)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(clientName));
            }

            return await _dbSet
                .Where(i => i.ClientName.Contains(clientName))
                .ToListAsync(cancellationToken);
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

        /// <inheritdoc/>
        public async Task<Item> GetByItemNumberWithPalletAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            return await _dbSet
                .Include(i => i.Pallet)
                .FirstOrDefaultAsync(i => i.ItemNumber == itemNumber, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> GetNextItemNumberAsync(CancellationToken cancellationToken = default)
        {
            var items = await _dbSet.ToListAsync(cancellationToken);

            if (!items.Any())
            {
                return "100000";
            }

            var maxNumber = items
                .Select(i => i.ItemNumber)
                .Select(i =>
                {
                    if (int.TryParse(i, out int num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(100000)
                .Max();

            return (maxNumber + 1).ToString();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Item>();
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
                .ToListAsync(cancellationToken);
        }
    }
}