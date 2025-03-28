// src/PalletManagementSystem.Web/Services/UserContextAdapter.cs
using System;
using System.Threading.Tasks;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;

namespace PalletManagementSystem.Web2.Services
{
    public class UserContextAdapter : IUserContextAdapter
    {
        private readonly IUserContext _userContext;
        private readonly ISessionManager _sessionManager;

        public UserContextAdapter(IUserContext userContext, ISessionManager sessionManager)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }

        public string GetUsername()
        {
            return _userContext.GetUsername();
        }

        public string GetDisplayName()
        {
            Task<string> task = _userContext.GetDisplayNameAsync();
            task.Wait();
            return task.Result;
        }

        public string GetEmail()
        {
            Task<string> task = _userContext.GetEmailAsync();
            task.Wait();
            return task.Result;
        }

        public bool IsInRole(string role)
        {
            return _userContext.IsInRole(role);
        }

        public string[] GetRoles()
        {
            Task<string[]> task = _userContext.GetRolesAsync();
            task.Wait();
            return task.Result;
        }

        public bool CanEditPallets()
        {
            return _userContext.CanEditPallets();
        }

        public bool CanClosePallets()
        {
            return _userContext.CanClosePallets();
        }

        public bool CanEditItems()
        {
            return _userContext.CanEditItems();
        }

        public bool CanMoveItems()
        {
            return _userContext.CanMoveItems();
        }

        public Division GetDivision()
        {
            // Use session manager to get division from session or user preferences
            return _sessionManager.GetCurrentDivision();
        }

        public Platform GetPlatform()
        {
            // Use session manager to get platform from session or user preferences
            return _sessionManager.GetCurrentPlatform();
        }
    }
}