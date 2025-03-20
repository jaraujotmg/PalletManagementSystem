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
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

namespace PalletManagementSystem.Core.Services
{
    public class PalletService : IPalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueryService _queryService;
        private readonly IPrinterService _printerService;
        private readonly IPlatformValidationService _platformValidationService;
        private readonly ILogger<PalletService> _logger;

        public PalletService(
            IUnitOfWork unitOfWork,
            IQueryService queryService,
            IPrinterService printerService,
            IPlatformValidationService platformValidationService,
            ILogger<PalletService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PalletDto> GetPalletByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(id));
            }

            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.Id == id);

                var results = await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    false,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with ID {id}");
                throw;
            }
        }

        public async Task<PalletDto> GetPalletByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.PalletNumber.Value == palletNumber);

                var results = await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    false,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with number {palletNumber}");
                throw;
            }
        }

        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
            if (!isValid)
            {
                throw new ArgumentException($"Platform {platform} is not valid for division {division}");
            }

            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.Division == division && p.Platform == platform);

                return await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    includeItems,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} and platform {platform}");
                throw;
            }
        }

        public async Task<IEnumerable<PalletDto>> GetPalletsByStatusAsync(
            bool isClosed,
            bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.IsClosed == isClosed);

                return await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    includeItems,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets with status {isClosed}");
                throw;
            }
        }

        public async Task<IEnumerable<PalletDto>> GetPalletsByDivisionAndStatusAsync(
            Division division,
            bool isClosed,
            bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.Division == division && p.IsClosed == isClosed);

                return await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    includeItems,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallets for division {division} with status {isClosed}");
                throw;
            }
        }

        public async Task<PalletDto> CreatePalletAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
            if (!isValid)
            {
                throw new ArgumentException($"Platform {platform} is not valid for division {division}");
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                int sequenceNumber = await _unitOfWork.PalletRepository.GetNextTemporarySequenceNumberAsync(cancellationToken);
                var palletNumber = PalletNumber.CreateTemporary(sequenceNumber, division);

                var pallet = new Pallet(
                    palletNumber,
                    manufacturingOrder,
                    division,
                    platform,
                    unitOfMeasure,
                    username);

                var createdPallet = await _unitOfWork.PalletRepository.AddAsync(pallet, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.Id == createdPallet.Id);

                var results = await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    false,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, $"Error creating pallet for manufacturing order {manufacturingOrder}");
                throw;
            }
        }

        public async Task<PalletDto> ClosePalletAsync(
            int palletId,
            bool autoPrint = true,
            string notes = null,
            CancellationToken cancellationToken = default)
        {
            if (palletId <= 0)
            {
                throw new ArgumentException("Invalid pallet ID", nameof(palletId));
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var pallet = await _unitOfWork.PalletRepository.GetByIdWithItemsAsync(palletId, cancellationToken);
                if (pallet == null)
                {
                    throw new DomainException($"Pallet with ID {palletId} not found");
                }

                if (pallet.IsClosed)
                {
                    throw new PalletClosedException($"Pallet {pallet.PalletNumber.Value} is already closed");
                }

                PalletNumber permanentNumber = null;
                if (pallet.PalletNumber.IsTemporary)
                {
                    int sequenceNumber = await _unitOfWork.PalletRepository.GetNextPermanentSequenceNumberAsync(pallet.Division, cancellationToken);
                    permanentNumber = PalletNumber.CreatePermanent(sequenceNumber, pallet.Division);
                }

                pallet.Close(permanentNumber);

                await _unitOfWork.PalletRepository.UpdateAsync(pallet, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                if (autoPrint)
                {
                    await _printerService.PrintPalletListAsync(pallet.Id);
                }

                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.Id == pallet.Id);

                var results = await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (PalletClosedException pex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogWarning(pex, $"Attempted to close already closed pallet {palletId}");
                throw;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogError(ex, $"Error closing pallet {palletId}");
                throw;
            }
        }

        public async Task<IEnumerable<PalletDto>> SearchPalletsAsync(
            string keyword,
            bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<PalletDto>();
            }

            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p =>
                        p.PalletNumber.Value.Contains(keyword) ||
                        p.ManufacturingOrder.Contains(keyword));

                return await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    includeItems,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets with keyword '{keyword}'");
                throw;
            }
        }

        public async Task<PalletDto> GetPalletWithItemsByNumberAsync(
            string palletNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(palletNumber))
            {
                throw new ArgumentException("Pallet number cannot be null or empty", nameof(palletNumber));
            }

            try
            {
                var query = _unitOfWork.PalletRepository
                    .AsQueryable()
                    .Where(p => p.PalletNumber.Value == palletNumber);

                var results = await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    true,
                    cancellationToken);

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet with items for number {palletNumber}");
                throw;
            }
        }

        public async Task<IEnumerable<PalletDto>> GetAllPalletsAsync(
            bool includeItems = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _unitOfWork.PalletRepository.AsQueryable();

                return await _queryService.ProjectToDtoAsync<Pallet, PalletDto>(
                    query,
                    includeItems,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pallets");
                throw;
            }
        }

        public async Task<PagedResultDto<PalletDto>> GetPagedPalletsAsync(
            int pageNumber,
            int pageSize,
            Division? division = null,
            Platform? platform = null,
            bool? isClosed = null,
            string keyword = null,
            bool includeItems = false,
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
                var query = _unitOfWork.PalletRepository.AsQueryable();

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
                    query = query.Where(p =>
                        p.PalletNumber.Value.Contains(keyword) ||
                        p.ManufacturingOrder.Contains(keyword));
                }

                query = query.OrderByDescending(p => p.CreatedDate);

                return await _queryService.ProjectToPagedResultAsync<Pallet, PalletDto>(
                    query,
                    pageNumber,
                    pageSize,
                    includeItems,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged pallets");
                throw;
            }
        }
    }
}