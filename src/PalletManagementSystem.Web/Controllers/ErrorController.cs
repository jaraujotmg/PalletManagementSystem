// src/PalletManagementSystem.Web/Controllers/ErrorController.cs
using System;
using System.Web.Mvc;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web.Services;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.Controllers
{
    public class ErrorController : BaseController
    {
        private readonly IUserPreferenceService _userPreferenceService;

        public ErrorController(
            IUserContext userContext,
            IUserPreferenceService userPreferenceService)
            : base(userContext)
        {
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
        }

        // General error page
        public ActionResult Index()
        {
            Response.StatusCode = 500;
            return View("Error", CreateErrorViewModel("An error occurred"));
        }

        // 404 - Not Found
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View(CreateErrorViewModel("The page you requested was not found"));
        }

        // 500 - Server Error
        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            return View("Error", CreateErrorViewModel("A server error occurred"));
        }

        // 403 - Forbidden
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            return View("Error", CreateErrorViewModel("You don't have permission to access this resource"));
        }

        // Helper method to create error view model
        private ErrorViewModel CreateErrorViewModel(string message)
        {
            var viewModel = new ErrorViewModel
            {
                ErrorMessage = message,
                RequestedUrl = Request.RawUrl,
                ReferrerUrl = Request.UrlReferrer?.ToString()
            };

            // Populate common view model properties
            try
            {
                viewModel.Username = Username;
                viewModel.DisplayName = GetDisplayName().Result;
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = _userPreferenceService.GetTouchModeEnabledAsync(Username).Result;
            }
            catch
            {
                // If we can't get user context, continue with default values
            }

            return viewModel;
        }
    }
}