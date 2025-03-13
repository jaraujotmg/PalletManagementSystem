using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Implementation of the item repository
    /// </summary>
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public ItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Item> GetByItemNumberAsync(string itemNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(i => i.ItemNumber == itemNumber);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByPalletIdAsync(int palletId)
        {
            return await _dbSet
                .Where(i => i.PalletId == palletId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByManufacturingOrderAsync(string manufacturingOrder)
        {
            return await _dbSet
                .Where(i => i.ManufacturingOrder == manufacturingOrder)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByClientCodeAsync(string clientCode)
        {
            return await _dbSet
                .Where(i => i.ClientCode == clientCode)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByClientNameAsync(string clientName)
        {
            return await _dbSet
                .Where(i => i.ClientName.Contains(clientName))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetByReferenceAsync(string reference)
        {
            return await _dbSet
                .Where(i => i.Reference == reference)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> GetAllWithPalletsAsync()
        {
            return await _dbSet
                .Include(i => i.Pallet)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Item> GetByIdWithPalletAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Pallet)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Item> GetByItemNumberWithPalletAsync(string itemNumber)
        {
            return await _dbSet
                .Include(i => i.Pallet)
                .FirstOrDefaultAsync(i => i.ItemNumber == itemNumber);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Item>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Item>();

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
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GetNextItemNumberAsync()
        {
            // Find the highest item number and add 1
            var items = await _dbSet.ToListAsync();

            // If no items exist, start with 100000
            if (!items.Any())
            {
                return "100000";
            }

            // Extract numbers from item numbers assuming they are numeric
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
    }
}