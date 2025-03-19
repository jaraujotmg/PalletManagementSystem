using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models;

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
                var item = await _unitOfWork.ItemRepository.GetByIdAsync(id, cancellationToken);
                return item != null ? MapToDto(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with ID {id}");
                throw;
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
                var item = await _unitOfWork.ItemRepository.GetByItemNumberAsync(itemNumber, cancellationToken);
                return item != null ? MapToDto(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with number {itemNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await _unitOfWork.ItemRepository.GetByPalletIdAsync(palletId, cancellationToken);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for pallet ID {palletId}");
                throw;
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
                var items = await _unitOfWork.ItemRepository.GetByClientCodeAsync(clientCode, cancellationToken);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for client code {clientCode}");
                throw;
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

                return MapToDto(createdItem);
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

                return MapToDto(item);
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

                return MapToDto(item);
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
                var items = await _unitOfWork.ItemRepository.SearchAsync(keyword, cancellationToken);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items with keyword '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletAsync(int itemId, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                return item != null ? MapToDtoWithPallet(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for ID {itemId}");
                throw;
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
                var item = await _unitOfWork.ItemRepository.GetByItemNumberWithPalletAsync(itemNumber, cancellationToken);
                return item != null ? MapToDtoWithPallet(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for number {itemNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var items = await _unitOfWork.ItemRepository.GetAllAsync(cancellationToken);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all items");
                throw;
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

                // Map to DTOs
                var itemDtos = includePallets
                    ? pagedResult.Items.Select(MapToDtoWithPallet).ToList()
                    : pagedResult.Items.Select(MapToDto).ToList();

                // Create DTO for paged result
                var pagedResultDto = new PagedResultDto<ItemDto>
                {
                    Items = itemDtos,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                return pagedResultDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged items");
                throw;
            }
        }

        #region Mapping Methods

        /// <summary>
        /// Maps an Item entity to an ItemDto
        /// </summary>
        /// <param name="item">The item entity</param>
        /// <returns>The item DTO</returns>
        private static ItemDto MapToDto(Item item)
        {
            return new ItemDto
            {
                Id = item.Id,
                ItemNumber = item.ItemNumber,
                PalletId = item.PalletId,
                ManufacturingOrder = item.ManufacturingOrder,
                ManufacturingOrderLine = item.ManufacturingOrderLine,
                ServiceOrder = item.ServiceOrder,
                ServiceOrderLine = item.ServiceOrderLine,
                FinalOrder = item.FinalOrder,
                FinalOrderLine = item.FinalOrderLine,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                Reference = item.Reference,
                Finish = item.Finish,
                Color = item.Color,
                Quantity = item.Quantity,
                QuantityUnit = item.QuantityUnit,
                Weight = item.Weight,
                WeightUnit = item.WeightUnit,
                Width = item.Width,
                WidthUnit = item.WidthUnit,
                Quality = item.Quality,
                Batch = item.Batch,
                CreatedDate = item.CreatedDate,
                CreatedBy = item.CreatedBy
            };
        }

        /// <summary>
        /// Maps an Item entity to an ItemDto including its pallet
        /// </summary>
        /// <param name="item">The item entity</param>
        /// <returns>The item DTO with pallet</returns>
        private static ItemDto MapToDtoWithPallet(Item item)
        {
            var dto = MapToDto(item);

            // Add pallet if exists
            if (item.Pallet != null)
            {
                dto.Pallet = new PalletDto
                {
                    Id = item.Pallet.Id,
                    PalletNumber = item.Pallet.PalletNumber.Value,
                    IsTemporary = item.Pallet.PalletNumber.IsTemporary,
                    ManufacturingOrder = item.Pallet.ManufacturingOrder,
                    Division = item.Pallet.Division.ToString(),
                    Platform = item.Pallet.Platform.ToString(),
                    UnitOfMeasure = item.Pallet.UnitOfMeasure.ToString(),
                    Quantity = item.Pallet.Quantity,
                    ItemCount = item.Pallet.ItemCount,
                    IsClosed = item.Pallet.IsClosed,
                    CreatedDate = item.Pallet.CreatedDate,
                    ClosedDate = item.Pallet.ClosedDate,
                    CreatedBy = item.Pallet.CreatedBy
                };
            }

            return dto;
        }

        #endregion
    }
}