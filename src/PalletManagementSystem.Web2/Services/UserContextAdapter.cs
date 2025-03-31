// src/PalletManagementSystem.Web2/Services/UserContextAdapter.cs
using System;
using System.Threading.Tasks; // Add using
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity; // Reference to IUserContext

namespace PalletManagementSystem.Web2.Services // Adjust namespace if needed
{
    /// <summary>
    /// Concrete implementation of IUserContextAdapter.
    /// Delegates calls to IUserContext and ISessionManager, using async/await
    /// for inherently asynchronous operations from IUserContext.
    /// </summary>
    public class UserContextAdapter : IUserContextAdapter
    {
        private readonly IUserContext _userContext;
        private readonly ISessionManager _sessionManager;

        public UserContextAdapter(IUserContext userContext, ISessionManager sessionManager)
        {
            _userContext = userContext ?? throw new ArgumentNullException("userContext");
            _sessionManager = sessionManager ?? throw new ArgumentNullException("sessionManager");
        }

        // --- Synchronous Methods ---

        public string GetUsername()
        {
            // Delegate directly - GetUsername is synchronous on IUserContext
            return _userContext.GetUsername();
        }

        public Division GetDivision()
        {
            // Delegate directly - GetCurrentDivision is synchronous on ISessionManager
            return _sessionManager.GetCurrentDivision();
        }

        public Platform GetPlatform()
        {
            // Delegate directly - GetCurrentPlatform is synchronous on ISessionManager
            return _sessionManager.GetCurrentPlatform();
        }


        // --- Asynchronous Methods ---

        public async Task<string> GetDisplayNameAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.GetDisplayNameAsync();
        }

        public async Task<string> GetEmailAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.GetEmailAsync();
        }

        public async Task<string[]> GetRolesAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.GetRolesAsync();
        }

        public async Task<bool> IsInRoleAsync(string role)
        {
            // Use await to call the async method on IUserContext
            return await _userContext.IsInRoleAsync(role);
        }

        public async Task<bool> CanEditPalletsAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.CanEditPalletsAsync();
        }

        public async Task<bool> CanClosePalletsAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.CanClosePalletsAsync();
        }

        public async Task<bool> CanEditItemsAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.CanEditItemsAsync();
        }

        public async Task<bool> CanMoveItemsAsync()
        {
            // Use await to call the async method on IUserContext
            return await _userContext.CanMoveItemsAsync();
        }
    }
}