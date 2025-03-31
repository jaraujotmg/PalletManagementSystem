// src/PalletManagementSystem.Web2/Controllers/SettingsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Extensions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web2.ViewModels.Settings;

namespace PalletManagementSystem.Web2.Controllers
{
    //[Authorize]
    public class SettingsController : BaseController
    {
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IPrinterService _printerService;
        private readonly IPlatformValidationService _platformValidationService;

        public SettingsController(
            IUserContext userContext,
            IUserPreferenceService userPreferenceService,
            IPrinterService printerService,
            IPlatformValidationService platformValidationService)
            : base(userContext)
        {
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
        }

        // GET: Settings
        public async Task<ActionResult> Index()
        {
            try
            {
                // Get user preferences
                var preferences = await _userPreferenceService.GetAllPreferencesAsync(Username);

                // Get platform options based on selected division
                Division division = (Division)Enum.Parse(typeof(Division), preferences.PreferredDivision);
                var platformOptions = await _platformValidationService.GetPlatformsForDivisionAsync(division);

                // Create the view model
                var viewModel = new UserPreferencesViewModel
                {
                    PreferredDivision = (Division)Enum.Parse(typeof(Division), preferences.PreferredDivision),
                    PreferredPlatform = (Platform)Enum.Parse(typeof(Platform), preferences.PreferredPlatform),
                    ItemsPerPage = preferences.ItemsPerPage,
                    DefaultPalletView = preferences.DefaultPalletView,
                    TouchModeEnabled = preferences.TouchModeEnabled,
                    TouchKeyboardEnabled = true, // Default value
                    LargeButtonsEnabled = true, // Default value
                    ButtonSize = "large", // Default value
                    ShowConfirmationPrompts = preferences.ShowConfirmationPrompts,
                    DefaultPalletListPrinter = preferences.DefaultPalletListPrinter,
                    DefaultItemLabelPrinter = preferences.DefaultItemLabelPrinter,
                    AutoPrintPalletList = preferences.AutoPrintPalletList,
                    UseSpecialPrinterForSpecialClients = preferences.UseSpecialPrinterForSpecialClients,
                    SessionTimeoutMinutes = preferences.SessionTimeoutMinutes,
                    RememberDivisionAndPlatform = preferences.RememberDivisionAndPlatform,
                    AutoRefreshPalletList = preferences.AutoRefreshPalletList,
                    AutoRefreshIntervalSeconds = preferences.AutoRefreshIntervalSeconds,
                    ShowBrowserNotifications = preferences.ShowBrowserNotifications,

                    // Additional info for display
                    ApplicationVersion = "v2.5.1",
                    DatabaseVersion = "v2.5.0",
                    LastUpdateDate = "01/02/2025",
                    ServerName = Environment.MachineName,
                    AllServicesOperational = true,

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform()
                    // Note: TouchModeEnabled is already set above
                };

                // Populate dropdown options
                viewModel.DivisionOptions = GetDivisionOptions(viewModel.PreferredDivision);
                viewModel.PlatformOptions = GetPlatformOptions(platformOptions, viewModel.PreferredPlatform);
                viewModel.DefaultViewOptions = GetDefaultViewOptions(viewModel.DefaultPalletView);
                viewModel.ButtonSizeOptions = GetButtonSizeOptions(viewModel.ButtonSize);
                viewModel.SessionTimeoutOptions = GetSessionTimeoutOptions(viewModel.SessionTimeoutMinutes);
                viewModel.RefreshIntervalOptions = GetRefreshIntervalOptions(viewModel.AutoRefreshIntervalSeconds);

                // Get printer options
                var palletListPrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);
                viewModel.PalletListPrinterOptions = palletListPrinters
                    .Select(p => new SelectListItem { Text = p, Value = p, Selected = p == viewModel.DefaultPalletListPrinter })
                    .ToList();

                var itemLabelPrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.ItemLabel);
                viewModel.ItemLabelPrinterOptions = itemLabelPrinters
                    .Select(p => new SelectListItem { Text = p, Value = p, Selected = p == viewModel.DefaultItemLabelPrinter })
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error retrieving settings: {ex.Message}");

