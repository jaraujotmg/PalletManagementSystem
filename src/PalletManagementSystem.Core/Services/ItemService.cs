using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemService"/> class
        /// </summary>
        /// <param name="itemRepository">The item repository</param>
        /// <param name="palletRepository">The pallet repository</param>
        public ItemService(IItemRepository itemRepository, IPalletRepository palletRepository)
        {
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemByIdAsync(int id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            return item != null ? MapToDto(item) : null;
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemByNumberAsync(string itemNumber)
        {
            var item = await _itemRepository.GetByItemNumberAsync(itemNumber);
            return item != null ? MapToDto(item) : null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId)
        {
            var items = await _itemRepository.GetByPalletIdAsync(palletId);
            return items.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode)
        {
            var items = await _itemRepository.GetByClientCodeAsync(clientCode);
            return items.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username)
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

        /// <inheritdoc/>
        public async Task<ItemDto> UpdateItemAsync(int itemId, decimal weight, decimal width, string quality, string batch)
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

        /// <inheritdoc/>
        public async Task<ItemDto> MoveItemToPalletAsync(int itemId, int targetPalletId)
        {
            var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
            if (item == null)
            {
                throw new DomainException($"Item with ID {itemId} not found");
            }

            var targetPallet = await _palletRepository.GetByIdAsync(targetPalletId);
            if (targetPallet == null)
            {
                throw new DomainException($"Target pallet with ID {targetPalletId} not found");
            }

            // Move item to the target pallet
            try
            {
                item.SetPallet(targetPallet);
            }
            catch (PalletClosedException ex)
            {
                throw new PalletClosedException($"Cannot move item #{item.ItemNumber}: {ex.Message}");
            }

            // Save to repository
            await _itemRepository.UpdateAsync(item);

            return MapToDto(item);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword)
        {
            var items = await _itemRepository.SearchAsync(keyword);
            return items.Select(MapToDto);
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletAsync(int itemId)
        {
            var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
            return item != null ? MapToDtoWithPallet(item) : null;
        }

        /// <inheritdoc/>
        public async Task<ItemDto> GetItemWithPalletByNumberAsync(string itemNumber)
        {
            var item = await _itemRepository.GetByItemNumberWithPalletAsync(itemNumber);
            return item != null ? MapToDtoWithPallet(item) : null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync()
        {
            var items = await _itemRepository.GetAllAsync();
            return items.Select(MapToDto);
        }

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
    }
}