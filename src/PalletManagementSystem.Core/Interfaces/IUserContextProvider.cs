using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Provides access to the current user context
    /// </summary>
    public interface IUserContextProvider
    {
        /// <summary>
        /// Gets the current username
        /// </summary>
        /// <returns>The username</returns>
        string GetCurrentUsername();

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
        bool IsInRole(string role);

        /// <summary>
        /// Gets the current user's roles
        /// </summary>
        /// <returns>An array of role names</returns>
        Task<string[]> GetRolesAsync();
    }
}