using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
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
        /// Initializes a new instance of the <see cref="SettingsController"/> class
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
        /// <returns>Settings view</returns>
        public async Task<ActionResult> Index()
        {
            try
            {
                var username = _userContext.GetUsername();
                var preferences = await _userPreferenceService.GetAllPreferencesAsync(username);

                // Get available divisions for dropdown
                ViewBag.Divisions = new[]
                {
                    new SelectListItem { Value = "MA", Text = "MA - Manufacturing", Selected = preferences.PreferredDivision == "MA" },
                    new SelectListItem { Value = "TC", Text = "TC - Technical Center", Selected = preferences.PreferredDivision == "TC" }
                };

                // Get available platforms for the current division
                Division division;
                if (!Enum.TryParse(preferences.PreferredDivision, out division))
                {
                    division = Division.MA;
                }

                ViewBag.Platforms = GetPlatformsForDivision(division);

                // Get available printers
                ViewBag.PalletListPrinters = new[]
                {
                    new SelectListItem { Value = "HP LaserJet 4200 - Office", Text = "HP LaserJet 4200 - Office", Selected = preferences.DefaultPalletListPrinter == "HP LaserJet 4200 - Office" },
                    new SelectListItem { Value = "Xerox WorkCentre - Production", Text = "Xerox WorkCentre - Production", Selected = preferences.DefaultPalletListPrinter == "Xerox WorkCentre - Production" }
                };

                ViewBag.ItemLabelPrinters = new[]
                {
                    new SelectListItem { Value = "Zebra ZT410 - Warehouse", Text = "Zebra ZT410 - Warehouse", Selected = preferences.DefaultItemLabelPrinter == "Zebra ZT410 - Warehouse" },
                    new SelectListItem { Value = "Zebra ZT230 - Shipping", Text = "Zebra ZT230 - Shipping", Selected = preferences.DefaultItemLabelPrinter == "Zebra ZT230 - Shipping" }
                };

                return View(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user settings");
                SetErrorMessage("There was an error loading your settings. Please try again.");
                return View(new UserPreferencesDto());
            }
        }

        /// <summary>
        /// POST: Settings
        /// Saves user settings
        /// </summary>
        /// <param name="preferences">The user preferences</param>
        /// <returns>Redirect to settings with result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(UserPreferencesDto preferences)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var username = _userContext.GetUsername();
                    preferences.Username = username;

                    // Parse and validate the division and platform
                    if (Enum.TryParse(preferences.PreferredDivision, out Division division) &&
                        Enum.TryParse(preferences.PreferredPlatform, out Platform platform))
                    {
                        if (!IsValidPlatformForDivision(platform, division))
                        {
                            ModelState.AddModelError("PreferredPlatform", $"Platform {platform} is not valid for division {division}.");

                            // Repopulate view data
                            ViewBag.Divisions = new[]
                            {
                                new SelectListItem { Value = "MA", Text = "MA - Manufacturing", Selected = preferences.PreferredDivision == "MA" },
                                new SelectListItem { Value = "TC", Text = "TC - Technical Center", Selected = preferences.PreferredDivision == "TC" }
                            };

                            ViewBag.Platforms = GetPlatformsForDivision(division);

                            ViewBag.PalletListPrinters = new[]
                            {
                                new SelectListItem { Value = "HP LaserJet 4200 - Office", Text = "HP LaserJet 4200 - Office", Selected = preferences.DefaultPalletListPrinter == "HP LaserJet 4200 - Office" },
                                new SelectListItem { Value = "Xerox WorkCentre - Production", Text = "Xerox WorkCentre - Production", Selected = preferences.DefaultPalletListPrinter == "Xerox WorkCentre - Production" }
                            };

                            ViewBag.ItemLabelPrinters = new[]
                            {
                                new SelectListItem { Value = "Zebra ZT410 - Warehouse", Text = "Zebra ZT410 - Warehouse", Selected = preferences.DefaultItemLabelPrinter == "Zebra ZT410 - Warehouse" },
                                new SelectListItem { Value = "Zebra ZT230 - Shipping", Text = "Zebra ZT230 - Shipping", Selected = preferences.DefaultItemLabelPrinter == "Zebra ZT230 - Shipping" }
                            };

                            return View(preferences);
                        }
                    }

                    // Save all preferences
                    await _userPreferenceService.SetAllPreferencesAsync(username, preferences);

                    // Also save printer preferences
                    await _printerService.SetDefaultPalletListPrinterAsync(username, preferences.DefaultPalletListPrinter);
                    await _printerService.SetDefaultItemLabelPrinterAsync(username, preferences.DefaultItemLabelPrinter);

                    // Update session variables
                    Session["CurrentDivision"] = preferences.PreferredDivision;
                    Session["CurrentPlatform"] = preferences.PreferredPlatform;

                    SetSuccessMessage("Your settings have been saved successfully.");
                    return RedirectToAction("Index");
                }

                // If we got here, there was a validation error
                // Repopulate view data
                ViewBag.Divisions = new[]
                {
                    new SelectListItem { Value = "MA", Text = "MA - Manufacturing", Selected = preferences.PreferredDivision == "MA" },
                    new SelectListItem { Value = "TC", Text = "TC - Technical Center", Selected = preferences.PreferredDivision == "TC" }
                };

                Division div;
                if (!Enum.TryParse(preferences.PreferredDivision, out div))
                {
                    div = Division.MA;
                }

                ViewBag.Platforms = GetPlatformsForDivision(div);

                ViewBag.PalletListPrinters = new[]
                {
                    new SelectListItem { Value = "HP LaserJet 4200 - Office", Text = "HP LaserJet 4200 - Office", Selected = preferences.DefaultPalletListPrinter == "HP LaserJet 4200 - Office" },
                    new SelectListItem { Value = "Xerox WorkCentre - Production", Text = "Xerox WorkCentre - Production", Selected = preferences.DefaultPalletListPrinter == "Xerox WorkCentre - Production" }
                };

                ViewBag.ItemLabelPrinters = new[]
                {
                    new SelectListItem { Value = "Zebra ZT410 - Warehouse", Text = "Zebra ZT410 - Warehouse", Selected = preferences.DefaultItemLabelPrinter == "Zebra ZT410 - Warehouse" },
                    new SelectListItem { Value = "Zebra ZT230 - Shipping", Text = "Zebra ZT230 - Shipping", Selected = preferences.DefaultItemLabelPrinter == "Zebra ZT230 - Shipping" }
                };

                return View(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user settings");
                SetErrorMessage("There was an error saving your settings. Please try again.");

                // Repopulate view data with basic values
                ViewBag.Divisions = new[]
                {
                    new SelectListItem { Value = "MA", Text = "MA - Manufacturing", Selected = true },
                    new SelectListItem { Value = "TC", Text = "TC - Technical Center" }
                };

                ViewBag.Platforms = GetPlatformsForDivision(Division.MA);

                ViewBag.PalletListPrinters = new[]
                {
                    new SelectListItem { Value = "HP LaserJet 4200 - Office", Text = "HP LaserJet 4200 - Office", Selected = true },
                    new SelectListItem { Value = "Xerox WorkCentre - Production", Text = "Xerox WorkCentre - Production" }
                };

                ViewBag.ItemLabelPrinters = new[]
                {
                    new SelectListItem { Value = "Zebra ZT410 - Warehouse", Text = "Zebra ZT410 - Warehouse", Selected = true },
                    new SelectListItem { Value = "Zebra ZT230 - Shipping", Text = "Zebra ZT230 - Shipping" }
                };

                return View(preferences);
            }
        }

        /// <summary>
        /// GET: Settings/SetDivision/MA
        /// Quick action to set the current division
        /// </summary>
        /// <param name="division">The division code</param>
        /// <returns>Redirect to previous page</returns>
        public async Task<ActionResult> SetDivision(string division)
        {
            try
            {
                // Parse the division
                if (!Enum.TryParse(division, out Division selectedDivision))
                {
                    SetErrorMessage("Invalid division selected.");
                    return RedirectToAction("Index", "Home");
                }

                // Save the preference
                var username = _userContext.GetUsername();
                await _userPreferenceService.SetPreferredDivisionAsync(username, selectedDivision);

                // Get the default platform for this division
                Platform defaultPlatform = GetDefaultPlatformForDivision(selectedDivision);
                await _userPreferenceService.SetPreferredPlatformAsync(username, selectedDivision, defaultPlatform);

                // Update session
                Session["CurrentDivision"] = selectedDivision.ToString();
                Session["CurrentPlatform"] = defaultPlatform.ToString();

                // Redirect back to referring page or home
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting division to {division}");
                SetErrorMessage("There was an error setting your division. Please try again.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: Settings/SetPlatform/TEC1
        /// Quick action to set the current platform
        /// </summary>
        /// <param name="platform">The platform code</param>
        /// <returns>Redirect to previous page</returns>
        public async Task<ActionResult> SetPlatform(string platform)
        {
            try
            {
                // Parse the platform
                if (!Enum.TryParse(platform, out Platform selectedPlatform))
                {
                    SetErrorMessage("Invalid platform selected.");
                    return RedirectToAction("Index", "Home");
                }

                // Get current division
                var username = _userContext.GetUsername();
                var division = await _userPreferenceService.GetPreferredDivisionAsync(username);

                // Validate platform for division
                if (!IsValidPlatformForDivision(selectedPlatform, division))
                {
                    SetErrorMessage($"Platform {selectedPlatform} is not valid for division {division}.");
                    return RedirectToAction("Index", "Home");
                }

                // Save the preference
                await _userPreferenceService.SetPreferredPlatformAsync(username, division, selectedPlatform);

                // Update session
                Session["CurrentPlatform"] = selectedPlatform.ToString();

                // Redirect back to referring page or home
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting platform to {platform}");
                SetErrorMessage("There was an error setting your platform. Please try again.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// GET: Settings/ToggleTouchMode
        /// Toggles touch mode on or off
        /// </summary>
        /// <param name="enable">Whether to enable touch mode</param>
        /// <returns>Redirect to previous page</returns>
        public async Task<ActionResult> ToggleTouchMode(bool enable)
        {
            try
            {
                // Save the preference
                var username = _userContext.GetUsername();
                await _userPreferenceService.SetTouchModeEnabledAsync(username, enable);

                // Update session
                Session["TouchModeEnabled"] = enable;

                // Set message
                if (enable)
                {
                    SetSuccessMessage("Touch mode enabled.");
                }
                else
                {
                    SetSuccessMessage("Touch mode disabled.");
                }

                // Redirect back to referring page or home
                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting touch mode to {enable}");
                SetErrorMessage("There was an error updating your touch mode setting. Please try again.");
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Gets the SelectListItems for platforms based on division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>SelectListItems for platforms</returns>
        private SelectListItem[] GetPlatformsForDivision(Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return new[]
                    {
                        new SelectListItem { Value = "TEC1", Text = "TEC1" },
                        new SelectListItem { Value = "TEC2", Text = "TEC2" },
                        new SelectListItem { Value = "TEC4I", Text = "TEC4I" }
                    };
                case Division.TC:
                    return new[]
                    {
                        new SelectListItem { Value = "TEC1", Text = "TEC1" },
                        new SelectListItem { Value = "TEC3", Text = "TEC3" },
                        new SelectListItem { Value = "TEC5", Text = "TEC5" }
                    };
                default:
                    return new[]
                    {
                        new SelectListItem { Value = "TEC1", Text = "TEC1" }
                    };
            }
        }

        /// <summary>
        /// Determines if a platform is valid for a division
        /// </summary>
        /// <param name="platform">The platform</param>
        /// <param name="division">The division</param>
        /// <returns>True if valid, false otherwise</returns>
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
        /// <param name="division">The division</param>
        /// <returns>The default platform</returns>
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