                var fallbackViewModel = new UserPreferencesViewModel
                {
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username)
                };

                return View(fallbackViewModel);
            }
        }

        // POST: Settings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save(UserPreferencesViewModel viewModel)
        {
            try
            {
                // Update common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                // Remove duplicate initialization of TouchModeEnabled

                // Validate division/platform combination
                bool isValidPlatform = await _platformValidationService.IsValidPlatformForDivisionAsync(
                    viewModel.PreferredPlatform,
                    viewModel.PreferredDivision);

                if (!isValidPlatform)
                {
                    ModelState.AddModelError("PreferredPlatform",
                        $"Platform {viewModel.PreferredPlatform} is not valid for division {viewModel.PreferredDivision}");

                    // Re-populate dropdown options
                    await RepopulateDropdownOptions(viewModel);
                    return View("Index", viewModel);
                }

                if (!ModelState.IsValid)
                {
                    // Re-populate dropdown options
                    await RepopulateDropdownOptions(viewModel);
                    return View("Index", viewModel);
                }

                // Create the DTO to save
                var preferencesDto = new UserPreferencesDto
                {
                    Username = Username,
                    PreferredDivision = viewModel.PreferredDivision.ToString(),
                    PreferredPlatform = viewModel.PreferredPlatform.ToString(),
                    ItemsPerPage = viewModel.ItemsPerPage,
                    DefaultPalletView = viewModel.DefaultPalletView,
                    TouchModeEnabled = viewModel.TouchModeEnabled,
                    DefaultPalletListPrinter = viewModel.DefaultPalletListPrinter,
                    DefaultItemLabelPrinter = viewModel.DefaultItemLabelPrinter,
                    ShowConfirmationPrompts = viewModel.ShowConfirmationPrompts,
                    AutoPrintPalletList = viewModel.AutoPrintPalletList,
                    UseSpecialPrinterForSpecialClients = viewModel.UseSpecialPrinterForSpecialClients,
                    SessionTimeoutMinutes = viewModel.SessionTimeoutMinutes,
                    RememberDivisionAndPlatform = viewModel.RememberDivisionAndPlatform,
                    AutoRefreshPalletList = viewModel.AutoRefreshPalletList,
                    AutoRefreshIntervalSeconds = viewModel.AutoRefreshIntervalSeconds,
                    ShowBrowserNotifications = viewModel.ShowBrowserNotifications
                };

                // Save preferences
                bool result = await _userPreferenceService.SetAllPreferencesAsync(Username, preferencesDto);

                if (!result)
                {
                    ModelState.AddModelError("", "Failed to save preferences");

                    // Re-populate dropdown options
                    await RepopulateDropdownOptions(viewModel);
                    return View("Index", viewModel);
                }

                TempData["SuccessMessage"] = "Settings saved successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error saving settings: {ex.Message}");

                // Re-populate dropdown options
                await RepopulateDropdownOptions(viewModel);
                return View("Index", viewModel);
            }
        }

        // POST: Settings/GetPlatformsForDivision
        [HttpPost]
        public async Task<JsonResult> GetPlatformsForDivision(string division)
        {
            try
            {
                Division divisionEnum;
                if (!Enum.TryParse(division, out divisionEnum))
                {
                    return Json(new { success = false, message = "Invalid division" });
                }

                var platforms = await _platformValidationService.GetPlatformsForDivisionAsync(divisionEnum);

                return Json(new
                {
                    success = true,
                    platforms = platforms.Select(p => new
                    {
                        value = p.ToString(),
                        text = p.GetDescription()
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper methods for dropdown options
        private List<SelectListItem> GetDivisionOptions(Division selectedDivision)
        {
            return Enum.GetValues(typeof(Division))
                .Cast<Division>()
                .Select(d => new SelectListItem
                {
                    Text = d.GetDescription(),
                    Value = d.ToString(),
                    Selected = d == selectedDivision
                })
                .ToList();
        }

        private List<SelectListItem> GetPlatformOptions(IEnumerable<Platform> platforms, Platform selectedPlatform)
        {
            return platforms
                .Select(p => new SelectListItem
                {
                    Text = p.GetDescription(),
                    Value = p.ToString(),
                    Selected = p == selectedPlatform
                })
                .ToList();
        }

        private List<SelectListItem> GetDefaultViewOptions(string selectedView)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "All Pallets", Value = "all", Selected = selectedView == "all" },
                new SelectListItem { Text = "Open Pallets", Value = "open", Selected = selectedView == "open" },
                new SelectListItem { Text = "Closed Pallets", Value = "closed", Selected = selectedView == "closed" }
            };
        }

        private List<SelectListItem> GetButtonSizeOptions(string selectedSize)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Normal", Value = "normal", Selected = selectedSize == "normal" },
                new SelectListItem { Text = "Large", Value = "large", Selected = selectedSize == "large" },
                new SelectListItem { Text = "Extra Large", Value = "extra-large", Selected = selectedSize == "extra-large" }
            };
        }

        private List<SelectListItem> GetSessionTimeoutOptions(int selectedTimeout)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "15 minutes", Value = "15", Selected = selectedTimeout == 15 },
                new SelectListItem { Text = "30 minutes", Value = "30", Selected = selectedTimeout == 30 },
                new SelectListItem { Text = "60 minutes", Value = "60", Selected = selectedTimeout == 60 },
                new SelectListItem { Text = "120 minutes", Value = "120", Selected = selectedTimeout == 120 }
            };
        }

        private List<SelectListItem> GetRefreshIntervalOptions(int selectedInterval)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "30 seconds", Value = "30", Selected = selectedInterval == 30 },
                new SelectListItem { Text = "60 seconds", Value = "60", Selected = selectedInterval == 60 },
                new SelectListItem { Text = "5 minutes", Value = "300", Selected = selectedInterval == 300 },
                new SelectListItem { Text = "10 minutes", Value = "600", Selected = selectedInterval == 600 }
            };
        }

        // Helper method to re-populate all dropdown options
        private async Task RepopulateDropdownOptions(UserPreferencesViewModel viewModel)
        {
            // Get platforms for selected division
            var platformOptions = await _platformValidationService.GetPlatformsForDivisionAsync(viewModel.PreferredDivision);

            viewModel.DivisionOptions = GetDivisionOptions(viewModel.PreferredDivision);
            viewModel.PlatformOptions = GetPlatformOptions(platformOptions, viewModel.PreferredPlatform);
            viewModel.DefaultViewOptions = GetDefaultViewOptions(viewModel.DefaultPalletView);
            viewModel.ButtonSizeOptions = GetButtonSizeOptions(viewModel.ButtonSize);
            viewModel.SessionTimeoutOptions = GetSessionTimeoutOptions(viewModel.SessionTimeoutMinutes);
            viewModel.RefreshIntervalOptions = GetRefreshIntervalOptions(viewModel.AutoRefreshIntervalSeconds);

            // Get printer options
            var palletListPrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);
            viewModel.PalletListPrinterOptions = palletListPrinters
                .Select(p => new SelectListItem { Text = p, Value = p, Selected = p == viewModel.DefaultPalletListPrinter })
                .ToList();

            var itemLabelPrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.ItemLabel);
            viewModel.ItemLabelPrinterOptions = itemLabelPrinters
                .Select(p => new SelectListItem { Text = p, Value = p, Selected = p == viewModel.DefaultItemLabelPrinter })
                .ToList();
        }
    }
}