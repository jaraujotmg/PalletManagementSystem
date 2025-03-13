using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the printer service
    /// </summary>
    public class PrinterService : IPrinterService
    {
        private readonly IPalletRepository _palletRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IReportingService _reportingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PrinterService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterService"/> class
        /// </summary>
        /// <param name="palletRepository">The pallet repository</param>
        /// <param name="itemRepository">The item repository</param>
        /// <param name="reportingService">The reporting service</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="logger">The logger</param>
        public PrinterService(
            IPalletRepository palletRepository,
            IItemRepository itemRepository,
            IReportingService reportingService,
            IConfiguration configuration,
            ILogger<PrinterService> logger)
        {
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task PrintPalletListAsync(int palletId)
        {
            try
            {
                // Get the pallet with items
                var pallet = await _palletRepository.GetByIdWithItemsAsync(palletId);
                if (pallet == null)
                {
                    _logger.LogWarning($"Cannot print pallet list: Pallet with ID {palletId} not found");
                    return;
                }

                // Determine if the pallet has special client items
                bool hasSpecialClient = pallet.Items.Any(i => i.IsSpecialClient());

                // Get the appropriate printer
                string printerName = GetPalletListPrinter(pallet.Division, pallet.Platform, hasSpecialClient);

                // Print the report
                var reportPath = _configuration["Reports:PalletListReport"];
                var parameters = new { PalletId = palletId };

                await _reportingService.PrintReportAsync(reportPath, parameters, printerName);

                _logger.LogInformation($"Printed pallet list for pallet {pallet.PalletNumber.Value} to printer {printerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing pallet list for pallet ID {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task PrintItemLabelAsync(int itemId)
        {
            try
            {
                // Get the item with pallet
                var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning($"Cannot print item label: Item with ID {itemId} not found");
                    return;
                }

                if (item.Pallet == null)
                {
                    _logger.LogWarning($"Cannot print item label: Item with ID {itemId} is not assigned to a pallet");
                    return;
                }

                // Check if this is a special client
                bool isSpecialClient = item.IsSpecialClient();

                // Get the appropriate printer
                string printerName = GetItemLabelPrinter(item.Pallet.Division, item.Pallet.Platform, isSpecialClient);

                // Print the report
                var reportPath = _configuration["Reports:ItemLabelReport"];
                var parameters = new { ItemId = itemId };

                await _reportingService.PrintReportAsync(reportPath, parameters, printerName);

                _logger.LogInformation($"Printed label for item {item.ItemNumber} to printer {printerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing label for item ID {itemId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public string GetPalletListPrinter(Division division, Platform platform, bool hasSpecialClient)
        {
            // Check for special client rule
            if (hasSpecialClient)
            {
                var specialClientPrinter = _configuration["Printers:SpecialClient:PalletList"];
                if (!string.IsNullOrEmpty(specialClientPrinter))
                {
                    return specialClientPrinter;
                }
            }

            // Get division and platform specific printer
            string divisionPlatformKey = $"Printers:{division}:{platform}:PalletList";
            var divisionPlatformPrinter = _configuration[divisionPlatformKey];
            if (!string.IsNullOrEmpty(divisionPlatformPrinter))
            {
                return divisionPlatformPrinter;
            }

            // Get division specific printer
            string divisionKey = $"Printers:{division}:PalletList";
            var divisionPrinter = _configuration[divisionKey];
            if (!string.IsNullOrEmpty(divisionPrinter))
            {
                return divisionPrinter;
            }

            // Use default printer
            return _configuration["Printers:Default:PalletList"];
        }

        /// <inheritdoc/>
        public string GetItemLabelPrinter(Division division, Platform platform, bool isSpecialClient)
        {
            // Check for special client rule
            if (isSpecialClient)
            {
                var specialClientPrinter = _configuration["Printers:SpecialClient:ItemLabel"];
                if (!string.IsNullOrEmpty(specialClientPrinter))
                {
                    return specialClientPrinter;
                }
            }

            // Get division and platform specific printer
            string divisionPlatformKey = $"Printers:{division}:{platform}:ItemLabel";
            var divisionPlatformPrinter = _configuration[divisionPlatformKey];
            if (!string.IsNullOrEmpty(divisionPlatformPrinter))
            {
                return divisionPlatformPrinter;
            }

            // Get division specific printer
            string divisionKey = $"Printers:{division}:ItemLabel";
            var divisionPrinter = _configuration[divisionKey];
            if (!string.IsNullOrEmpty(divisionPrinter))
            {
                return divisionPrinter;
            }

            // Use default printer
            return _configuration["Printers:Default:ItemLabel"];
        }

        /// <inheritdoc/>
        public async Task SetDefaultPalletListPrinterAsync(string username, string printerName)
        {
            // In a real application, this would store the user preference in a database
            // For this implementation, we'll log the operation
            _logger.LogInformation($"Setting default pallet list printer for user {username} to {printerName}");
            await Task.CompletedTask; // Placeholder for actual implementation
        }

        /// <inheritdoc/>
        public async Task SetDefaultItemLabelPrinterAsync(string username, string printerName)
        {
            // In a real application, this would store the user preference in a database
            // For this implementation, we'll log the operation
            _logger.LogInformation($"Setting default item label printer for user {username} to {printerName}");
            await Task.CompletedTask; // Placeholder for actual implementation
        }

        /// <inheritdoc/>
        public async Task<string> GetDefaultPalletListPrinterAsync(string username)
        {
            // In a real application, this would retrieve the user preference from a database
            // For this implementation, we'll return the default printer
            _logger.LogInformation($"Getting default pallet list printer for user {username}");
            return await Task.FromResult(_configuration["Printers:Default:PalletList"]);
        }

        /// <inheritdoc/>
        public async Task<string> GetDefaultItemLabelPrinterAsync(string username)
        {
            // In a real application, this would retrieve the user preference from a database
            // For this implementation, we'll return the default printer
            _logger.LogInformation($"Getting default item label printer for user {username}");
            return await Task.FromResult(_configuration["Printers:Default:ItemLabel"]);
        }
    }
}