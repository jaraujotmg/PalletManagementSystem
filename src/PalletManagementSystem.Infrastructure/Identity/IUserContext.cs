using PalletManagementSystem.Core.Models.Enums;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Interface for providing user context information
    /// </summary>
    public interface IUserContext
    {
        /// <summary>
        /// Gets the current username
        /// </summary>
        /// <returns>The username</returns>
        string GetUsername();

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        /// <returns>The display name</returns>
        Task<string> GetDisplayNameAsync();

        /// <summary>
        /// Gets the current user's email
        /// </summary>
        /// <returns>The email</returns>
        Task<string> GetEmailAsync();

        /// <summary>
        /// Checks if the current user is in a specified role
        /// </summary>
        /// <param name="role">The role name</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        Task<bool> IsInRoleAsync(string role);

        /// <summary>
        /// Gets the current user's roles
        /// </summary>
        /// <returns>An array of role names</returns>
        Task<string[]> GetRolesAsync();

        /// <summary>
        /// Checks if the current user can edit pallets
        /// </summary>
        /// <returns>True if the user can edit pallets, false otherwise</returns>
        Task<bool> CanEditPalletsAsync();

        /// <summary>
        /// Checks if the current user can close pallets
        /// </summary>
        /// <returns>True if the user can close pallets, false otherwise</returns>
        Task<bool> CanClosePalletsAsync();

        /// <summary>
        /// Checks if the current user can edit items
        /// </summary>
        /// <returns>True if the user can edit items, false otherwise</returns>
        Task<bool> CanEditItemsAsync();

        /// <summary>
        /// Checks if the current user can move items
        /// </summary>
        /// <returns>True if the user can move items, false otherwise</returns>
        Task<bool> CanMoveItemsAsync();

        /// <summary>
        /// Gets the current session's division
        /// </summary>
        /// <returns>The division</returns>
        Division GetDivision();

        /// <summary>
        /// Gets the current session's platform
        /// </summary>
        /// <returns>The platform</returns>
        Platform GetPlatform();
    }
}