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
    /// Controller for home page and dashboard functionality
    /// </summary>
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IPalletService _palletService;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly UserContext _userContext;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the HomeController
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
        /// Dashboard page showing recent activity and system information
        /// </summary>
        public async Task<ActionResult> Index()
        {
            try
            {
                // Get user preferences
                var username = _userContext.GetUsername();

                // Get the currently selected division and platform
                var division = _userContext.GetDivision();
                var platform = _userContext.GetPlatform();

                // Get recent pallets for the dashboard
                var pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
                var recentPallets = pallets.OrderByDescending(p => p.CreatedDate).Take(5).ToList();
                ViewBag.RecentPallets = recentPallets;

                // Add system information
                ViewBag.ApplicationVersion = "v2.5.1";
                ViewBag.DatabaseVersion = "v2.5.0";
                ViewBag.LastUpdateDate = "01/02/2025";
                ViewBag.Server = "PROD-APP01";

                // Add statistics
                ViewBag.OpenPallets = pallets.Count(p => !p.IsClosed);
                ViewBag.ClosedPallets = pallets.Count(p => p.IsClosed);
                ViewBag.TotalPallets = pallets.Count();

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
        /// GET: Home/DivisionSelector
        /// Displays the division and platform selection page
        /// </summary>
        public ActionResult DivisionSelector()
        {
            // Get the current division and platform
            var currentDivision = _userContext.GetDivision();
            var currentPlatform = _userContext.GetPlatform();

            ViewBag.CurrentDivision = currentDivision.ToString();
            ViewBag.CurrentPlatform = currentPlatform.ToString();

            return View();
        }

        /// <summary>
        /// POST: Home/SetDivision
        /// Sets the division and platform for the current session
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetDivision(string division, string platform)
        {
            try
            {
                // Parse the division and platform
                if (!Enum.TryParse(division, out Division selectedDivision))
                {
                    SetErrorMessage("Invalid division selected.");
                    return RedirectToAction("DivisionSelector");
                }

                if (!Enum.TryParse(platform, out Platform selectedPlatform))
                {
                    SetErrorMessage("Invalid platform selected.");
                    return RedirectToAction("DivisionSelector");
                }

                // Validate that the platform is valid for the division
                if (!IsValidPlatformForDivision(selectedPlatform, selectedDivision))
                {
                    SetErrorMessage($"Platform {selectedPlatform} is not valid for division {selectedDivision}.");
                    return RedirectToAction("DivisionSelector");
                }

                // Save user preferences
                var username = _userContext.GetUsername();
                await _userPreferenceService.SetPreferredDivisionAsync(username, selectedDivision);
                await _userPreferenceService.SetPreferredPlatformAsync(username, selectedDivision, selectedPlatform);

                // Store in session
                Session["CurrentDivision"] = selectedDivision.ToString();
                Session["CurrentPlatform"] = selectedPlatform.ToString();

                SetSuccessMessage($"Division set to {selectedDivision} and platform set to {selectedPlatform}.");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting division and platform");
                SetErrorMessage("There was an error setting your division and platform. Please try again.");
                return RedirectToAction("DivisionSelector");
            }
        }

        /// <summary>
        /// GET: Home/About
        /// Displays information about the application
        /// </summary>
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
        public ActionResult Error(string message = null)
        {
            ViewBag.ErrorMessage = message ?? "An error occurred while processing your request.";
            return View();
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
    }
}