// src/PalletManagementSystem.Web/Controllers/PalletsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Extensions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web.ViewModels.Pallets;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.Controllers
{
    [Authorize]
    public class PalletsController : BaseController
    {
        private readonly IPalletService _palletService;
        private readonly IItemService _itemService;
        private readonly IPrinterService _printerService;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IPlatformValidationService _platformValidationService;

        public PalletsController(
            IUserContext userContext,
            IPalletService palletService,
            IItemService itemService,
            IPrinterService printerService,
            IUserPreferenceService userPreferenceService,
            IPlatformValidationService platformValidationService)
            : base(userContext)
        {
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException(nameof(platformValidationService));
        }

        // GET: Pallets
        public async Task<ActionResult> Index(string keyword = null, bool? isClosed = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var division = UserContext.GetDivision();
                var platform = UserContext.GetPlatform();

                // Get user preferences for page size if not specified
                if (pageSize <= 0)
                {
                    pageSize = await _userPreferenceService.GetItemsPerPageAsync(Username);
                }

                // Get available platforms for the current division
                var availablePlatforms = await _platformValidationService.GetPlatformsForDivisionAsync(division);

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
                    PageSize = pageSize,
                    SelectedDivision = division,
                    SelectedPlatform = platform,
                    AvailablePlatforms = availablePlatforms.ToList(),

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = division,
                    CurrentPlatform = platform,
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    CanCreatePallet = UserContext.CanEditPallets()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", $"Error retrieving pallets: {ex.Message}");

                return View(new PalletListViewModel
                {
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username)
                });
            }
        }

        // GET: Pallets/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var pallet = await _palletService.GetPalletDetailByIdAsync(id);
                if (pallet == null)
                {
                    return HttpNotFound();
                }

                // Prepare client summary
                var clientSummary = pallet.Items
                    .GroupBy(i => i.ClientName)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Prepare activity log (example - in a real app, this would come from a log repository)
                var activityLogs = new List<ActivityLogItem>
                {
                    new ActivityLogItem
                    {
                        ActivityType = "Created",
                        Description = $"Pallet {pallet.PalletNumber} created",
                        Timestamp = pallet.CreatedDate,
                        Username = pallet.CreatedBy,
                        BadgeClass = "badge-primary",
                        IconClass = "fas fa-pallet"
                    }
                };

                // Add closure log if pallet is closed
                if (pallet.IsClosed)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        ActivityType = "Closed",
                        Description = $"Pallet {pallet.PalletNumber} closed",
                        Timestamp = pallet.ClosedDate.Value,
                        Username = pallet.CreatedBy, // Ideally this would be the user who closed it
                        BadgeClass = "badge-success",
                        IconClass = "fas fa-lock"
                    });
                }

                // Add item activity logs
                foreach (var item in pallet.Items)
                {
                    activityLogs.Add(new ActivityLogItem
                    {
                        ActivityType = "Item Added",
                        Description = $"Item {item.ItemNumber} added to pallet",
                        Timestamp = item.CreatedDate,
                        Username = item.CreatedBy,
                        BadgeClass = "badge-info",
                        IconClass = "fas fa-box"
                    });
                }

                // Sort activity logs by timestamp descending
                activityLogs = activityLogs.OrderByDescending(l => l.Timestamp).ToList();

                var viewModel = new PalletDetailViewModel
                {
                    Pallet = pallet,
                    ClientSummary = clientSummary,
                    ActivityLogs = activityLogs,
                    CanClose = !pallet.IsClosed && UserContext.CanClosePallets(),
                    CanEdit = !pallet.IsClosed && UserContext.CanEditPallets(),
                    CanPrint = true, // Everyone can print

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", $"Error retrieving pallet details: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // GET: Pallets/Create
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Create()
        {
            try
            {
                // Get current division and platform
                var division = UserContext.GetDivision();

                // Get all available platforms for the current division
                var platforms = await _platformValidationService.GetPlatformsForDivisionAsync(division);

                var viewModel = new CreatePalletViewModel
                {
                    // Default to current division and platform
                    Division = division,
                    Platform = UserContext.GetPlatform(),
                    UnitOfMeasure = UnitOfMeasure.PC, // Default to Piece

                    // Populate dropdown options
                    DivisionOptions = Enum.GetValues(typeof(Division))
                        .Cast<Division>()
                        .Select(d => new SelectListItem
                        {
                            Text = d.GetDescription(),
                            Value = d.ToString(),
                            Selected = d == division
                        })
                        .ToList(),

                    PlatformOptions = platforms
                        .Select(p => new SelectListItem
                        {
                            Text = p.GetDescription(),
                            Value = p.ToString(),
                            Selected = p == UserContext.GetPlatform()
                        })
                        .ToList(),

                    UnitOfMeasureOptions = Enum.GetValues(typeof(UnitOfMeasure))
                        .Cast<UnitOfMeasure>()
                        .Select(u => new SelectListItem
                        {
                            Text = u.GetDescription(),
                            Value = u.ToString(),
                            Selected = u == UnitOfMeasure.PC
                        })
                        .ToList(),

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = division,
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    CanEdit = UserContext.CanEditPallets()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = $"Error preparing pallet creation: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Pallets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Create(CreatePalletViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Repopulate dropdown options
                    await RepopulateCreatePalletDropdowns(viewModel);
                    return View(viewModel);
                }

                // Validate platform for division
                bool isValidPlatform = await _platformValidationService.IsValidPlatformForDivisionAsync(
                    viewModel.Platform, viewModel.Division);

                if (!isValidPlatform)
                {
                    ModelState.AddModelError("Platform",
                        $"Platform {viewModel.Platform} is not valid for division {viewModel.Division}");

                    // Repopulate dropdown options
                    await RepopulateCreatePalletDropdowns(viewModel);
                    return View(viewModel);
                }

                // Create the pallet
                var pallet = await _palletService.CreatePalletAsync(
                    viewModel.ManufacturingOrder,
                    viewModel.Division,
                    viewModel.Platform,
                    viewModel.UnitOfMeasure,
                    Username);

                TempData["SuccessMessage"] = $"Pallet {pallet.PalletNumber} created successfully";
                return RedirectToAction("Details", new { id = pallet.Id });
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", $"Error creating pallet: {ex.Message}");

                // Repopulate dropdown options
                await RepopulateCreatePalletDropdowns(viewModel);
                return View(viewModel);
            }
        }

        // Helper method to repopulate dropdowns for create pallet
        private async Task RepopulateCreatePalletDropdowns(CreatePalletViewModel viewModel)
        {
            var platforms = await _platformValidationService.GetPlatformsForDivisionAsync(viewModel.Division);

            viewModel.DivisionOptions = Enum.GetValues(typeof(Division))
                .Cast<Division>()
                .Select(d => new SelectListItem
                {
                    Text = d.GetDescription(),
                    Value = d.ToString(),
                    Selected = d == viewModel.Division
                })
                .ToList();

            viewModel.PlatformOptions = platforms
                .Select(p => new SelectListItem
                {
                    Text = p.GetDescription(),
                    Value = p.ToString(),
                    Selected = p == viewModel.Platform
                })
                .ToList();

            viewModel.UnitOfMeasureOptions = Enum.GetValues(typeof(UnitOfMeasure))
                .Cast<UnitOfMeasure>()
                .Select(u => new SelectListItem
                {
                    Text = u.GetDescription(),
                    Value = u.ToString(),
                    Selected = u == viewModel.UnitOfMeasure
                })
                .ToList();

            // Set common properties
            viewModel.Username = Username;
            viewModel.DisplayName = await GetDisplayName();
            viewModel.CurrentDivision = UserContext.GetDivision();
            viewModel.CurrentPlatform = UserContext.GetPlatform();
            viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
            viewModel.CanEdit = UserContext.CanEditPallets();
        }

        // GET: Pallets/Close/5
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Close(int id)
        {
            try
            {
                var pallet = await _palletService.GetPalletByIdAsync(id);
                if (pallet == null)
                {
                    return HttpNotFound();
                }

                if (pallet.IsClosed)
                {
                    TempData["ErrorMessage"] = "Pallet is already closed";
                    return RedirectToAction("Details", new { id });
                }

                var viewModel = new ClosePalletViewModel
                {
                    PalletId = pallet.Id,
                    PalletNumber = pallet.PalletNumber,
                    ManufacturingOrder = pallet.ManufacturingOrder,
                    ItemCount = pallet.ItemCount,

                    // Default to printing based on user preferences
                    PrintPalletList = await _userPreferenceService.GetAllPreferencesAsync(Username)
                        .ContinueWith(t => t.Result.AutoPrintPalletList),

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    CanEdit = UserContext.CanEditPallets()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = $"Error preparing pallet closure: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Pallets/Close
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Close(ClosePalletViewModel viewModel)
        {
            try
            {
                if (!viewModel.IsValid())
                {
                    ModelState.AddModelError("ConfirmationText", "Please type 'CLOSE' to confirm");

                    // Set common properties
                    viewModel.Username = Username;
                    viewModel.DisplayName = await GetDisplayName();
                    viewModel.CurrentDivision = UserContext.GetDivision();
                    viewModel.CurrentPlatform = UserContext.GetPlatform();
                    viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
                    viewModel.CanEdit = UserContext.CanEditPallets();

                    return View(viewModel);
                }

                // Close the pallet
                var closedPallet = await _palletService.ClosePalletAsync(
                    viewModel.PalletId, viewModel.PrintPalletList);

                TempData["SuccessMessage"] = $"Pallet {closedPallet.PalletNumber} closed successfully";

                // Redirect to details
                return RedirectToAction("Details", new { id = viewModel.PalletId });
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", $"Error closing pallet: {ex.Message}");

                // Set common properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
                viewModel.CanEdit = UserContext.CanEditPallets();

                return View(viewModel);
            }
        }

        // GET: Pallets/Print/5
        public async Task<ActionResult> Print(int id)
        {
            try
            {
                var pallet = await _palletService.GetPalletDetailByIdAsync(id);
                if (pallet == null)
                {
                    return HttpNotFound();
                }

                // Get available printers
                var availablePrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);

                // Get default printer for the user
                var defaultPrinter = await _printerService.GetDefaultPalletListPrinterAsync(Username);

                var viewModel = new PrintPalletViewModel
                {
                    PalletId = pallet.Id,
                    Pallet = pallet,
                    PrinterName = defaultPrinter,
                    AvailablePrinters = availablePrinters
                        .Select(p => new SelectListItem
                        {
                            Text = p,
                            Value = p,
                            Selected = p == defaultPrinter
                        })
                        .ToList(),

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    CanEdit = UserContext.CanEditPallets()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = $"Error preparing print: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Pallets/Print
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Print(PrintPalletViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Repopulate printers
                    var availablePrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);
                    viewModel.AvailablePrinters = availablePrinters
                        .Select(p => new SelectListItem
                        {
                            Text = p,
                            Value = p,
                            Selected = p == viewModel.PrinterName
                        })
                        .ToList();

                    // Get the pallet details again
                    viewModel.Pallet = await _palletService.GetPalletDetailByIdAsync(viewModel.PalletId);

                    // Set common properties
                    viewModel.Username = Username;
                    viewModel.DisplayName = await GetDisplayName();
                    viewModel.CurrentDivision = UserContext.GetDivision();
                    viewModel.CurrentPlatform = UserContext.GetPlatform();
                    viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
                    viewModel.CanEdit = UserContext.CanEditPallets();

                    return View(viewModel);
                }

                // Save as default if requested
                if (viewModel.SaveAsDefault)
                {
                    await _printerService.SetDefaultPalletListPrinterAsync(Username, viewModel.PrinterName);
                }

                // Print the pallet list
                bool success = await _printerService.PrintPalletListAsync(viewModel.PalletId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Pallet list printed successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error printing pallet list";
                }

                // Redirect to details
                return RedirectToAction("Details", new { id = viewModel.PalletId });
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", $"Error printing pallet list: {ex.Message}");

                // Repopulate printers
                var availablePrinters = await _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);
                viewModel.AvailablePrinters = availablePrinters
                    .Select(p => new SelectListItem
                    {
                        Text = p,
                        Value = p,
                        Selected = p == viewModel.PrinterName
                    })
                    .ToList();

                // Get the pallet details again
                viewModel.Pallet = await _palletService.GetPalletDetailByIdAsync(viewModel.PalletId);

                // Set common properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
                viewModel.CanEdit = UserContext.CanEditPallets();

                return View(viewModel);
            }
        }

        // GET: Pallets/MoveItem/5 (itemId)
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> MoveItem(int id)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                // Check if item can be moved (not in a closed pallet)
                if (item.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot move item from a closed pallet";
                    return RedirectToAction("Details", "Items", new { id });
                }

                // Get all open pallets except the current pallet
                var division = UserContext.GetDivision();
                var platform = UserContext.GetPlatform();

                var pallets = await _palletService.GetPagedPalletsAsync(
                    1, 100, division, platform, false); // Only open pallets

                // Filter out current pallet
                var availablePallets = pallets.Items
                    .Where(p => p.Id != item.PalletId)
                    .ToList();

                var viewModel = new MovePalletItemViewModel
                {
                    ItemId = item.Id,
                    ItemNumber = item.ItemNumber,
                    Item = item,
                    SourcePalletId = item.PalletId,
                    SourcePalletNumber = item.Pallet?.PalletNumber,
                    AvailablePallets = availablePallets,
                    CreateNewPallet = false,
                    NewPalletManufacturingOrder = item.ManufacturingOrder, // Default to same order
                    NewPalletUnitOfMeasure = item.QuantityUnit, // Default to same unit

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    CanEdit = UserContext.CanEditPallets()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = $"Error preparing to move item: {ex.Message}";
                return RedirectToAction("Details", "Items", new { id });
            }
        }

        // POST: Pallets/MoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> MoveItem(MovePalletItemViewModel viewModel)
        {
            try
            {
                if (viewModel.CreateNewPallet)
                {
                    // Create new pallet first
                    if (string.IsNullOrWhiteSpace(viewModel.NewPalletManufacturingOrder))
                    {
                        ModelState.AddModelError("NewPalletManufacturingOrder", "Manufacturing order is required for new pallet");

                        // Repopulate available pallets
                        await RepopulateMovePalletItemViewModel(viewModel);
                        return View(viewModel);
                    }

                    // Parse unit of measure
                    if (string.IsNullOrWhiteSpace(viewModel.NewPalletUnitOfMeasure) ||
                        !Enum.TryParse<UnitOfMeasure>(viewModel.NewPalletUnitOfMeasure, out var unitOfMeasure))
                    {
                        ModelState.AddModelError("NewPalletUnitOfMeasure", "Valid unit of measure is required");

                        // Repopulate available pallets
                        await RepopulateMovePalletItemViewModel(viewModel);
                        return View(viewModel);
                    }

                    var division = UserContext.GetDivision();
                    var platform = UserContext.GetPlatform();

                    // Create the new pallet
                    var newPallet = await _palletService.CreatePalletAsync(
                        viewModel.NewPalletManufacturingOrder,
                        division,
                        platform,
                        unitOfMeasure,
                        Username);

                    if (viewModel.MoveToNewPallet)
                    {
                        // Set target pallet to the new pallet
                        viewModel.TargetPalletId = newPallet.Id;
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"New pallet {newPallet.PalletNumber} created successfully";
                        return RedirectToAction("Details", new { id = newPallet.Id });
                    }
                }
                else if (viewModel.TargetPalletId <= 0)
                {
                    ModelState.AddModelError("TargetPalletId", "Please select a target pallet");

                    // Repopulate available pallets
                    await RepopulateMovePalletItemViewModel(viewModel);
                    return View(viewModel);
                }

                // Check if item can be moved
                bool canMove = await _itemService.CanMoveItemToPalletAsync(
                    viewModel.ItemId, viewModel.TargetPalletId);

                if (!canMove)
                {
                    ModelState.AddModelError("", "Cannot move item to the selected pallet");

                    // Repopulate available pallets
                    await RepopulateMovePalletItemViewModel(viewModel);
                    return View(viewModel);
                }

                // Move the item
                var movedItem = await _itemService.MoveItemToPalletAsync(
                    viewModel.ItemId, viewModel.TargetPalletId);

                // Determine where to redirect based on where the item was moved from
                TempData["SuccessMessage"] = $"Item {movedItem.ItemNumber} moved successfully";

                if (viewModel.SourcePalletId > 0)
                {
                    return RedirectToAction("Details", new { id = viewModel.SourcePalletId });
                }
                else
                {
                    return RedirectToAction("Details", "Items", new { id = viewModel.ItemId });
                }
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", $"Error moving item: {ex.Message}");

                // Repopulate available pallets
                await RepopulateMovePalletItemViewModel(viewModel);
                return View(viewModel);
            }
        }

        // Helper method to repopulate move item view model
        private async Task RepopulateMovePalletItemViewModel(MovePalletItemViewModel viewModel)
        {
            // Get the item
            viewModel.Item = await _itemService.GetItemDetailByIdAsync(viewModel.ItemId);

            // Get available pallets
            var division = UserContext.GetDivision();
            var platform = UserContext.GetPlatform();

            var pallets = await _palletService.GetPagedPalletsAsync(
                1, 100, division, platform, false); // Only open pallets

            // Filter out current pallet
            viewModel.AvailablePallets = pallets.Items
                .Where(p => p.Id != viewModel.SourcePalletId)
                .ToList();

            // Set common properties
            viewModel.Username = Username;
            viewModel.DisplayName = await GetDisplayName();
            viewModel.CurrentDivision = UserContext.GetDivision();
            viewModel.CurrentPlatform = UserContext.GetPlatform();
            viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
            viewModel.CanEdit = UserContext.CanEditPallets();
        }

        // POST: Pallets/Filter
        [HttpPost]
        public async Task<ActionResult> Filter(PalletFilterViewModel filter)
        {
            try
            {
                // Apply the filter
                return RedirectToAction("Index", new
                {
                    keyword = filter.Keyword,
                    isClosed = filter.IsClosed,
                    page = 1, // Reset to first page
                    pageSize = await _userPreferenceService.GetItemsPerPageAsync(Username)
                });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error applying filter: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Pallets/Search
        [HttpPost]
        public async Task<JsonResult> Search(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                {
                    return Json(new { success = false, message = "Search term must be at least 2 characters" });
                }

                var division = UserContext.GetDivision();
                var platform = UserContext.GetPlatform();

                // Search for pallets
                var pallets = await _palletService.SearchPalletsAsync(term);

                // Filter by division and platform
                pallets = pallets.Where(p =>
                    p.Division == division.ToString() &&
                    p.Platform == platform.ToString()).ToList();

                // Return results
                var results = pallets.Select(p => new
                {
                    id = p.Id,
                    text = p.PalletNumber,
                    info = $"MO: {p.ManufacturingOrder}, Items: {p.ItemCount}, " +
                           $"Status: {(p.IsClosed ? "Closed" : "Open")}"
                }).ToList();

                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}