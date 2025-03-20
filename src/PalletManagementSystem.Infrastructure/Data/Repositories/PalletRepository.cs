using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        public override async Task<Pallet> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var pallet = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }
            return pallet;
        }

        /// <inheritdoc/>
        public override async Task<IReadOnlyList<Pallet>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var pallets = await base.GetAllAsync(cancellationToken);
            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }
            return pallets;
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByPalletNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            var pallet = await _dbSet
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber, cancellationToken);

            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallet;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformAsync(Division division, Platform platform, CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Where(p => p.Division == division && p.Platform == platform)
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndPlatformWithItemsAsync(Division division, Platform platform, CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Include(p => p.Items)
                .Where(p => p.Division == division && p.Platform == platform)
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByManufacturingOrderAsync(string manufacturingOrder, CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Where(p => p.ManufacturingOrder == manufacturingOrder)
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByStatusAsync(bool isClosed, CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Where(p => p.IsClosed == isClosed)
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetByDivisionAndStatusAsync(Division division, bool isClosed, CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Where(p => p.Division == division && p.IsClosed == isClosed)
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> GetAllWithItemsAsync(CancellationToken cancellationToken = default)
        {
            var pallets = await _dbSet
                .Include(p => p.Items)
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken = default)
        {
            var pallet = await _dbSet
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallet;
        }

        /// <inheritdoc/>
        public async Task<Pallet> GetByPalletNumberWithItemsAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            var pallet = await _dbSet
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => EF.Property<string>(p, "_palletNumberValue") == palletNumber, cancellationToken);

            if (pallet != null)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallet;
        }

        /// <inheritdoc/>
        public async Task<int> GetNextTemporarySequenceNumberAsync(CancellationToken cancellationToken = default)
        {
            // Find the highest temporary sequence number and add 1
            var tempPallets = await _dbSet
                .Where(p => EF.Property<bool>(p, "_isTemporaryPalletNumber"))
                .ToListAsync(cancellationToken);

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
        public async Task<int> GetNextPermanentSequenceNumberAsync(Division division, CancellationToken cancellationToken = default)
        {
            // Find the highest permanent sequence number for this division and add 1
            var divisionPallets = await _dbSet
                .Where(p => p.Division == division && !EF.Property<bool>(p, "_isTemporaryPalletNumber"))
                .ToListAsync(cancellationToken);

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
                    throw new ArgumentException($"Unsupported division: {division}", nameof(division));
            }

            return maxNumber + 1;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Pallet>> SearchAsync(string keyword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Pallet>();

            keyword = keyword.Trim();

            var pallets = await _dbSet
                .Where(p =>
                    EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword)
                )
                .ToListAsync(cancellationToken);

            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return pallets;
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Pallet>> GetPagedPalletsAsync(
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
            // Validate parameters
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than or equal to 1", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than or equal to 1", nameof(pageSize));

            // Build query
            var query = _dbSet.AsQueryable();

            // Apply filters
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
                // Search in pallet number and manufacturing order
                query = query.Where(p =>
                    EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                    p.ManufacturingOrder.Contains(keyword));
            }

            // Apply ordering
            if (orderByCreatedDate)
            {
                query = descending
                    ? query.OrderByDescending(p => p.CreatedDate)
                    : query.OrderBy(p => p.CreatedDate);
            }
            else
            {
                query = descending
                    ? query.OrderByDescending(p => EF.Property<string>(p, "_palletNumberValue"))
                    : query.OrderBy(p => EF.Property<string>(p, "_palletNumberValue"));
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply paging
            var skip = (pageNumber - 1) * pageSize;
            var pallets = await query
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Reconstruct pallet numbers
            foreach (var pallet in pallets)
            {
                ReconstructPalletNumber(pallet);
            }

            return new PagedResult<Pallet>(pallets, totalCount, pageNumber, pageSize);
        }

        /// <inheritdoc/>
        public override async Task<Pallet> AddAsync(Pallet entity, CancellationToken cancellationToken = default)
        {
            // Save the PalletNumber value object to shadow properties
            _context.Entry(entity).Property("_palletNumberValue").CurrentValue = entity.PalletNumber.Value;
            _context.Entry(entity).Property("_isTemporaryPalletNumber").CurrentValue = entity.PalletNumber.IsTemporary;
            _context.Entry(entity).Property("_palletNumberDivision").CurrentValue = entity.PalletNumber.Division;

            return await base.AddAsync(entity, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task UpdateAsync(Pallet entity, CancellationToken cancellationToken = default)
        {
            // Save the PalletNumber value object to shadow properties
            _context.Entry(entity).Property("_palletNumberValue").CurrentValue = entity.PalletNumber.Value;
            _context.Entry(entity).Property("_isTemporaryPalletNumber").CurrentValue = entity.PalletNumber.IsTemporary;
            _context.Entry(entity).Property("_palletNumberDivision").CurrentValue = entity.PalletNumber.Division;

            await base.UpdateAsync(entity, cancellationToken);
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