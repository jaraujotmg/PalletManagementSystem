using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;

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
        public async Task<bool> PrintPalletListAsync(int palletId)
        {
            try
            {
                // Get the pallet with items
                var pallet = await _palletRepository.GetByIdWithItemsAsync(palletId);
                if (pallet == null)
                {
                    _logger.LogWarning($"Cannot print pallet list: Pallet with ID {palletId} not found");
                    return false;
                }

                // Determine if the pallet has special client items
                bool hasSpecialClient = pallet.Items.Any(i => i.IsSpecialClient());

                // Get the appropriate printer
                string printerName = await GetPalletListPrinterAsync(pallet.Division, pallet.Platform, hasSpecialClient);

                // Print the report
                var reportPath = _configuration["Reports:PalletListReport"];
                var parameters = new { PalletId = palletId };

                bool result = await _reportingService.PrintReportAsync(reportPath, parameters, printerName);

                if (result)
                {
                    _logger.LogInformation($"Printed pallet list for pallet {pallet.PalletNumber.Value} to printer {printerName}");
                }
                else
                {
                    _logger.LogWarning($"Failed to print pallet list for pallet {pallet.PalletNumber.Value} to printer {printerName}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing pallet list for pallet ID {palletId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PrintItemLabelAsync(int itemId)
        {
            try
            {
                // Get the item with pallet
                var item = await _itemRepository.GetByIdWithPalletAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning($"Cannot print item label: Item with ID {itemId} not found");
                    return false;
                }

                if (item.Pallet == null)
                {
                    _logger.LogWarning($"Cannot print item label: Item with ID {itemId} is not assigned to a pallet");
                    return false;
                }

                // Check if this is a special client
                bool isSpecialClient = item.IsSpecialClient();

                // Get the appropriate printer
                string printerName = await GetItemLabelPrinterAsync(item.Pallet.Division, item.Pallet.Platform, isSpecialClient);

                // Print the report
                var reportPath = _configuration["Reports:ItemLabelReport"];
                var parameters = new { ItemId = itemId };

                bool result = await _reportingService.PrintReportAsync(reportPath, parameters, printerName);

                if (result)
                {
                    _logger.LogInformation($"Printed label for item {item.ItemNumber} to printer {printerName}");
                }
                else
                {
                    _logger.LogWarning($"Failed to print label for item {item.ItemNumber} to printer {printerName}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing label for item ID {itemId}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetPalletListPrinterAsync(Division division, Platform platform, bool hasSpecialClient)
        {
            try
            {
                // Check for special client rule
                if (hasSpecialClient)
                {
                    var specialClientPrinter = _configuration["Printers:SpecialClient:PalletList"];
                    if (!string.IsNullOrEmpty(specialClientPrinter))
                    {
                        return await Task.FromResult(specialClientPrinter);
                    }
                }

                // Get division and platform specific printer
                string divisionPlatformKey = $"Printers:{division}:{platform}:PalletList";
                var divisionPlatformPrinter = _configuration[divisionPlatformKey];
                if (!string.IsNullOrEmpty(divisionPlatformPrinter))
                {
                    return await Task.FromResult(divisionPlatformPrinter);
                }

                // Get division specific printer
                string divisionKey = $"Printers:{division}:PalletList";
                var divisionPrinter = _configuration[divisionKey];
                if (!string.IsNullOrEmpty(divisionPrinter))
                {
                    return await Task.FromResult(divisionPrinter);
                }

                // Use default printer
                return await Task.FromResult(_configuration["Printers:Default:PalletList"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting pallet list printer for division {division}, platform {platform}, hasSpecialClient={hasSpecialClient}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetItemLabelPrinterAsync(Division division, Platform platform, bool isSpecialClient)
        {
            try
            {
                // Check for special client rule
                if (isSpecialClient)
                {
                    var specialClientPrinter = _configuration["Printers:SpecialClient:ItemLabel"];
                    if (!string.IsNullOrEmpty(specialClientPrinter))
                    {
                        return await Task.FromResult(specialClientPrinter);
                    }
                }

                // Get division and platform specific printer
                string divisionPlatformKey = $"Printers:{division}:{platform}:ItemLabel";
                var divisionPlatformPrinter = _configuration[divisionPlatformKey];
                if (!string.IsNullOrEmpty(divisionPlatformPrinter))
                {
                    return await Task.FromResult(divisionPlatformPrinter);
                }

                // Get division specific printer
                string divisionKey = $"Printers:{division}:ItemLabel";
                var divisionPrinter = _configuration[divisionKey];
                if (!string.IsNullOrEmpty(divisionPrinter))
                {
                    return await Task.FromResult(divisionPrinter);
                }

                // Use default printer
                return await Task.FromResult(_configuration["Printers:Default:ItemLabel"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting item label printer for division {division}, platform {platform}, isSpecialClient={isSpecialClient}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetDefaultPalletListPrinterAsync(string username, string printerName)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(printerName))
            {
                throw new ArgumentException("Printer name cannot be null or empty", nameof(printerName));
            }

            try
            {
                // In a real application, this would store the user preference in a database
                // For this implementation, we'll log the operation
                _logger.LogInformation($"Setting default pallet list printer for user {username} to {printerName}");

                // Return success
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default pallet list printer for user {username} to {printerName}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SetDefaultItemLabelPrinterAsync(string username, string printerName)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(printerName))
            {
                throw new ArgumentException("Printer name cannot be null or empty", nameof(printerName));
            }

            try
            {
                // In a real application, this would store the user preference in a database
                // For this implementation, we'll log the operation
                _logger.LogInformation($"Setting default item label printer for user {username} to {printerName}");

                // Return success
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting default item label printer for user {username} to {printerName}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetDefaultPalletListPrinterAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // In a real application, this would retrieve the user preference from a database
                // For this implementation, we'll return the default printer
                _logger.LogInformation($"Getting default pallet list printer for user {username}");
                return await Task.FromResult(_configuration["Printers:Default:PalletList"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting default pallet list printer for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetDefaultItemLabelPrinterAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            try
            {
                // In a real application, this would retrieve the user preference from a database
                // For this implementation, we'll return the default printer
                _logger.LogInformation($"Getting default item label printer for user {username}");
                return await Task.FromResult(_configuration["Printers:Default:ItemLabel"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting default item label printer for user {username}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetAvailablePrintersAsync(PrinterType printerType)
        {
            try
            {
                var printers = new List<string>();

                // In a real application, this would query the available printers from the system or configuration
                // For this implementation, we'll return hard-coded values

                switch (printerType)
                {
                    case PrinterType.PalletList:
                        printers.Add("HP LaserJet 4200 - Office");
                        printers.Add("Xerox WorkCentre - Production");
                        printers.Add("Brother MFC - Warehouse");
                        printers.Add("Xerox WorkCentre - Special Printer");
                        break;
                    case PrinterType.ItemLabel:
                        printers.Add("Zebra ZT410 - Warehouse");
                        printers.Add("Zebra ZT230 - Shipping");
                        printers.Add("Zebra ZT230 - Special Label");
                        printers.Add("Datamax - Production");
                        break;
                    default:
                        throw new ArgumentException($"Unknown printer type: {printerType}");
                }

                return await Task.FromResult(printers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting available printers for type {printerType}");
                throw;
            }
        }
    }
}