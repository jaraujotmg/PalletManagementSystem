// src/PalletManagementSystem.Web2/Controllers/HomeController.cs
using System;
using System.Web.Mvc;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web2.Services;
using PalletManagementSystem.Web2.ViewModels.Home;

namespace PalletManagementSystem.Web2.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ISessionManager _sessionManager;

        public HomeController(IUserContext userContext, ISessionManager sessionManager)
            : base(userContext)
        {
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }

        public ActionResult Index()
        {
            var viewModel = new HomeViewModel();

            // Set common properties
            viewModel.Username = Username;
            viewModel.DisplayName = Username; // GetDisplayName().Result;
            viewModel.CurrentDivision = UserContext.GetDivision();
            viewModel.CurrentPlatform = UserContext.GetPlatform();
            viewModel.TouchModeEnabled = _sessionManager.IsTouchModeEnabled();

            viewModel.LastLoginDate = DateTime.Now.AddDays(-1).ToString("MMMM dd, yyyy, hh:mm tt");
            viewModel.ApplicationVersion = "v2.5.1";
            viewModel.DatabaseVersion = "v2.5.0";
            viewModel.LastUpdateDate = "01/02/2025";
            viewModel.ServerName = "PROD-APP01";
            viewModel.AllServicesOperational = true;

            return View(viewModel);
        }

        // Add the missing SetDivisionPlatform action method
        public ActionResult SetDivisionPlatform(string division, string platform, string returnUrl)
        {
            // Validate and parse division
            if (!string.IsNullOrEmpty(division) && Enum.TryParse<Division>(division, out Division divisionEnum))
            {
                _sessionManager.SetCurrentDivision(divisionEnum);
            }

            // Validate and parse platform
            if (!string.IsNullOrEmpty(platform) && Enum.TryParse<Platform>(platform, out Platform platformEnum))
            {
                _sessionManager.SetCurrentPlatform(platformEnum);
            }

            // Redirect to the provided return URL or to the home page if not specified
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}