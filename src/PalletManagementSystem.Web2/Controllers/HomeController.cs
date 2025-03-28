// src/PalletManagementSystem.Web/Controllers/HomeController.cs
using System;
using System.Reflection;
using System.Web.Mvc;
using PalletManagementSystem.Web2.ViewModels;
using PalletManagementSystem.Web2.Services;

namespace PalletManagementSystem.Web2.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IUserContextAdapter userContext, ISessionManager sessionManager)
            : base(userContext, sessionManager)
        {
        }

        public ActionResult Index()
        {
            var viewModel = new HomeViewModel();
            PopulateBaseViewModel(viewModel);

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