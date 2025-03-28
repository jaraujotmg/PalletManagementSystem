// src/PalletManagementSystem.Web2/Controllers/HomeController.cs
using System;
using System.Web.Mvc;
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
            //viewModel.DisplayName = GetDisplayName().Result; //NOT WORKING
            viewModel.DisplayName = Username;
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
    }
}