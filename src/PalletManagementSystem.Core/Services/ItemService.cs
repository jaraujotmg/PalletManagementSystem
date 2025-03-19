using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IItemRepository _itemRepository;
        private readonly IPalletRepository _palletRepository;
        private readonly ILogger<ItemService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class
        /// </summary>
        /// <param name="itemRepository">The item repository</param>
        /// <param name="palletRepository">The pallet repository</param>
        /// <param name="logger">The logger</param>
        public ItemService(
            IItemRepository itemRepository,
            IPalletRepository palletRepository,
            ILogger<ItemService> logger)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemByIdAsync(int id)
        {
            try
            {
                var item = await _itemRepository.GetByIdAsync(id);
                return item != null ? MapToDto(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with ID {id}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemByNumberAsync(string itemNumber)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            try
            {
                var item = await _itemRepository.GetByItemNumberAsync(itemNumber);
                return item != null ? MapToDto(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with number {itemNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId)
        {
            try
            {
                var items = await _itemRepository.GetByPalletIdAsync(palletId);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for pallet ID {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
            {
                throw new ArgumentException("Client code cannot be null or empty", nameof(clientCode));
            }

            try
            {
                var items = await _itemRepository.GetByClientCodeAsync(clientCode);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for client code {clientCode}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username)
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
                // Get the pallet
                var pallet = await _palletRepository.GetByIdAsync(palletId);
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

                // Save to repository
                var createdItem = await _itemRepository.AddAsync(item);

                return MapToDto(createdItem);
            }
            catch (PalletClosedException pex)
            {
                _logger.LogWarning(pex, $"Attempted to add item to closed pallet {palletId}");
                throw;
            }
            catch (DomainException dex)
            {
                _logger.LogWarning(dex, $"Domain error when creating item for pallet {palletId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating item for pallet {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> UpdateItemAsync(int itemId, decimal weight, decimal width, string quality, string batch)
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
                var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
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

                // Save to repository
                await _itemRepository.UpdateAsync(item);

                return MapToDto(item);
            }
            catch (PalletClosedException pex)
            {
                _logger.LogWarning(pex, $"Attempted to update item on closed pallet, item ID {itemId}");
                throw;
            }
            catch (ItemValidationException ivex)
            {
                _logger.LogWarning(ivex, $"Validation error when updating item {itemId}");
                throw;
            }
            catch (DomainException dex)
            {
                _logger.LogWarning(dex, $"Domain error when updating item {itemId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating item {itemId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> MoveItemToPalletAsync(int itemId, int targetPalletId)
        {
            try
            {
                // Validate if the move is allowed
                bool canMove = await CanMoveItemToPalletAsync(itemId, targetPalletId);
                if (!canMove)
                {
                    throw new DomainException($"Cannot move item {itemId} to pallet {targetPalletId}");
                }

                var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
                var targetPallet = await _palletRepository.GetByIdAsync(targetPalletId);

                // Move item to the target pallet
                item.SetPallet(targetPallet);

                // Save to repository
                await _itemRepository.UpdateAsync(item);

                return MapToDto(item);
            }
            catch (PalletClosedException pex)
            {
                _logger.LogWarning(pex, $"Attempted to move item {itemId} to/from closed pallet {targetPalletId}");
                throw;
            }
            catch (DomainException dex)
            {
                _logger.LogWarning(dex, $"Domain error when moving item {itemId} to pallet {targetPalletId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moving item {itemId} to pallet {targetPalletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanMoveItemToPalletAsync(int itemId, int targetPalletId)
        {
            try
            {
                var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
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
                var targetPallet = await _palletRepository.GetByIdAsync(targetPalletId);
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
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ItemDto>();
            }

            try
            {
                var items = await _itemRepository.SearchAsync(keyword);
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items with keyword '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletAsync(int itemId)
        {
            try
            {
                var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
                return item != null ? MapToDtoWithPallet(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for ID {itemId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            try
            {
                var item = await _itemRepository.GetByItemNumberWithPalletAsync(itemNumber);
                return item != null ? MapToDtoWithPallet(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for number {itemNumber}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
        {
            try
            {
                var items = await _itemRepository.GetAllAsync();
                return items.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all items");
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