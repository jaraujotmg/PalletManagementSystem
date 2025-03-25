// src/PalletManagementSystem.Web/Controllers/PalletsController.cs
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Web.Models;
using PalletManagementSystem.Web.Services;

namespace PalletManagementSystem.Web.Controllers
{
    public class PalletsController : BaseController
    {
        private readonly IPalletService _palletService;
        private readonly IPlatformValidationService _platformValidationService;

        public PalletsController(
            IUserContextAdapter userContext,
            ISessionManager sessionManager,
            IPalletService palletService,
            IPlatformValidationService platformValidationService)
            : base(userContext, sessionManager)
        {
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
        }

        public async Task<ActionResult> Index(string keyword = null, bool? isClosed = null, int page = 1, int pageSize = 20)
        {
            var division = _sessionManager.GetCurrentDivision();
            var platform = _sessionManager.GetCurrentPlatform();

            var pallets = await _palletService.GetPagedPalletsAsync(
                page,
                pageSize,
                division,
                platform,
                isClosed,
                keyword);

            var viewModel = new PalletListViewModel
            {
                Pallets = pallets,
                SearchKeyword = keyword,
                IsClosed = isClosed,
                PageNumber = page,
                PageSize = pageSize
            };

            PopulateBaseViewModel(viewModel);

            return View(viewModel);
        }

        public async Task<ActionResult> Details(int id)
        {
            var pallet = await _palletService.GetPalletDetailByIdAsync(id);
            if (pallet == null)
            {
                return HttpNotFound();
            }

            var viewModel = new PalletDetailViewModel
            {
                Pallet = pallet
            };

            PopulateBaseViewModel(viewModel);

            return View(viewModel);
        }
    }
}