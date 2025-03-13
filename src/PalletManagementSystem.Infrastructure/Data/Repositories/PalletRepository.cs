using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Implementation of the pallet repository
    /// </summary>
    public class PalletRepository : Repository<Pallet>, IPalletRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PalletRepository"/> class
        /// </summary>
        /// <param name="context">The database context</param>
        public PalletRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByPalletNumberAsync(string palletNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformAsync(Division division, Platform platform)
        {
            return await _dbSet
                .Where(p => p.Division == division && p.Platform == platform)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByManufacturingOrderAsync(string manufacturingOrder)
        {
            return await _dbSet
                .Where(p => p.ManufacturingOrder == manufacturingOrder)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByStatusAsync(bool isClosed)
        {
            return await _dbSet
                .Where(p => p.IsClosed == isClosed)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndStatusAsync(Division division, bool isClosed)
        {
            return await _dbSet
                .Where(p => p.Division == division && p.IsClosed == isClosed)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetAllWithItemsAsync()
        {
            return await _dbSet
                .Include(p => p.Items)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByIdWithItemsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByPalletNumberWithItemsAsync(string palletNumber)
        {
            return await _dbSet
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber);
        }

        /// <inheritdoc/>
        public async Task<int> GetNextTemporarySequenceNumberAsync()
        {
            // Find the highest temporary sequence number and add 1
            var tempPallets = await _dbSet
                .Where(p => EF.Property<bool>(p, "_isTemporaryPalletNumber"))
                .ToListAsync();

            // If no temporary pallets exist, start with 1
            if (!tempPallets.Any())
            {
                return 1;
            }

            // Extract numbers from TEMP-XXX format
            var maxNumber = tempPallets
                .Select(p => EF.Property<string>(p, "_palletNumberValue"))
                .Where(p => p.StartsWith("TEMP-", StringComparison.OrdinalIgnoreCase))
                .Select(p =>
                {
                    if (int.TryParse(p.Substring(5), out int num))
                        return num;
                    return 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return maxNumber + 1;
        }

        /// <inheritdoc/>
        public async Task<int> GetNextPermanentSequenceNumberAsync(Division division)
        {
            // Find the highest permanent sequence number for this division and add 1
            var divisionPallets = await _dbSet
                .Where(p => p.Division == division && !EF.Property<bool>(p, "_isTemporaryPalletNumber"))
                .ToListAsync();

            // If no permanent pallets exist for this division, start with 1
            if (!divisionPallets.Any())
            {
                return 1;
            }

            // Extract numbers based on division-specific format
            var maxNumber = 0;
            var palletNumbers = divisionPallets
                .Select(p => EF.Property<string>(p, "_palletNumberValue"));

            switch (division)
            {
                case Division.MA:
                    // Format: P8XXXXX
                    maxNumber = palletNumbers
                        .Where(p => p.StartsWith("P8"))
                        .Select(p =>
                        {
                            if (int.TryParse(p.Substring(2), out int num))
                                return num;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();
                    break;

                case Division.TC:
                    // Format: 47XXXXX
                    maxNumber = palletNumbers
                        .Where(p => p.StartsWith("47"))
                        .Select(p =>
                        {
                            if (int.TryParse(p.Substring(2), out int num))
                                return num;
                            return 0;
                        })
                        .DefaultIfEmpty(0)
                        .Max();
                    break;

                default:
                    throw new ArgumentException($"Unsupported division: {division}");
            }

            return maxNumber + 1;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Pallet>();

            keyword = keyword.Trim();

            return await _dbSet
                .Where(p =>
                    EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword)
                )
                .ToListAsync();
        }
    }
}