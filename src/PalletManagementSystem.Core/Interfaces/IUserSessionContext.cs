// src/PalletManagementSystem.Core/Interfaces/IUserSessionContext.cs
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Core.Interfaces
{
    /// <summary>
    /// Defines an interface for accessing user-specific session context,
    /// such as selected Division and Platform.
    /// This abstracts the underlying storage mechanism (e.g., web session).
    /// </summary>
    public interface IUserSessionContext
    {
        /// <summary>
        /// Gets the currently selected Division for the user's session.
        /// </summary>
        /// <returns>The current Division.</returns>
        Division GetCurrentDivision();

        /// <summary>
        /// Gets the currently selected Platform for the user's session.
        /// </summary>
        /// <returns>The current Platform.</returns>
        Platform GetCurrentPlatform();

        // Add other session-related methods here if needed in the future
        // For example: bool IsTouchModeEnabled();
    }
}