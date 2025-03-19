using PalletManagementSystem.Core.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for printing pallet lists and item labels
    /// </summary>
    public interface IPrinterService
    {
        /// <summary>
        /// Prints a pallet list
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> PrintPalletListAsync(int palletId);

        /// <summary>
        /// Prints an item label
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> PrintItemLabelAsync(int itemId);

        /// <summary>
        /// Gets the appropriate printer for a pallet list based on division, platform, and special client status
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="hasSpecialClient">Whether the pallet contains items for special clients</param>
        /// <returns>The printer name</returns>
        Task<string> GetPalletListPrinterAsync(Division division, Platform platform, bool hasSpecialClient);

        /// <summary>
        /// Gets the appropriate printer for an item label based on division, platform, and special client status
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="isSpecialClient">Whether the item belongs to a special client</param>
        /// <returns>The printer name</returns>
        Task<string> GetItemLabelPrinterAsync(Division division, Platform platform, bool isSpecialClient);

        /// <summary>
        /// Sets the default pallet list printer for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="printerName">The printer name</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetDefaultPalletListPrinterAsync(string username, string printerName);

        /// <summary>
        /// Sets the default item label printer for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="printerName">The printer name</param>
        /// <returns>Task representing the async operation</returns>
        Task<bool> SetDefaultItemLabelPrinterAsync(string username, string printerName);

        /// <summary>
        /// Gets the default pallet list printer for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The printer name</returns>
        Task<string> GetDefaultPalletListPrinterAsync(string username);

        /// <summary>
        /// Gets the default item label printer for a user
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The printer name</returns>
        Task<string> GetDefaultItemLabelPrinterAsync(string username);

        /// <summary>
        /// Gets available printers for a specific context
        /// </summary>
        /// <param name="printerType">The type of printer (pallet list or item label)</param>
        /// <returns>Collection of available printer names</returns>
        Task<IEnumerable<string>> GetAvailablePrintersAsync(PrinterType printerType);
    }

    /// <summary>
    /// Enum defining printer types
    /// </summary>
    public enum PrinterType
    {
        /// <summary>
        /// Printer for pallet lists
        /// </summary>
        PalletList,

        /// <summary>
        /// Printer for item labels
        /// </summary>
        ItemLabel
    }
}