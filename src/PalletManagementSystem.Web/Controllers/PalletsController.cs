using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Logging;
using PalletManagementSystem.Web.ViewModels.PalletViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller for pallet operations
    /// </summary>
    [Authorize]
    public class PalletsController : BaseController
    {
        private readonly IPalletService _palletService;
        private readonly IItemService _itemService;
        private readonly IPrinterService _printerService;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly UserContext _userContext;
        private readonly LoggingService _loggingService;
        private readonly ILogger<PalletsController> _logger;

        /// <summary>
        /// Initializes a new instance of PalletsController
        /// </summary>
        public PalletsController(
            IPalletService palletService,
            IItemService itemService,
            IPrinterService printerService,
            IUserPreferenceService userPreferenceService,
            UserContext userContext,
            LoggingService loggingService,
            ILogger<PalletsController> logger)
        {
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: Pallets
        /// Displays a list of all pallets with filtering options
        /// </summary>
        public async Task<ActionResult> Index(int page = 1, string status = "all", string searchTerm = null)
        {
            try
            {
                // Get current division and platform
                var division = _userContext.GetDivision();
                var platform = _userContext.GetPlatform();

                // Get user preference for page size
                var username = _userContext.GetUsername();
                int pageSize = await _userPreferenceService.GetItemsPerPageAsync(username);

                // Get pallets according to filters
                var pallets = await GetFilteredPalletsAsync(division, platform, status, searchTerm);

                // Create the view model
                var viewModel = PalletListViewModel.FromPalletDtos(
                    pallets,
                    page,
                    pageSize,
                    status,
                    searchTerm,
                    division,
                    platform);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pallet list");
                SetErrorMessage("An error occurred while retrieving the pallet list. Please try again.");
                return View(new PalletListViewModel());
            }
        }

        /// <summary>
        /// GET: Pallets/Details/5
        /// Shows details for a specific pallet
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                // Get the pallet with items
                var pallet = await _palletService.GetPalletWithItemsAsync(id);
                if (pallet == null)
                {
                    SetErrorMessage("Pallet not found.");
                    return RedirectToAction("Index");
                }

                // Create the view model
                var viewModel = PalletDetailsViewModel.FromPalletDto(pallet);

                // Log the view action
                _loggingService.LogAudit("View", "Pallet", pallet.Id.ToString(),
                    $"Viewed pallet {pallet.PalletNumber}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pallet details for ID {id}");
                SetErrorMessage("An error occurred while retrieving the pallet details. Please try again.");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// GET: Pallets/Create
        /// Shows form to create a new pallet
        /// </summary>
        public ActionResult Create()
        {
            try
            {
                // Check if user has permission to create pallets
                if (!_userContext.CanEditPallets())
                {
                    SetErrorMessage("You do not have permission to create new pallets.");
                    return RedirectToAction("Index");
                }

                // Get touch mode status
                bool touchModeEnabled = Session["TouchModeEnabled"] != null &&
                                       (bool)Session["TouchModeEnabled"];

                // Create view model with user's current division and platform
                var viewModel = new CreatePalletViewModel(
                    _userContext.GetDivision(),
                    _userContext.GetPlatform(),
                    touchModeEnabled);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error displaying pallet creation form");
                SetErrorMessage("An error occurred while loading the pallet creation form. Please try again.");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: Pallets/Create
        /// Processes the pallet creation form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreatePalletViewModel model)
        {
            try
            {
                // Check if user has permission to create pallets
                if (!_userContext.CanEditPallets())
                {
                    SetErrorMessage("You do not have permission to create new pallets.");
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    // Parse enums from strings
                    if (!Enum.TryParse(model.Division, out Division division) ||
                        !Enum.TryParse(model.Platform, out Platform platform) ||
                        !Enum.TryParse(model.UnitOfMeasure, out UnitOfMeasure unitOfMeasure))
                    {
                        ModelState.AddModelError("", "Invalid division, platform, or unit of measure selection.");
                        return View(model);
                    }

                    // Create the pallet
                    var pallet = await _palletService.CreatePalletAsync(
                        model.ManufacturingOrder,
                        division,
                        platform,
                        unitOfMeasure,
                        _userContext.GetUsername());

                    // Log the creation
                    _loggingService.LogAudit("Create", "Pallet", pallet.Id.ToString(),
                        $"Created pallet {pallet.PalletNumber} for manufacturing order {pallet.ManufacturingOrder}");

                    SetSuccessMessage($"Pallet {pallet.PalletNumber} created successfully.");
                    return RedirectToAction("Details", new { id = pallet.Id });
                }

                // If we got here, there was a validation error
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pallet");
                ModelState.AddModelError("", "An error occurred while creating the pallet. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Pallets/Close/5
        /// Shows confirmation form to close a pallet
        /// </summary>
        public async Task<ActionResult> Close(int id)
        {
            try
            {
                // Check if user has permission to close pallets
                if (!_userContext.CanClosePallets())
                {
                    SetErrorMessage("You do not have permission to close pallets.");
                    return RedirectToAction("Details", new { id });
                }

                // Get the pallet to verify it exists
                var pallet = await _palletService.GetPalletByIdAsync(id);
                if (pallet == null)
                {
                    SetErrorMessage("Pallet not found.");
                    return RedirectToAction("Index");
                }

                // Check if already closed
                if (pallet.IsClosed)
                {
                    SetErrorMessage($"Pallet {pallet.PalletNumber} is already closed.");
                    return RedirectToAction("Details", new { id });
                }

                // Create close pallet view model
                var viewModel = new ClosePalletViewModel(
                    pallet.Id,
                    pallet.PalletNumber,
                    pallet.IsTemporary,
                    pallet.ManufacturingOrder,
                    pallet.ItemCount,
                    pallet.Quantity,
                    pallet.UnitOfMeasure);

                // Get auto-print preference
                var username = _userContext.GetUsername();
                var preferences = await _userPreferenceService.GetAllPreferencesAsync(username);
                viewModel.AutoPrint = preferences.AutoPrintPalletList;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading close pallet form for ID {id}");
                SetErrorMessage("An error occurred while loading the close pallet form. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// POST: Pallets/Close
        /// Processes the close pallet form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CloseConfirm(ClosePalletViewModel model)
        {
            try
            {
                // Check if user has permission to close pallets
                if (!_userContext.CanClosePallets())
                {
                    SetErrorMessage("You do not have permission to close pallets.");
                    return RedirectToAction("Details", new { id = model.Id });
                }

                if (ModelState.IsValid)
                {
                    // Verify user confirms understanding
                    if (!model.ConfirmUnderstanding)
                    {
                        ModelState.AddModelError("ConfirmUnderstanding",
                            "You must confirm that you understand this action cannot be undone.");
                        return View("Close", model);
                    }

                    // Get the pallet to verify it exists
                    var pallet = await _palletService.GetPalletByIdAsync(model.Id);
                    if (pallet == null)
                    {
                        SetErrorMessage("Pallet not found.");
                        return RedirectToAction("Index");
                    }

                    // Check if already closed
                    if (pallet.IsClosed)
                    {
                        SetErrorMessage($"Pallet {pallet.PalletNumber} is already closed.");
                        return RedirectToAction("Details", new { id = model.Id });
                    }

                    // Close the pallet
                    var closedPallet = await _palletService.ClosePalletAsync(model.Id);

                    // Log the close action
                    _loggingService.LogAudit("Close", "Pallet", closedPallet.Id.ToString(),
                        $"Closed pallet {closedPallet.PalletNumber} with notes: {model.Notes ?? "None"}");

                    // Auto-print if requested
                    if (model.AutoPrint)
                    {
                        await _printerService.PrintPalletListAsync(model.Id);
                    }

                    // Check if number changed (temporary to permanent)
                    if (pallet.PalletNumber != closedPallet.PalletNumber)
                    {
                        SetSuccessMessage($"Pallet closed successfully. Number changed from {pallet.PalletNumber} to {closedPallet.PalletNumber}.");
                    }
                    else
                    {
                        SetSuccessMessage($"Pallet {closedPallet.PalletNumber} closed successfully.");
                    }

                    return RedirectToAction("Details", new { id = model.Id });
                }

                // If we got here, there was a validation error
                return View("Close", model);
            }
            catch (PalletClosedException pcEx)
            {
                _logger.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing pallet with ID {model.Id}");
                SetErrorMessage("An error occurred while closing the pallet. Please try again.");
                return RedirectToAction("Details", new { id = model.Id });
            }
        }

        /// <summary>
        /// GET: Pallets/Print/5
        /// Prints a pallet list
        /// </summary>
        public async Task<ActionResult> Print(int id)
        {
            try
            {
                // Get the pallet to verify it exists
                var pallet = await _palletService.GetPalletByIdAsync(id);
                if (pallet == null)
                {
                    SetErrorMessage("Pallet not found.");
                    return RedirectToAction("Index");
                }

                // Print the pallet list
                await _printerService.PrintPalletListAsync(id);

                // Log the print action
                _loggingService.LogAudit("Print", "Pallet", pallet.Id.ToString(),
                    $"Printed list for pallet {pallet.PalletNumber}");

                SetSuccessMessage($"Print job for pallet {pallet.PalletNumber} sent successfully.");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing pallet list for ID {id}");
                SetErrorMessage("An error occurred while printing the pallet list. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// Helper method to get filtered pallets
        /// </summary>
        private async Task<PalletDto[]> GetFilteredPalletsAsync(
            Division division,
            Platform platform,
            string status,
            string searchTerm)
        {
            // If search term provided, use search service
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                return (await _palletService.SearchPalletsAsync(searchTerm)).ToArray();
            }

            // Otherwise get by division and platform
            var pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);

            // Filter by status if specified
            if (status == "open")
            {
                return pallets.Where(p => !p.IsClosed).ToArray();
            }
            else if (status == "closed")
            {
                return pallets.Where(p => p.IsClosed).ToArray();
            }

            return pallets.ToArray();
        }
    }
}