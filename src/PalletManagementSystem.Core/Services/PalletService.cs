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
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

namespace PalletManagementSystem.Core.Services
{
    public class PalletService : IPalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPalletRepository _palletRepository;
        private readonly IPrinterService _printerService;
        private readonly IPlatformValidationService _platformValidationService;
        private readonly ITransactionManager _transactionManager;
        private readonly ILogger<PalletService> _logger;

        public PalletService(
            IUnitOfWork unitOfWork,
            IPalletRepository palletRepository,
            IPrinterService printerService,
            IPlatformValidationService platformValidationService,
            ITransactionManager transactionManager,
            ILogger<PalletService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
            _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PalletListDto> GetPalletByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _palletRepository.GetPalletListByIdAsync(id, cancellationToken);
        }

        public async Task<PalletDetailDto> GetPalletDetailByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _palletRepository.GetPalletDetailByIdAsync(id, cancellationToken);
        }

        public async Task<PalletListDto> GetPalletByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            return await _palletRepository.GetPalletListByNumberAsync(palletNumber, cancellationToken);
        }

        public async Task<PalletDetailDto> GetPalletDetailByNumberAsync(string palletNumber, CancellationToken cancellationToken = default)
        {
            return await _palletRepository.GetPalletDetailByNumberAsync(palletNumber, cancellationToken);
        }

        public async Task<IEnumerable<PalletListDto>> GetPalletsByDivisionAndPlatformAsync(
            Division division,
            Platform platform,
            CancellationToken cancellationToken = default)
        {
            bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
            if (!isValid)
            {
                throw new ArgumentException($"Platform {platform} is not valid for division {division}");
            }

            return await _palletRepository.GetPalletsByDivisionAndPlatformAsync(division, platform, cancellationToken);
        }

        public async Task<IEnumerable<PalletListDto>> GetPalletsByStatusAsync(
            bool isClosed,
            CancellationToken cancellationToken = default)
        {
            return await _palletRepository.GetPalletsByStatusAsync(isClosed, cancellationToken);
        }

        public async Task<PalletListDto> CreatePalletAsync(
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string username,
            CancellationToken cancellationToken = default)
        {
            // Use isolation level ReadCommitted for pallet creation
            return await _transactionManager.ExecuteInTransactionWithIsolationAsync(async (token) =>
            {
                bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform, division);
                if (!isValid)
                {
                    throw new ArgumentException($"Platform {platform} is not valid for division {division}");
                }

                int sequenceNumber = await _palletRepository.GetNextTemporarySequenceNumberAsync(token);
                var palletNumber = PalletNumber.CreateTemporary(sequenceNumber, division);

                var pallet = new Pallet(
                    palletNumber,
                    manufacturingOrder,
                    division,
                    platform,
                    unitOfMeasure,
                    username);

                await _unitOfWork.PalletRepository.AddAsync(pallet, token);

                // Return the created pallet
                return await _palletRepository.GetPalletListByIdAsync(pallet.Id, token);
            }, cancellationToken, IsolationLevel.ReadCommitted);
        }

        public async Task<PalletDetailDto> ClosePalletAsync(
            int palletId,
            bool autoPrint = true,
            CancellationToken cancellationToken = default)
        {
            // Use serializable isolation level for closing pallets to prevent conflicts
            return await _transactionManager.ExecuteInTransactionWithIsolationAsync(async (token) =>
            {
                var pallet = await _unitOfWork.PalletRepository.GetByIdWithItemsAsync(palletId, token);
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
                    int sequenceNumber = await _palletRepository.GetNextPermanentSequenceNumberAsync(pallet.Division, token);
                    permanentNumber = PalletNumber.CreatePermanent(sequenceNumber, pallet.Division);
                }

                pallet.Close(permanentNumber);

                await _unitOfWork.PalletRepository.UpdateAsync(pallet, token);

                var updatedPallet = await _palletRepository.GetPalletDetailByIdAsync(palletId, token);

                // Print outside the transaction
                if (autoPrint)
                {
                    try
                    {
                        await _printerService.PrintPalletListAsync(pallet.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to print pallet list for pallet ID {pallet.Id}");
                        // Continue - printing failure shouldn't affect the business operation
                    }
                }

                return updatedPallet;
            }, cancellationToken, IsolationLevel.Serializable);
        }

        public async Task<IEnumerable<PalletListDto>> SearchPalletsAsync(string keyword, CancellationToken cancellationToken = default)
        {
            return await _palletRepository.SearchPalletsAsync(keyword, cancellationToken);
        }

        public async Task<PagedResultDto<PalletListDto>> GetPagedPalletsAsync(
            int pageNumber,
            int pageSize,
            Division? division = null,
            Platform? platform = null,
            bool? isClosed = null,
            string keyword = null,
            CancellationToken cancellationToken = default)
        {
            // Validate platform for division if both are specified
            if (division.HasValue && platform.HasValue)
            {
                bool isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(platform.Value, division.Value);
                if (!isValid)
                {
                    throw new ArgumentException($"Platform {platform} is not valid for division {division}");
                }
            }

            return await _palletRepository.GetPagedPalletsAsync(
                pageNumber,
                pageSize,
                division,
                platform,
                isClosed,
                keyword,
                true,
                true,
                cancellationToken);
        }
    }
}