using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Infrastructure.Data;

namespace PalletManagementSystem.Core.Services
{
    /// <summary>
    /// Implementation of the item service
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ItemService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class
        /// </summary>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="logger">The logger</param>
        public ItemService(
            IUnitOfWork unitOfWork,
            ILogger<ItemService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the item using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i => i.Id == id)
                    .ProjectToDto()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with ID {id}");

                // Fallback to traditional approach if projection fails
                var item = await _unitOfWork.ItemRepository.GetByIdAsync(id, cancellationToken);
                return ItemMapper.ToDto(item);
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemByNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            try
            {
                // Get the item using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i => i.ItemNumber == itemNumber)
                    .ProjectToDto()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with number {itemNumber}");

                // Fallback to traditional approach if projection fails
                var item = await _unitOfWork.ItemRepository.GetByItemNumberAsync(itemNumber, cancellationToken);
                return ItemMapper.ToDto(item);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the items using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i => i.PalletId == palletId)
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for pallet ID {palletId}");

                // Fallback to traditional approach if projection fails
                var items = await _unitOfWork.ItemRepository.GetByPalletIdAsync(palletId, cancellationToken);
                return ItemMapper.ToDtoList(items);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
            {
                throw new ArgumentException("Client code cannot be null or empty", nameof(clientCode));
            }

