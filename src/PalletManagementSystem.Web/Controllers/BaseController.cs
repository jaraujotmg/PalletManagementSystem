// src/PalletManagementSystem.Web/Controllers/BaseController.cs
using System;
using System.Web.Mvc;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Web.Services;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUserContextAdapter _userContext;
        protected readonly ISessionManager _sessionManager;

        protected BaseController(IUserContextAdapter userContext, ISessionManager sessionManager)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        }

        protected void PopulateBaseViewModel(ViewModelBase viewModel)
        {
            viewModel.Username = _userContext.GetUsername();
            viewModel.DisplayName = _userContext.GetDisplayName();
            viewModel.CanEdit = _userContext.CanEditPallets();
            viewModel.CurrentDivision = _sessionManager.GetCurrentDivision();
            viewModel.CurrentPlatform = _sessionManager.GetCurrentPlatform();
            viewModel.TouchModeEnabled = _sessionManager.IsTouchModeEnabled();
        }

        [HttpPost]
        public ActionResult SwitchDivision(Division division)
        {
            _sessionManager.SetCurrentDivision(division);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult SwitchPlatform(Platform platform)
        {
            _sessionManager.SetCurrentPlatform(platform);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult ToggleTouchMode(bool enabled)
        {
            _sessionManager.SetTouchModeEnabled(enabled);
            return RedirectToAction(Request.UrlReferrer.ToString());
        }
    }
}