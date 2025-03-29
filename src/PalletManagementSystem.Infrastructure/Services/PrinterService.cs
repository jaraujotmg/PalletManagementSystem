// src/PalletManagementSystem.Infrastructure/Services/PrinterService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Services.SSRSIntegration;

namespace PalletManagementSystem.Infrastructure.Services
{
    public class PrinterService : IPrinterService
    {
        private readonly IPalletRepository _palletRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IReportingService _reportingService;
        private readonly IAppSettings _appSettings;
        private readonly ILogger<PrinterService> _logger;

        public PrinterService(
            IPalletRepository palletRepository,
            IItemRepository itemRepository,
            IReportingService reportingService,
            IAppSettings appSettings,
            ILogger<PrinterService> logger = null)
        {
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger;
        }

        public async Task<bool> PrintPalletListAsync(int palletId)
        {
            try
            {
                var pallet = await _palletRepository.GetByIdWithIncludesAsync(palletId, new[] { "Items" });
                if (pallet == null)
                {
                    _logger?.LogWarning($"PrinterService: Pallet with ID {palletId} not found.");
                    return false;
                }

                // Determine which printer to use based on division and platform
                string printerName = GetPalletListPrinterAsync(
                    pallet.Division,
                    pallet.Platform,
                    pallet.Items.Any(i => i.IsSpecialClient())).Result;

                if (string.IsNullOrEmpty(printerName))
                {
                    _logger?.LogWarning("PrinterService: No printer found for pallet list.");
                    return false;
                }

                // Send to reporting service
                var reportParameters = new Dictionary<string, string>
                {
                    { "PalletId", palletId.ToString() },
                    { "PrinterName", printerName }
                };

                bool result = await _reportingService.GenerateAndPrintReportAsync(
                    "PalletList", reportParameters, printerName);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"PrinterService: Error printing pallet list for pallet ID {palletId}");
                return false;
            }
        }

        public async Task<bool> PrintItemLabelAsync(int itemId)
        {
            try
            {
                var item = await _itemRepository.GetByIdWithIncludesAsync(itemId, new[] { "Pallet" });
                if (item == null)
                {
                    _logger?.LogWarning($"PrinterService: Item with ID {itemId} not found.");
                    return false;
                }

                // Determine which printer to use based on division and platform
                string printerName = await GetItemLabelPrinterAsync(
                    item.Pallet.Division,
                    item.Pallet.Platform,
                    item.IsSpecialClient());

                if (string.IsNullOrEmpty(printerName))
                {
                    _logger?.LogWarning("PrinterService: No printer found for item label.");
                    return false;
                }

                // Send to reporting service
                var reportParameters = new Dictionary<string, string>
                {
                    { "ItemId", itemId.ToString() },
                    { "PrinterName", printerName }
                };

                bool result = await _reportingService.GenerateAndPrintReportAsync(
                    "ItemLabel", reportParameters, printerName);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"PrinterService: Error printing item label for item ID {itemId}");
                return false;
            }
        }

        public async Task<string> GetPalletListPrinterAsync(Division division, Platform platform, bool hasSpecialClient)
        {
            try
            {
                // Get printer based on division and platform
                string printerName = _appSettings.GetSetting($"Printers:{division}:{platform}:PalletList");

                // Check if special client handling is needed
                if (hasSpecialClient && _appSettings.GetBoolSetting("Printers:UseSpecialClientPrinter", false))
                {
                    string specialPrinter = _appSettings.GetSetting("Printers:SpecialClient:PalletList");
                    if (!string.IsNullOrEmpty(specialPrinter))
                    {
                        printerName = specialPrinter;
                    }
                }

                // If not found, get default printer
                if (string.IsNullOrEmpty(printerName))
                {
                    printerName = _appSettings.GetSetting("Printers:Default:PalletList");
                }

                return printerName;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error getting pallet list printer");
                return string.Empty;
            }
        }

        public async Task<string> GetItemLabelPrinterAsync(Division division, Platform platform, bool isSpecialClient)
        {
            try
            {
                // Get printer based on division and platform
                string printerName = _appSettings.GetSetting($"Printers:{division}:{platform}:ItemLabel");

                // Check if special client handling is needed
                if (isSpecialClient && _appSettings.GetBoolSetting("Printers:UseSpecialClientPrinter", false))
                {
                    string specialPrinter = _appSettings.GetSetting("Printers:SpecialClient:ItemLabel");
                    if (!string.IsNullOrEmpty(specialPrinter))
                    {
                        printerName = specialPrinter;
                    }
                }

                // If not found, get default printer
                if (string.IsNullOrEmpty(printerName))
                {
                    printerName = _appSettings.GetSetting("Printers:Default:ItemLabel");
                }

                return printerName;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error getting item label printer");
                return string.Empty;
            }
        }

        public async Task<bool> SetDefaultPalletListPrinterAsync(string username, string printerName)
        {
            try
            {
                // In a real application, this would save to a database
                // For now, just simulate success
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error setting default pallet list printer");
                return false;
            }
        }

        public async Task<bool> SetDefaultItemLabelPrinterAsync(string username, string printerName)
        {
            try
            {
                // In a real application, this would save to a database
                // For now, just simulate success
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error setting default item label printer");
                return false;
            }
        }

        public async Task<string> GetDefaultPalletListPrinterAsync(string username)
        {
            try
            {
                // In a real application, this would read from a database
                // For now, just return the default from settings
                return _appSettings.GetSetting("Printers:Default:PalletList");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error getting default pallet list printer");
                return string.Empty;
            }
        }

        public async Task<string> GetDefaultItemLabelPrinterAsync(string username)
        {
            try
            {
                // In a real application, this would read from a database
                // For now, just return the default from settings
                return _appSettings.GetSetting("Printers:Default:ItemLabel");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error getting default item label printer");
                return string.Empty;
            }
        }

        public async Task<IEnumerable<string>> GetAvailablePrintersAsync(PrinterType printerType)
        {
            try
            {
                // In a real application, this would query available system printers or a configured list
                // For now, just return some sample printers
                List<string> printers = new List<string>();

                switch (printerType)
                {
                    case PrinterType.PalletList:
                        printers.Add("PalletListPrinter1");
                        printers.Add("PalletListPrinter2");
                        printers.Add("SharedPrinter1");
                        break;
                    case PrinterType.ItemLabel:
                        printers.Add("LabelPrinter1");
                        printers.Add("LabelPrinter2");
                        printers.Add("SharedPrinter1");
                        break;
                }

                return printers;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PrinterService: Error getting available printers");
                return Enumerable.Empty<string>();
            }
        }
    }
}