// PalletManagementSystem.Infrastructure/Identity/DefaultUserContextProvider.cs
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Identity;
using System;
using System.Threading.Tasks;

namespace PalletManagementSystem.Infrastructure.Identity
{
    /// <summary>
    /// Default implementation of user context provider that can be used for tests
    /// or when not in a web context
    /// </summary>
    public class DefaultUserContextProvider : BaseUserContextProvider
    {
        /// <summary>
        /// Initializes a new instance of DefaultUserContextProvider
        /// </summary>
        /// <param name="logger">The logger</param>
        public DefaultUserContextProvider(ILogger<DefaultUserContextProvider> logger)
            : base(logger)
        {
        }

        /// <inheritdoc/>
        public override string GetCurrentUsername()
        {
            return Environment.UserName;
        }

        /// <inheritdoc/>
        public override Task<string> GetDisplayNameAsync()
        {
            return Task.FromResult(Environment.UserName);
        }

        /// <inheritdoc/>
        public override Task<string> GetEmailAsync()
        {
            return Task.FromResult($"{Environment.UserName}@example.com");
        }

        /// <inheritdoc/>
        public override bool IsInRole(string role)
        {
            // For default implementation, assume user has Viewer role only
            return role == "Viewer";
        }

        /// <inheritdoc/>
        public override Task<string[]> GetRolesAsync()
        {
            return Task.FromResult(new[] { "Viewer" });
        }
    }
}