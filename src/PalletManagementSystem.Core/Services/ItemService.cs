using System;
using System.Collections.Generic;
using System.Data;
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
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IItemRepository _itemRepository;
        private readonly IPalletRepository _palletRepository;
        private readonly IPrinterService _printerService;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<ItemService> _logger;

        public ItemService(
            IUnitOfWork unitOfWork,
            IItemRepository itemRepository,
            IPalletRepository palletRepository,
            IPrinterService printerService,
            ITransactionManager transactionManager,
            ILogger<ItemService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ItemListDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _itemRepository.GetItemListByIdAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with ID {id}");
                throw;
            }
        }

        public async Task<ItemDetailDto> GetItemDetailByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _itemRepository.GetItemDetailByIdAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item detail with ID {id}");
                throw;
            }
        }

        public async Task<ItemListDto> GetItemByNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _itemRepository.GetItemListByNumberAsync(itemNumber, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with number {itemNumber}");
                throw;
            }
        }

        public async Task<IEnumerable<ItemListDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _itemRepository.GetItemsByPalletIdAsync(palletId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for pallet ID {palletId}");
                throw;
            }
        }

        public async Task<ItemDto> CreateItemAsync(ItemDto itemDto, int palletId, string username, CancellationToken cancellationToken = default)
        {
            return await _transactionManager.ExecuteInTransactionWithIsolationAsync(async (token) =>
            {
                var pallet = await _unitOfWork.PalletRepository.GetByIdAsync(palletId, token);
                if (pallet == null)
                {
                    throw new DomainException($"Pallet with ID {palletId} not found");
                }

                if (pallet.IsClosed)
                {
                    throw new PalletClosedException($"Cannot add items to closed pallet {pallet.PalletNumber.Value}");
                }

                // Generate item number if not provided
                if (string.IsNullOrWhiteSpace(itemDto.ItemNumber))
                {
                    itemDto.ItemNumber = await _itemRepository.GetNextItemNumberAsync(token);
                }

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

                // Assign to pallet
                item.AssignToPallet(pallet);

                await _unitOfWork.ItemRepository.AddAsync(item, token);

                // Return the DTO
                return itemDto;
            }, cancellationToken, IsolationLevel.ReadCommitted);
        }

        public async Task<ItemDetailDto> UpdateItemAsync(int itemId, UpdateItemDto updateDto, CancellationToken cancellationToken = default)
        {
            return await _transactionManager.ExecuteInTransactionWithIsolationAsync(async (token) =>
            {
                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, token);
                if (item == null)
                {
                    throw new DomainException($"Item with ID {itemId} not found");
                }

                item.Update(
                    updateDto.Weight,
                    updateDto.Width,
                    updateDto.Quality,
                    updateDto.Batch);

                await _unitOfWork.ItemRepository.UpdateAsync(item, token);

                // Return the updated item
                return await _itemRepository.GetItemDetailByIdAsync(itemId, token);
            }, cancellationToken, IsolationLevel.ReadCommitted);
        }

        public async Task<ItemDetailDto> MoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default)
        {
            // Use serializable isolation level for moving items to prevent conflicts
            return await _transactionManager.ExecuteInTransactionWithIsolationAsync(async (token) =>
            {
                bool canMove = await CanMoveItemToPalletAsync(itemId, targetPalletId, token);
                if (!canMove)
                {
                    throw new DomainException($"Cannot move item {itemId} to pallet {targetPalletId}");
                }

                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, token);
                var targetPallet = await _unitOfWork.PalletRepository.GetByIdAsync(targetPalletId, token);

                item.MoveToPallet(targetPallet);

                await _unitOfWork.ItemRepository.UpdateAsync(item, token);

                // Return the updated item
                return await _itemRepository.GetItemDetailByIdAsync(itemId, token);
            }, cancellationToken, IsolationLevel.Serializable);
        }

        public async Task<bool> CanMoveItemToPalletAsync(int itemId, int targetPalletId, CancellationToken cancellationToken = default)
        {
            try
            {
                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                if (item == null)
                {
                    return false;
                }

                if (item.Pallet != null && item.Pallet.IsClosed)
                {
                    return false;
                }

                var targetPallet = await _unitOfWork.PalletRepository.GetByIdAsync(targetPalletId, cancellationToken);
                if (targetPallet == null || targetPallet.IsClosed)
                {
                    return false;
                }

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

        public async Task<IEnumerable<ItemListDto>> SearchItemsAsync(string keyword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ItemListDto>();
            }

            try
            {
                return await _itemRepository.SearchItemsAsync(keyword, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items with keyword '{keyword}'");
                throw;
            }
        }

        public async Task<PagedResultDto<ItemListDto>> GetPagedItemsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _itemRepository.GetPagedItemsAsync(
                    pageNumber,
                    pageSize,
                    palletId,
                    clientCode,
                    manufacturingOrder,
                    keyword,
                    true,
                    true,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged items");
                throw;
            }
        }

        public async Task<PagedResultDto<ItemDetailDto>> GetPagedItemDetailsAsync(
            int pageNumber,
            int pageSize,
            int? palletId = null,
            string clientCode = null,
            string manufacturingOrder = null,
            string keyword = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _itemRepository.GetPagedItemDetailsAsync(
                    pageNumber,
                    pageSize,
                    palletId,
                    clientCode,
                    manufacturingOrder,
                    keyword,
                    true,
                    true,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged item details");
                throw;
            }
        }
    }
}