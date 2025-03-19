using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller for home page and general application functions
    /// </summary>
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IPalletService _palletService;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly UserContext _userContext;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class
        /// </summary>
        public HomeController(
            IPalletService palletService,
            IUserPreferenceService userPreferenceService,
            UserContext userContext,
            ILogger<HomeController> logger)
        {
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: Home
        /// Displays the welcome/dashboard page
        /// </summary>
        /// <returns>Welcome view</returns>
        public async Task<ActionResult> Index()
        {
            try
            {
                // Get user's preferences
                var username = _userContext.GetUsername();
                var division = await _userPreferenceService.GetPreferredDivisionAsync(username);
                var platform = await _userPreferenceService.GetPreferredPlatformAsync(username, division);

                // Store in ViewBag for the view
                ViewBag.Division = division;
                ViewBag.Platform = platform;
                ViewBag.Username = username;

                // Get recent activity for the dashboard
                // Get some recent pallets
                var pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
                var recentPallets = pallets.OrderByDescending(p => p.CreatedDate).Take(5).ToList();
                ViewBag.RecentPallets = recentPallets;

                // Check if user needs to select division/platform
                bool shouldSelectDivisionPlatform = false;
                ViewBag.ShouldSelectDivisionPlatform = shouldSelectDivisionPlatform;

                // Add some system info
                ViewBag.ApplicationVersion = "v2.5.1";
                ViewBag.DatabaseVersion = "v2.5.0";
                ViewBag.LastUpdateDate = "01/02/2025";
                ViewBag.Server = "PROD-APP01";

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                SetErrorMessage("There was an error loading the dashboard. Please try again.");
                return View();
            }
        }

        /// <summary>
        /// GET: Home/SetDivisionPlatform
        /// Shows the division and platform selection page
        /// </summary>
        /// <returns>Division/platform selection view</returns>
        public ActionResult SetDivisionPlatform()
        {
            // Set available divisions (always both MA and TC)
            ViewBag.Divisions = new[]
            {
                new { Value = "MA", Name = "Manufacturing" },
                new { Value = "TC", Name = "Technical Center" }
            };

            // Set available platforms for the current division (default to MA)
            var currentDivision = _userContext.GetDivision();
            var platforms = GetPlatformsForDivision(currentDivision);

            ViewBag.Platforms = platforms;
            ViewBag.CurrentDivision = currentDivision.ToString();
            ViewBag.CurrentPlatform = _userContext.GetPlatform().ToString();

            return View();
        }

        /// <summary>
        /// POST: Home/SetDivisionPlatform
        /// Processes the division and platform selection
        /// </summary>
        /// <param name="division">The selected division</param>
        /// <param name="platform">The selected platform</param>
        /// <returns>Redirect to dashboard</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetDivisionPlatform(string division, string platform)
        {
            try
            {
                // Parse the division and platform
                if (!Enum.TryParse(division, out Division selectedDivision))
                {
                    SetErrorMessage("Invalid division selected.");
                    return RedirectToAction("SetDivisionPlatform");
                }

                if (!Enum.TryParse(platform, out Platform selectedPlatform))
                {
                    SetErrorMessage("Invalid platform selected.");
                    return RedirectToAction("SetDivisionPlatform");
                }

                // Validate that the selected platform is valid for the division
                if (!IsValidPlatformForDivision(selectedPlatform, selectedDivision))
                {
                    SetErrorMessage($"Platform {selectedPlatform} is not valid for division {selectedDivision}.");
                    return RedirectToAction("SetDivisionPlatform");
                }

                // Save the preferences
                var username = _userContext.GetUsername();
                await _userPreferenceService.SetPreferredDivisionAsync(username, selectedDivision);
                await _userPreferenceService.SetPreferredPlatformAsync(username, selectedDivision, selectedPlatform);

                // Store the selection in session for this session
                Session["CurrentDivision"] = selectedDivision.ToString();
                Session["CurrentPlatform"] = selectedPlatform.ToString();

                SetSuccessMessage($"Division set to {selectedDivision} and platform set to {selectedPlatform}.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving division and platform preferences");
                SetErrorMessage("There was an error saving your preferences. Please try again.");
                return RedirectToAction("SetDivisionPlatform");
            }
        }

        /// <summary>
        /// GET: Home/About
        /// Shows information about the application
        /// </summary>
        /// <returns>About view</returns>
        public ActionResult About()
        {
            ViewBag.ApplicationVersion = "v2.5.1";
            ViewBag.DatabaseVersion = "v2.5.0";
            ViewBag.LastUpdateDate = "01/02/2025";
            ViewBag.Server = "PROD-APP01";

            return View();
        }

        /// <summary>
        /// GET: Home/Error
        /// Displays a general error page
        /// </summary>
        /// <param name="message">Optional error message</param>
        /// <returns>Error view</returns>
        public ActionResult Error(string message = null)
        {
            ViewBag.ErrorMessage = message ?? "An error occurred while processing your request.";
            return View();
        }

        /// <summary>
        /// Gets the available platforms for a division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>Array of platform value/name pairs</returns>
        private object[] GetPlatformsForDivision(Division division)
        {
            switch (division)
            {
                case Division.MA:
                    return new[]
                    {
                        new { Value = "TEC1", Name = "TEC1" },
                        new { Value = "TEC2", Name = "TEC2" },
                        new { Value = "TEC4I", Name = "TEC4I" }
                    };
                case Division.TC:
                    return new[]
                    {
                        new { Value = "TEC1", Name = "TEC1" },
                        new { Value = "TEC3", Name = "TEC3" },
                        new { Value = "TEC5", Name = "TEC5" }
                    };
                default:
                    return new[]
                    {
                        new { Value = "TEC1", Name = "TEC1" }
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
    }
}