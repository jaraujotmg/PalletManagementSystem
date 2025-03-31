// src/PalletManagementSystem.Web2/Services/SessionManager.cs
using System;
using System.Threading.Tasks; // <-- Add using
using System.Web;
using System.Web.SessionState; // For HttpSessionState
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
// REMOVE: using PalletManagementSystem.Infrastructure.Identity;

namespace PalletManagementSystem.Web2.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IPlatformValidationService _platformValidationService;
        // Add ILogger logger dependency here if you want proper logging instead of Debug.WriteLine

        // Helper to get session safely
        private HttpSessionState Session => HttpContext.Current?.Session;

        public SessionManager(
            IUserPreferenceService userPreferenceService,
            IPlatformValidationService platformValidationService)
        {
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
        }

        // --- Helper method to get username without IUserContext ---
        private string GetCurrentUsername()
        {
            var identity = HttpContext.Current?.User?.Identity;
            if (identity != null && identity.IsAuthenticated)
            {
                var username = identity.Name?.Split('\\').Length > 1
                    ? identity.Name.Split('\\')[1]
                    : identity.Name;
                return !string.IsNullOrEmpty(username) ? username : Environment.UserName;
            }
            return "UnknownUser_SessionManager"; // Fallback
        }

        // --- Synchronous GET methods (with blocking fallback) ---

        public Division GetCurrentDivision()
        {
            var session = Session;
            // Use pattern matching for safer cast
            if (session?["CurrentDivision"] is Division sessionDivision)
            {
                return sessionDivision;
            }

            // Fallback - synchronous call with GetAwaiter().GetResult()
            var username = GetCurrentUsername();
            try
            {
                var division = _userPreferenceService.GetPreferredDivisionAsync(username).GetAwaiter().GetResult();
                if (session != null) session["CurrentDivision"] = division; // Store in session after fetching
                return division;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetCurrentDivision fallback: {ex}");
                return Division.MA; // Default
            }
        }

        public Platform GetCurrentPlatform()
        {
            var session = Session;
            if (session?["CurrentPlatform"] is Platform sessionPlatform)
            {
                return sessionPlatform;
            }

            var username = GetCurrentUsername();
            // GetCurrentDivision might itself block here if session is empty
            var division = GetCurrentDivision();
            try
            {
                var platform = _userPreferenceService.GetPreferredPlatformAsync(username, division).GetAwaiter().GetResult();
                if (session != null) session["CurrentPlatform"] = platform; // Store in session
                return platform;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetCurrentPlatform fallback: {ex}");
                return Platform.TEC1; // Default
            }
        }

        public bool IsTouchModeEnabled()
        {
            var session = Session;
            if (session?["TouchModeEnabled"] is bool sessionTouchMode)
            {
                return sessionTouchMode;
            }
            var username = GetCurrentUsername();
            try
            {
                var touchModeEnabled = _userPreferenceService.GetTouchModeEnabledAsync(username).GetAwaiter().GetResult();
                if (session != null) session["TouchModeEnabled"] = touchModeEnabled; // Store in session
                return touchModeEnabled;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in IsTouchModeEnabled fallback: {ex}");
                return false; // Default
            }
        }

        public string GetPreferredPrinter(string printerType)
        {
            // This only reads from session, no async needed unless adding preference fallback
            return Session?[$"Printer_{printerType}"] as string;
        }

        // --- Implement ASYNC SET methods for Division and Platform ---

        public async Task SetCurrentDivisionAsync(Division division) // Implements async interface method
        {
            var session = Session;
            if (session == null)
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: SetCurrentDivisionAsync called with null session.");
                return; // Or throw? Cannot set session if context is null
            }

            session["CurrentDivision"] = division;

            var username = GetCurrentUsername();
            bool platformAdjusted = false; // Flag to avoid setting platform twice if adjusted here

            // Update preference in background (fire and forget isn't ideal, but simple here)
            // A better way involves background task queues or handling the Task result.
            _ = Task.Run(async () => {
                try
                {
                    await _userPreferenceService.SetPreferredDivisionAsync(username, division);
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ERROR saving division pref: {ex}"); }
            });


            // Perform platform validation logic asynchronously
            var currentPlatform = GetCurrentPlatform(); // Reads sync (might block if pref needed)
            bool isValid = false;
            try
            {
                isValid = await _platformValidationService.IsValidPlatformForDivisionAsync(currentPlatform, division);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ERROR validating platform: {ex}"); }


            if (!isValid)
            {
                try
                {
                    var defaultPlatform = await _platformValidationService.GetDefaultPlatformForDivisionAsync(division);
                    // Call the async SetCurrentPlatformAsync directly here
                    await SetCurrentPlatformAsync(defaultPlatform);
                    platformAdjusted = true; // Mark that platform was set here
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ERROR setting default platform: {ex}"); }
            }

            // If platform wasn't adjusted above, ensure preference is saved for potentially *existing* valid platform
            if (!platformAdjusted)
            {
                _ = Task.Run(async () => {
                    try
                    {
                        // Save preference for the *current* platform under the new division context
                        await _userPreferenceService.SetPreferredPlatformAsync(username, division, currentPlatform);
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ERROR saving existing platform pref: {ex}"); }
                });
            }
        }

        public async Task SetCurrentPlatformAsync(Platform platform) // Implements async interface method
        {
            var session = Session;
            if (session == null)
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: SetCurrentPlatformAsync called with null session.");
                return;
            }

            session["CurrentPlatform"] = platform;

            var username = GetCurrentUsername();
            var division = GetCurrentDivision(); // Reads sync (might block if pref needed)

            // Update preference in background
            _ = Task.Run(async () => {
                try
                {
                    await _userPreferenceService.SetPreferredPlatformAsync(username, division, platform);
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ERROR saving platform pref: {ex}"); }
            });
        }

        // --- Keep other SET methods synchronous as per interface ---

        public void SetTouchModeEnabled(bool enabled)
        {
            var session = Session;
            if (session == null) return;

            session["TouchModeEnabled"] = enabled;

            var username = GetCurrentUsername();
            // Update preference in background
            _ = Task.Run(async () => {
                try
                {
                    await _userPreferenceService.SetTouchModeEnabledAsync(username, enabled);
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ERROR saving touch mode pref: {ex}"); }
            });
        }

        public void SetPreferredPrinter(string printerType, string printerName)
        {
            var session = Session;
            if (session == null) return;

            session[$"Printer_{printerType}"] = printerName;
            // Add preference saving logic here if needed (make it async and use Task.Run or similar)
        }
    }
}