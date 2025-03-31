// src/PalletManagementSystem.Web2/Services/IUserContextAdapter.cs (or appropriate location)
using System.Threading.Tasks; // Add using
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Web2.Services // Adjust namespace if needed
{
    /// <summary>
    /// Adapts IUserContext (potentially from another layer) and combines it
    /// with session information for use primarily within the Web layer,
    /// exposing methods asynchronously where appropriate.
    /// </summary>
    public interface IUserContextAdapter
    {
        // --- Synchronous Methods (Underlying sources are sync) ---
        string GetUsername();
        Division GetDivision();
        Platform GetPlatform();

        // --- Asynchronous Methods (Wrapping async IUserContext methods) ---
        Task<string> GetDisplayNameAsync();
        Task<string> GetEmailAsync();
        Task<string[]> GetRolesAsync();
        Task<bool> IsInRoleAsync(string role);
        Task<bool> CanEditPalletsAsync();
        Task<bool> CanClosePalletsAsync();
        Task<bool> CanEditItemsAsync();
        Task<bool> CanMoveItemsAsync();
    }
}