            try
            {
                // Get the items using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i => i.ClientCode == clientCode)
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for client code {clientCode}");

                // Fallback to traditional approach if projection fails
                var items = await _unitOfWork.ItemRepository.GetByClientCodeAsync(clientCode, cancellationToken);
                return ItemMapper.ToDtoList(items);
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username, CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (itemDto == null)
            {
                throw new ArgumentNullException(nameof(itemDto));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Get the pallet
                var pallet = await _unitOfWork.PalletRepository.GetByIdAsync(palletId, cancellationToken);
                if (pallet == null)
                {
                    throw new DomainException($"Pallet with ID {palletId} not found");
                }

                if (pallet.IsClosed)
                {
                    throw new PalletClosedException($"Cannot add items to closed pallet {pallet.PalletNumber.Value}");
                }

                // Create a new item
                var item = new Item(
                    itemDto.ItemNumber,
                    itemDto.ManufacturingOrder,
                    itemDto.ManufacturingOrderLine,
                    itemDto.ServiceOrder,
                    itemDto.ServiceOrderLine,
                    itemDto.FinalOrder,
                    itemDto.FinalOrderLine,
                    itemDto.ClientCode,
                    itemDto.ClientName,
                    itemDto.Reference,
                    itemDto.Finish,
                    itemDto.Color,
                    itemDto.Quantity,
                    itemDto.QuantityUnit,
                    itemDto.Weight,
                    itemDto.WeightUnit,
                    itemDto.Width,
                    itemDto.WidthUnit,
                    itemDto.Quality,
                    itemDto.Batch,
                    username);

                // Set the pallet
                item.SetPallet(pallet);

                // Add to repository
                var createdItem = await _unitOfWork.ItemRepository.AddAsync(item, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return ItemMapper.ToDto(createdItem);
            }
            catch (PalletClosedException pex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(pex, $"Attempted to add item to closed pallet {palletId}");
                throw;
            }
            catch (DomainException dex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(dex, $"Domain error when creating item for pallet {palletId}");
                throw;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, $"Error creating item for pallet {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> UpdateItemAsync(
            int itemId, decimal weight, decimal width, string quality, string batch, CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (weight < 0)
            {
                throw new ArgumentException("Weight cannot be negative", nameof(weight));
            }

            if (width < 0)
            {
                throw new ArgumentException("Width cannot be negative", nameof(width));
            }

            if (string.IsNullOrWhiteSpace(quality))
            {
                throw new ArgumentException("Quality cannot be null or empty", nameof(quality));
            }

            if (string.IsNullOrWhiteSpace(batch))
            {
                throw new ArgumentException("Batch cannot be null or empty", nameof(batch));
            }

            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                if (item == null)
                {
                    throw new DomainException($"Item with ID {itemId} not found");
                }

                // Update the editable properties
                try
                {
                    item.Update(weight, width, quality, batch);
                }
                catch (PalletClosedException ex)
                {
                    throw new PalletClosedException($"Cannot update item #{item.ItemNumber}: {ex.Message}");
                }
                catch (ItemValidationException ex)
                {
                    throw new ItemValidationException($"Invalid item data: {ex.Message}");
                }

                // Update in repository
                await _unitOfWork.ItemRepository.UpdateAsync(item, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return ItemMapper.ToDto(item);
            }
            catch (PalletClosedException pex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(pex, $"Attempted to update item on closed pallet, item ID {itemId}");
                throw;
            }
            catch (ItemValidationException ivex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(ivex, $"Validation error when updating item {itemId}");
                throw;
            }
            catch (DomainException dex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(dex, $"Domain error when updating item {itemId}");
                throw;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, $"Error updating item {itemId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> MoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                // Validate if the move is allowed
                bool canMove = await CanMoveItemToPalletAsync(itemId, targetPalletId, cancellationToken);
                if (!canMove)
                {
                    throw new DomainException($"Cannot move item {itemId} to pallet {targetPalletId}");
                }

                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                var targetPallet = await _unitOfWork.PalletRepository.GetByIdAsync(targetPalletId, cancellationToken);

                // Move item to the target pallet
                item.SetPallet(targetPallet);

                // Update in repository
                await _unitOfWork.ItemRepository.UpdateAsync(item, cancellationToken);

                // Save changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return ItemMapper.ToDto(item);
            }
            catch (PalletClosedException pex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(pex, $"Attempted to move item {itemId} to/from closed pallet {targetPalletId}");
                throw;
            }
            catch (DomainException dex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogWarning(dex, $"Domain error when moving item {itemId} to pallet {targetPalletId}");
                throw;
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                _logger.LogError(ex, $"Error moving item {itemId} to pallet {targetPalletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanMoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                if (item == null)
                {
                    return false;
                }

                // Check if source pallet is closed
                if (item.Pallet != null && item.Pallet.IsClosed)
                {
                    return false;
                }

                // Check if target pallet exists and is not closed
                var targetPallet = await _unitOfWork.PalletRepository.GetByIdAsync(targetPalletId, cancellationToken);
                if (targetPallet == null || targetPallet.IsClosed)
                {
                    return false;
                }

                // Cannot move to the same pallet
                if (item.PalletId == targetPalletId)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if item {itemId} can be moved to pallet {targetPalletId}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ItemDto>();
            }

            try
            {
                // Use direct projection with appropriate query
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i =>
                        i.ItemNumber.Contains(keyword) ||
                        i.ManufacturingOrder.Contains(keyword) ||
                        i.ServiceOrder.Contains(keyword) ||
                        i.FinalOrder.Contains(keyword) ||
                        i.ClientCode.Contains(keyword) ||
                        i.ClientName.Contains(keyword) ||
                        i.Reference.Contains(keyword) ||
                        i.Batch.Contains(keyword))
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items with keyword '{keyword}'");

                // Fallback to traditional approach
                var items = await _unitOfWork.ItemRepository.SearchAsync(keyword, cancellationToken);
                return ItemMapper.ToDtoList(items);
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletAsync(int itemId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the item with pallet using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i => i.Id == itemId)
                    .ProjectToDtoWithPallet()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for ID {itemId}");

                // Fallback to traditional approach if projection fails
                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                return ItemMapper.ToDtoWithPallet(item);
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            try
            {
                // Get the item with pallet using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i => i.ItemNumber == itemNumber)
                    .ProjectToDtoWithPallet()
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for number {itemNumber}");

                // Fallback to traditional approach if projection fails
                var item = await _unitOfWork.ItemRepository.GetByItemNumberWithPalletAsync(itemNumber, cancellationToken);
                return ItemMapper.ToDtoWithPallet(item);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Get all items using projection
                return await DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .ProjectToDto()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all items");

                // Fallback to traditional approach if projection fails
                var items = await _unitOfWork.ItemRepository.GetAllAsync(cancellationToken);
                return ItemMapper.ToDtoList(items);
            }
        }

        /// <inheritdoc/>
        public async Task<PagedResultDto<ItemDto>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            bool includePallets = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Build the query
                var query = DbContextAccessor.CreateQuery<Item>(_unitOfWork);

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
                    // Search in relevant fields
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

                // Apply ordering - newest first
                query = query.OrderByDescending(i => i.CreatedDate);

                // Use projection to get paged results
                return await query.ProjectToPagedResultAsync(pageNumber, pageSize, includePallets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged items");

                // Fallback to traditional approach
                var pagedResult = await _unitOfWork.ItemRepository.GetPagedItemsAsync(
                    pageNumber,
                    pageSize,
                    palletId,
                    clientCode,
                    manufacturingOrder,
                    keyword,
                    includePallets,
                    orderByCreatedDate: true,
                    descending: true,
                    cancellationToken);

                // Map results
                var itemDtos = includePallets
                    ? pagedResult.Items.Select(ItemMapper.ToDtoWithPallet).ToList()
                    : pagedResult.Items.Select(ItemMapper.ToDto).ToList();

                // Create DTO for paged result
                return new PagedResultDto<ItemDto>
                {
                    Items = itemDtos,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };
            }
        }
    }
}