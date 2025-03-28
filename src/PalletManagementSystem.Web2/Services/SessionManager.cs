// src/PalletManagementSystem.Web/Services/SessionManager.cs
using System;
using System.Web;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;

namespace PalletManagementSystem.Web2.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly IUserContext _userContext;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IPlatformValidationService _platformValidationService;

        public SessionManager(
            IUserContext userContext,
            IUserPreferenceService userPreferenceService,
            IPlatformValidationService platformValidationService)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
        }

        public Division GetCurrentDivision()
        {
            // Try to get from session first
            if (HttpContext.Current.Session["CurrentDivision"] != null)
            {
                return (Division)HttpContext.Current.Session["CurrentDivision"];
            }

            // Otherwise get from user preferences and store in session
            var username = _userContext.GetUsername();
            var division = _userPreferenceService.GetPreferredDivisionAsync(username).Result;
            HttpContext.Current.Session["CurrentDivision"] = division;
            return division;
        }

        public void SetCurrentDivision(Division division)
        {
            HttpContext.Current.Session["CurrentDivision"] = division;

            // Also update user preferences asynchronously
            var username = _userContext.GetUsername();
            _userPreferenceService.SetPreferredDivisionAsync(username, division);

            // Check if current platform is valid for new division
            var currentPlatform = GetCurrentPlatform();
            bool isValid = _platformValidationService.IsValidPlatformForDivisionAsync(currentPlatform, division).Result;

            if (!isValid)
            {
                // Set to default platform for this division
                var defaultPlatform = _platformValidationService.GetDefaultPlatformForDivisionAsync(division).Result;
                SetCurrentPlatform(defaultPlatform);
            }
        }

        public Platform GetCurrentPlatform()
        {
            // Try to get from session first
            if (HttpContext.Current.Session["CurrentPlatform"] != null)
            {
                return (Platform)HttpContext.Current.Session["CurrentPlatform"];
            }

            // Otherwise get from user preferences and store in session
            var username = _userContext.GetUsername();
            var division = GetCurrentDivision();
            var platform = _userPreferenceService.GetPreferredPlatformAsync(username, division).Result;
            HttpContext.Current.Session["CurrentPlatform"] = platform;
            return platform;
        }

        public void SetCurrentPlatform(Platform platform)
        {
            HttpContext.Current.Session["CurrentPlatform"] = platform;

            // Also update user preferences asynchronously
            var username = _userContext.GetUsername();
            var division = GetCurrentDivision();
            _userPreferenceService.SetPreferredPlatformAsync(username, division, platform);
        }

        public bool IsTouchModeEnabled()
        {
            // Try to get from session first
            if (HttpContext.Current.Session["TouchModeEnabled"] != null)
            {
                return (bool)HttpContext.Current.Session["TouchModeEnabled"];
            }

            // Otherwise get from user preferences and store in session
            var username = _userContext.GetUsername();
            var touchModeEnabled = _userPreferenceService.GetTouchModeEnabledAsync(username).Result;
            HttpContext.Current.Session["TouchModeEnabled"] = touchModeEnabled;
            return touchModeEnabled;
        }

        public void SetTouchModeEnabled(bool enabled)
        {
            HttpContext.Current.Session["TouchModeEnabled"] = enabled;

            // Also update user preferences asynchronously
            var username = _userContext.GetUsername();
            _userPreferenceService.SetTouchModeEnabledAsync(username, enabled);
        }

        public void SetPreferredPrinter(string printerType, string printerName)
        {
            HttpContext.Current.Session[$"Printer_{printerType}"] = printerName;
        }

        public string GetPreferredPrinter(string printerType)
        {
            return HttpContext.Current.Session[$"Printer_{printerType}"] as string;
        }
    }
}