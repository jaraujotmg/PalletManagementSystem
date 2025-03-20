using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Extensions;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Services
{
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueryService _queryService;
        private readonly IPrinterService _printerService;
        private readonly ILogger<ItemService> _logger;

        public ItemService(
            IUnitOfWork unitOfWork,
            IQueryService queryService,
            IPrinterService printerService,
            ILogger<ItemService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ItemDto> GetItemByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid item ID", nameof(id));
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.Id == id);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    false,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with ID {id}");
                throw;
            }
        }

        public async Task<ItemDto> GetItemByNumberAsync(string itemNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.ItemNumber == itemNumber);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    false,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with number {itemNumber}");
                throw;
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItemsByPalletIdAsync(int palletId, CancellationToken cancellationToken = default)
        {
            if (palletId <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(palletId));
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.PalletId == palletId);

                return await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    false,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for pallet ID {palletId}");
                throw;
            }
        }

        public async Task<IEnumerable<ItemDto>> GetItemsByClientCodeAsync(string clientCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientCode))
            {
                throw new ArgumentException("Client code cannot be null or empty", nameof(clientCode));
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.ClientCode == clientCode);

                return await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    false,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving items for client code {clientCode}");
                throw;
            }
        }

        public async Task<ItemDto> CreateItemAsync(
            ItemDto itemDto,
            int palletId,
            string username,
            CancellationToken cancellationToken = default)
        {
            if (itemDto == null)
            {
                throw new ArgumentNullException(nameof(itemDto));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (palletId <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(palletId));
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var pallet = await _unitOfWork.PalletRepository.GetByIdAsync(palletId, cancellationToken);
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
                    itemDto.ItemNumber = await _unitOfWork.ItemRepository.GetNextItemNumberAsync(cancellationToken);
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

                item.SetPallet(pallet);

                var createdItem = await _unitOfWork.ItemRepository.AddAsync(item, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.Id == createdItem.Id);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, $"Error creating item for pallet {palletId}");
                throw;
            }
        }

        public async Task<ItemDto> UpdateItemAsync(
            int itemId,
            decimal weight,
            decimal width,
            string quality,
            string batch,
            CancellationToken cancellationToken = default)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("Invalid item ID", nameof(itemId));
            }

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
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                if (item == null)
                {
                    throw new DomainException($"Item with ID {itemId} not found");
                }

                item.Update(weight, width, quality, batch);

                await _unitOfWork.ItemRepository.UpdateAsync(item, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.Id == itemId);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (PalletClosedException pex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning(pex, $"Attempted to update item on closed pallet, item ID {itemId}");
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, $"Error updating item {itemId}");
                throw;
            }
        }

        public async Task<ItemDto> MoveItemToPalletAsync(
            int itemId,
            int targetPalletId,
            CancellationToken cancellationToken = default)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("Invalid item ID", nameof(itemId));
            }

            if (targetPalletId <= 0)
            {
                throw new ArgumentException("Invalid target pallet ID", nameof(targetPalletId));
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                bool canMove = await CanMoveItemToPalletAsync(itemId, targetPalletId, cancellationToken);
                if (!canMove)
                {
                    throw new DomainException($"Cannot move item {itemId} to pallet {targetPalletId}");
                }

                var item = await _unitOfWork.ItemRepository.GetByIdWithPalletAsync(itemId, cancellationToken);
                var targetPallet = await _unitOfWork.PalletRepository.GetByIdAsync(targetPalletId, cancellationToken);

                item.SetPallet(targetPallet);

                await _unitOfWork.ItemRepository.UpdateAsync(item, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.Id == itemId);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (PalletClosedException pex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning(pex, $"Attempted to move item {itemId} to/from closed pallet {targetPalletId}");
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, $"Error moving item {itemId} to pallet {targetPalletId}");
                throw;
            }
        }

        public async Task<bool> CanMoveItemToPalletAsync(
            int itemId,
            int targetPalletId,
            CancellationToken cancellationToken = default)
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

        // Continuing the ItemService class from the previous implementation

        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(
            string keyword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ItemDto>();
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i =>
                        i.ItemNumber.Contains(keyword) ||
                        i.ManufacturingOrder.Contains(keyword) ||
                        i.ServiceOrder.Contains(keyword) ||
                        i.FinalOrder.Contains(keyword) ||
                        i.ClientCode.Contains(keyword) ||
                        i.ClientName.Contains(keyword) ||
                        i.Reference.Contains(keyword) ||
                        i.Batch.Contains(keyword));

                return await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    false,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items with keyword '{keyword}'");
                throw;
            }
        }

        public async Task<ItemDto> GetItemWithPalletAsync(
            int itemId,
            CancellationToken cancellationToken = default)
        {
            if (itemId <= 0)
            {
                throw new ArgumentException("Invalid item ID", nameof(itemId));
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.Id == itemId);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for ID {itemId}");
                throw;
            }
        }

        public async Task<ItemDto> GetItemWithPalletByNumberAsync(
            string itemNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            try
            {
                var query = _unitOfWork.ItemRepository
                    .AsQueryable()
                    .Where(i => i.ItemNumber == itemNumber);

                var results = await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item with pallet for number {itemNumber}");
                throw;
            }
        }

        public async Task<IEnumerable<ItemDto>> GetAllItemsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _unitOfWork.ItemRepository.AsQueryable();

                return await _queryService.ProjectToDtoAsync<Item, ItemDto>(
                    query,
                    false,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all items");
                throw;
            }
        }

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
            if (pageNumber < 1)
            {
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
            }

            try
            {
                var query = _unitOfWork.ItemRepository.AsQueryable();

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

                // Order by created date descending
                query = query.OrderByDescending(i => i.CreatedDate);

                return await _queryService.ProjectToPagedResultAsync<Item, ItemDto>(
                    query,
                    pageNumber,
                    pageSize,
                    includePallets,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged items");
                throw;
            }
        }
    }
}