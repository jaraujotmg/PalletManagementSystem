// src/PalletManagementSystem.Web2/Services/UserSessionContext.cs
using PalletManagementSystem.Core.Interfaces; // Interface from Core
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Web2.Services; // Your ISessionManager location
using System;

namespace PalletManagementSystem.Web2.Services // Or adjust namespace as needed
{
    /// <summary>
    /// Web-specific implementation of IUserSessionContext that uses
    /// ISessionManager to retrieve session data.
    /// </summary>
    public class UserSessionContext : IUserSessionContext
    {
        private readonly ISessionManager _sessionManager;

        public UserSessionContext(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }

        /// <summary>
        /// Gets the currently selected Division from the session via ISessionManager.
        /// </summary>
        /// <returns>The current Division.</returns>
        public Division GetCurrentDivision()
        {
            // Delegate the call to the existing session manager
            return _sessionManager.GetCurrentDivision();
        }

        /// <summary>
        /// Gets the currently selected Platform from the session via ISessionManager.
        /// </summary>
        /// <returns>The current Platform.</returns>
        public Platform GetCurrentPlatform()
        {
            // Delegate the call to the existing session manager
            return _sessionManager.GetCurrentPlatform();
        }
    }
}