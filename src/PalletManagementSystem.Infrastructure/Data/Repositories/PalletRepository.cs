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
        public override async Task<Pallet> GetByIdAsync(int id)
        {
            var pallet = await _dbSet.FindAsync(id);
            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }
            return pallet;
        }

        /// <inheritdoc/>
        public override async Task<IReadOnlyList<Pallet>> GetAllAsync()
        {
            var pallets = await base.GetAllAsync();
            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }
            return pallets;
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByPalletNumberAsync(string palletNumber)
        {
            var pallet = await _dbSet
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber);

            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallet;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformAsync(Division division, Platform platform)
        {
            var pallets = await _dbSet
                .Where(p => p.Division == division && p.Platform == platform)
                .ToListAsync();

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByManufacturingOrderAsync(string manufacturingOrder)
        {
            var pallets = await _dbSet
                .Where(p => p.ManufacturingOrder == manufacturingOrder)
                .ToListAsync();

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByStatusAsync(bool isClosed)
        {
            var pallets = await _dbSet
                .Where(p => p.IsClosed == isClosed)
                .ToListAsync();

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndStatusAsync(Division division, bool isClosed)
        {
            var pallets = await _dbSet
                .Where(p => p.Division == division && p.IsClosed == isClosed)
                .ToListAsync();

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetAllWithItemsAsync()
        {
            var pallets = await _dbSet
                .Include(p => p.Items)
                .ToListAsync();

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByIdWithItemsAsync(int id)
        {
            var pallet = await _dbSet
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallet;
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByPalletNumberWithItemsAsync(string palletNumber)
        {
            var pallet = await _dbSet
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber);

            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallet;
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

            var pallets = await _dbSet
                .Where(p =>
                    EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword)
                )
                .ToListAsync();

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public override async Task<Pallet> AddAsync(Pallet entity)
        {
            // Save the PalletNumber value object to shadow properties
            _context.Entry(entity).Property("_palletNumberValue").CurrentValue = entity.PalletNumber.Value;
            _context.Entry(entity).Property("_isTemporaryPalletNumber").CurrentValue = entity.PalletNumber.IsTemporary;
            _context.Entry(entity).Property("_palletNumberDivision").CurrentValue = entity.PalletNumber.Division;

            return await base.AddAsync(entity);
        }

        /// <inheritdoc/>
        public override async Task UpdateAsync(Pallet entity)
        {
            // Save the PalletNumber value object to shadow properties
            _context.Entry(entity).Property("_palletNumberValue").CurrentValue = entity.PalletNumber.Value;
            _context.Entry(entity).Property("_isTemporaryPalletNumber").CurrentValue = entity.PalletNumber.IsTemporary;
            _context.Entry(entity).Property("_palletNumberDivision").CurrentValue = entity.PalletNumber.Division;

            await base.UpdateAsync(entity);
        }

        /// <summary>
        /// Reconstructs the PalletNumber value object from shadow properties
        /// </summary>
        /// <param name="pallet">The pallet entity</param>
        private void ReconstructPalletNumber(Pallet pallet)
        {
            // Access shadow properties using Entry API
            var entry = _context.Entry(pallet);

            var palletNumberValue = entry.Property<string>("_palletNumberValue").CurrentValue;
            var isTemporary = entry.Property<bool>("_isTemporaryPalletNumber").CurrentValue;
            var division = entry.Property<Division>("_palletNumberDivision").CurrentValue;

            // Use reflection to set the private PalletNumber field
            var palletNumberField = typeof(Pallet).GetField("_palletNumber",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (palletNumberField != null)
            {
                var palletNumber = new PalletNumber(palletNumberValue, isTemporary, division);
                palletNumberField.SetValue(pallet, palletNumber);
            }
        }
    }
}