// PalletManagementSystem.Core/Identity/BaseUserContextProvider.cs
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Identity
{
    /// <summary>
    /// Base implementation for user context providers
    /// </summary>
    public abstract class BaseUserContextProvider : IUserContextProvider
    {
        /// <summary>
        /// The logger instance
        /// </summary>
        protected readonly ILogger<BaseUserContextProvider> _logger;

        /// <summary>
        /// Initializes a new instance of BaseUserContextProvider
        /// </summary>
        /// <param name="logger">The logger</param>
        protected BaseUserContextProvider(ILogger<BaseUserContextProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current username
        /// </summary>
        public abstract string GetCurrentUsername();

        /// <summary>
        /// Gets the current user's display name
        /// </summary>
        public abstract Task<string> GetDisplayNameAsync();

        /// <summary>
        /// Gets the current user's email
        /// </summary>
        public abstract Task<string> GetEmailAsync();

        /// <summary>
        /// Checks if user is in role
        /// </summary>
        public abstract bool IsInRole(string role);

        /// <summary>
        /// Gets user roles
        /// </summary>
        public abstract Task<string[]> GetRolesAsync();
    }
}