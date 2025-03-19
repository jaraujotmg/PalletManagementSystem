using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller for user settings
    /// </summary>
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IPrinterService _printerService;
        private readonly UserContext _userContext;
        private readonly ILogger<SettingsController> _logger;

        /// <summary>
        /// Initializes a new instance of SettingsController
        /// </summary>
        public SettingsController(
            IUserPreferenceService userPreferenceService,
            IPrinterService printerService,
            UserContext userContext,
            ILogger<SettingsController> logger)
        {
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: Settings
        /// Displays the user settings page
        /// </summary>
        public async Task<ActionResult> Index()
        {
            try
            {
                // Get user and preferences
                var username = _userContext.GetUsername();
                var preferences = await _userPreferenceService.GetAllPreferencesAsync(username);

                // Create the settings view model
                var viewModel = new SettingsViewModel(
                    username,
                    _userContext.GetDivision(),
                    _userContext.GetPlatform())
                {
                    // Display settings
                    ItemsPerPage = preferences.ItemsPerPage,
                    DefaultView = preferences.DefaultView,
                    ShowConfirmationPrompts = preferences.ShowConfirmationPrompts,
                    AutoRefreshPalletList = preferences.AutoRefreshPalletList,
                    RefreshInterval = preferences.RefreshInterval,

                    // Touch mode settings
                    TouchModeEnabled = preferences.TouchModeEnabled,
                    ShowTouchKeyboard = preferences.ShowTouchKeyboard,
                    UseLargeButtons = preferences.UseLargeButtons,
                    ButtonSize = preferences.ButtonSize,

                    // Printer settings
                    DefaultPalletListPrinter = preferences.DefaultPalletListPrinter,
                    DefaultItemLabelPrinter = preferences.DefaultItemLabelPrinter,
                    AutoPrintPalletList = preferences.AutoPrintPalletList,
                    UseSpecialClientSettings = preferences.UseSpecialClientSettings,

                    // Session settings
                    SessionTimeout = preferences.SessionTimeout,
                    ShowNotifications = preferences.ShowNotifications,
                    RememberDivisionPlatform = preferences.RememberDivisionPlatform
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user settings");
                SetErrorMessage("An error occurred while loading your settings. Please try again.");
                return View(new SettingsViewModel());
            }
        }

        /// <summary>
        /// POST: Settings
        /// Saves user settings
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(SettingsViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get username
                    var username = _userContext.GetUsername();
                    model.Username = username;

                    // Parse and validate division and platform
                    if (Enum.TryParse(model.PreferredDivision, out Division division) &&
                        Enum.TryParse(model.PreferredPlatform, out Platform platform))
                    {
                        // Check if platform is valid for division
                        if (!IsValidPlatformForDivision(platform, division))
                        {
                            ModelState.AddModelError("PreferredPlatform",
                                $"Platform {platform} is not valid for division {division}.");
                            return View(model);
                        }

                        // Store in session
                        Session["CurrentDivision"] = division.ToString();
                        Session["CurrentPlatform"] = platform.ToString();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid division or platform selection.");
                        return View(model);
                    }

                    // Store touch mode in session
                    Session["TouchModeEnabled"] = model.TouchModeEnabled;

                    // Map view model to DTO
                    var preferencesDto = new Core.DTOs.UserPreferencesDto
                    {
                        Username = username,
                        PreferredDivision = model.PreferredDivision,
                        PreferredPlatform = model.PreferredPlatform,
                        RememberDivisionPlatform = model.RememberDivisionPlatform,

                        // Display settings
                        ItemsPerPage = model.ItemsPerPage,
                        DefaultView = model.DefaultView,
                        ShowConfirmationPrompts = model.ShowConfirmationPrompts,
                        AutoRefreshPalletList = model.AutoRefreshPalletList,
                        RefreshInterval = model.RefreshInterval,

                        // Touch mode settings
                        TouchModeEnabled = model.TouchModeEnabled,
                        ShowTouchKeyboard = model.ShowTouchKeyboard,
                        UseLargeButtons = model.UseLargeButtons,
                        ButtonSize = model.ButtonSize,

                        // Printer settings
                        DefaultPalletListPrinter = model.DefaultPalletListPrinter,
                        DefaultItemLabelPrinter = model.DefaultItemLabelPrinter,
                        AutoPrintPalletList = model.AutoPrintPalletList,
                        UseSpecialClientSettings = model.UseSpecialClientSettings,

                        // Session settings
                        SessionTimeout = model.SessionTimeout,
                        ShowNotifications = model.ShowNotifications
                    };

                    // Save all preferences
                    await _userPreferenceService.SetAllPreferencesAsync(username, preferencesDto);

                    // Also save printer preferences
                    await _printerService.SetDefaultPalletListPrinterAsync(username, model.DefaultPalletListPrinter);
                    await _printerService.SetDefaultItemLabelPrinterAsync(username, model.DefaultItemLabelPrinter);

                    SetSuccessMessage("Your settings have been saved successfully.");
                    return RedirectToAction("Index");
                }

                // If we got here, there was a validation error
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user settings");
                ModelState.AddModelError("", "An error occurred while saving your settings. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Settings/SetDivision/MA
        /// Quick action to set the current division
        /// </summary>
        public async Task<ActionResult> SetDivision(string division)
        {
            try
            {
                // Parse division
                if (!Enum.TryParse(division, out Division selectedDivision))
                {
                    SetErrorMessage("Invalid division selected.");
                    return RedirectToAction("Index", "Home");
                }

                // Get username
                var username = _userContext.GetUsername();

                // Save preference
                await _userPreferenceService.SetPreferredDivisionAsync(username, selectedDivision);

                // Get default platform for this division
                var defaultPlatform = GetDefaultPlatformForDivision(selectedDivision);
                await _userPreferenceService.SetPreferredPlatformAsync(username, selectedDivision, defaultPlatform);

                // Store in session
                Session["CurrentDivision"] = selectedDivision.ToString();
                Session["CurrentPlatform"] = defaultPlatform.ToString();

                SetSuccessMessage($"Division set to {selectedDivision} and platform set to {defaultPlatform}.");

                // Redirect back to referring page
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting division to {division}");
                SetErrorMessage("An error occurred while changing your division. Please try again.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: Settings/SetPlatform/TEC1
        /// Quick action to set the current platform
        /// </summary>
        public async Task<ActionResult> SetPlatform(string platform)
        {
            try
            {
                // Parse platform
                if (!Enum.TryParse(platform, out Platform selectedPlatform))
                {
                    SetErrorMessage("Invalid platform selected.");
                    return RedirectToAction("Index", "Home");
                }

                // Get username and current division
                var username = _userContext.GetUsername();
                var division = _userContext.GetDivision();

                // Validate platform for division
                if (!IsValidPlatformForDivision(selectedPlatform, division))
                {
                    SetErrorMessage($"Platform {selectedPlatform} is not valid for division {division}.");
                    return RedirectToAction("Index", "Home");
                }

                // Save preference
                await _userPreferenceService.SetPreferredPlatformAsync(username, division, selectedPlatform);

                // Store in session
                Session["CurrentPlatform"] = selectedPlatform.ToString();

                SetSuccessMessage($"Platform set to {selectedPlatform}.");

                // Redirect back to referring page
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting platform to {platform}");
                SetErrorMessage("An error occurred while changing your platform. Please try again.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: Settings/ToggleTouchMode
        /// Quick action to toggle touch mode
        /// </summary>
        public async Task<ActionResult> ToggleTouchMode(bool enable)
        {
            try
            {
                // Get username
                var username = _userContext.GetUsername();

                // Save preference
                await _userPreferenceService.SetTouchModeEnabledAsync(username, enable);

                // Store in session
                Session["TouchModeEnabled"] = enable;

                SetSuccessMessage($"Touch mode {(enable ? "enabled" : "disabled")}.");

                // Redirect back to referring page
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error {(enable ? "enabling" : "disabling")} touch mode");
                SetErrorMessage("An error occurred while changing touch mode. Please try again.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Determines if a platform is valid for a division
        /// </summary>
        private bool IsValidPlatformForDivision(Platform platform, Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return platform == Platform.TEC1 || platform == Platform.TEC2 || platform == Platform.TEC4I;
                case Division.TC:
                    return platform == Platform.TEC1 || platform == Platform.TEC3 || platform == Platform.TEC5;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the default platform for a division
        /// </summary>
        private Platform GetDefaultPlatformForDivision(Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return Platform.TEC1;
                case Division.TC:
                    return Platform.TEC1;
                default:
                    return Platform.TEC1;
            }
        }
    }
}