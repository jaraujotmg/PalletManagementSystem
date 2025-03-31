// src/PalletManagementSystem.Web2/Controllers/HomeController.cs
using System;
using System.Threading.Tasks; // <-- Ensure this using is present
using System.Web.Mvc;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web2.Services;
using PalletManagementSystem.Web2.ViewModels.Home;

namespace PalletManagementSystem.Web2.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ISessionManager _sessionManager;
        private readonly IPlatformValidationService _platformValidationService;

        public HomeController(
            IUserContext userContext,
            ISessionManager sessionManager,
            IPlatformValidationService platformValidationService)
            : base(userContext)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
        }

        // --- Change signature to async Task<ActionResult> ---
        public async Task<ActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // Set common properties
            viewModel.Username = Username; // Username from BaseController is likely sync
            try
            {
                // --- Use await instead of .Result ---
                viewModel.DisplayName = await GetDisplayName();
            }
            catch (Exception ex)
            {
                // Log the error if you have a logger
                System.Diagnostics.Debug.WriteLine($"Error getting display name: {ex.Message}");
                viewModel.DisplayName = Username; // Fallback
            }

            // Get methods from SessionManager are currently sync
            viewModel.CurrentDivision = _sessionManager.GetCurrentDivision();
            viewModel.CurrentPlatform = _sessionManager.GetCurrentPlatform();
            viewModel.TouchModeEnabled = _sessionManager.IsTouchModeEnabled();

            // ... rest of Index properties ...
            viewModel.LastLoginDate = DateTime.Now.AddDays(-1).ToString("MMMM dd, yyyy, hh:mm tt"); // Example static date
            viewModel.ApplicationVersion = "v2.5.1"; // Example version
            viewModel.DatabaseVersion = "v2.5.0"; // Example version
            viewModel.LastUpdateDate = "01/02/2025"; // Example static date
            viewModel.ServerName = Environment.MachineName; // Gets current server name
            viewModel.AllServicesOperational = true; // Example status


            return View(viewModel);
        }
        // --- End Change ---


        public async Task<ActionResult> SetDivisionPlatform(string division, string platform, string returnUrl)
        {
            Division divisionEnum = _sessionManager.GetCurrentDivision();

            if (!string.IsNullOrEmpty(division) && Enum.TryParse<Division>(division, out var parsedDivision))
            {
                divisionEnum = parsedDivision;
                await _sessionManager.SetCurrentDivisionAsync(divisionEnum);
            }

            if (!string.IsNullOrEmpty(platform) && Enum.TryParse<Platform>(platform, out Platform platformEnum))
            {
                var platformAfterDivisionSet = _sessionManager.GetCurrentPlatform();
                if (platformEnum != platformAfterDivisionSet)
                {
                    var divisionToCheck = _sessionManager.GetCurrentDivision();
                    bool isValidForDivision = await _platformValidationService.IsValidPlatformForDivisionAsync(platformEnum, divisionToCheck);

                    if (isValidForDivision)
                    {
                        await _sessionManager.SetCurrentPlatformAsync(platformEnum);
                    }
                    else { /* Log Warning */ }
                }
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}