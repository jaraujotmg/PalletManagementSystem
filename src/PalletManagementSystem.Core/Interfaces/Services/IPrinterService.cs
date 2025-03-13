using System.Threading.Tasks;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for printing operations
    /// </summary>
    public interface IPrinterService
    {
        /// <summary>
        /// Prints a pallet list
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PrintPalletListAsync(int palletId);

        /// <summary>
        /// Prints an item label
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PrintItemLabelAsync(int itemId);

        /// <summary>
        /// Gets the appropriate printer name for a pallet list based on division, platform, and special rules
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="hasSpecialClient">Whether the pallet has items from special clients</param>
        /// <returns>The printer name</returns>
        string GetPalletListPrinter(Division division, Platform platform, bool hasSpecialClient);

        /// <summary>
        /// Gets the appropriate printer name for an item label based on division, platform, and special rules
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="isSpecialClient">Whether the item belongs to a special client</param>
        /// <returns>The printer name</returns>
        string GetItemLabelPrinter(Division division, Platform platform, bool isSpecialClient);

        /// <summary>
        /// Sets the default pallet list printer for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="printerName">The printer name</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetDefaultPalletListPrinterAsync(string username, string printerName);

        /// <summary>
        /// Sets the default item label printer for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="printerName">The printer name</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SetDefaultItemLabelPrinterAsync(string username, string printerName);

        /// <summary>
        /// Gets the default pallet list printer for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The printer name</returns>
        Task<string> GetDefaultPalletListPrinterAsync(string username);

        /// <summary>
        /// Gets the default item label printer for a user
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <returns>The printer name</returns>
        Task<string> GetDefaultItemLabelPrinterAsync(string username);
    }
}