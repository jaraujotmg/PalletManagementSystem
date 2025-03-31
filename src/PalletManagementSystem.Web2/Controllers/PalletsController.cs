// src/PalletManagementSystem.Web2/Controllers/PalletsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc; // Using System.Web.Mvc
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Extensions; // For EnumExtensions
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web2.Services;       // For ISessionManager
using PalletManagementSystem.Web2.ViewModels.Pallets;
using PalletManagementSystem.Web2.ViewModels.Shared; // For ActivityLogItem, BaseViewModel

namespace PalletManagementSystem.Web2.Controllers
{
    // [Authorize] // Keep commented until auth fully integrated
    public class PalletsController : BaseController
    {
        private readonly IPalletService _palletService;
        private readonly IItemService _itemService;
        private readonly IPrinterService _printerService;
        private readonly IUserPreferenceService _userPreferenceService;
        private readonly IPlatformValidationService _platformValidationService;
        private readonly ISessionManager _sessionManager;

        public PalletsController(
            IUserContext userContext,
            IPalletService palletService,
            IItemService itemService,
            IPrinterService printerService,
            IUserPreferenceService userPreferenceService,
            IPlatformValidationService platformValidationService,
            ISessionManager sessionManager)
            : base(userContext) // BaseController constructor uses IUserContext
        {
            _palletService = palletService ?? throw new ArgumentNullException("palletService");
            _itemService = itemService ?? throw new ArgumentNullException("itemService");
            _printerService = printerService ?? throw new ArgumentNullException("printerService");
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException("userPreferenceService");
            _platformValidationService = platformValidationService ?? throw new ArgumentNullException("platformValidationService");
            _sessionManager = sessionManager ?? throw new ArgumentNullException("sessionManager");
        }

        // GET: Pallets
        public async Task<ActionResult> Index(
            // Use Nullable<T> for optional value type parameters in .NET Framework
            Nullable<Division> divisionParam = null,
            Nullable<Platform> platformParam = null,
            string keyword = null,
            Nullable<bool> isClosed = null,
            int page = 1,
            int pageSize = 0) // Default pageSize to trigger preference lookup
        {
            // --- Variable Initialization ---
            Division divisionToUse;
            Platform platformToUse;
            Platform defaultPlatform = Platform.TEC1;
            List<Platform> availablePlatforms = new List<Platform>();
            PagedResultDto<PalletListDto> palletsResult = null;
            string errorMessage = null;

            // Declare variables for awaited values
            string displayName = Username; // Default
            bool canCreatePallet = false; // Default
            bool touchModeEnabled = false; // Default
            int resolvedPageSize = 20; // Default page size

            try
            {
                // --- Determine Division & Platform (Sync Getters) ---
                divisionToUse = divisionParam.HasValue ? divisionParam.Value : _sessionManager.GetCurrentDivision();
                platformToUse = platformParam.HasValue ? platformParam.Value : _sessionManager.GetCurrentPlatform();

                // --- Validate/Correct Platform (Async) ---
                availablePlatforms = (await _platformValidationService.GetPlatformsForDivisionAsync(divisionToUse)).ToList();
                if (!availablePlatforms.Contains(platformToUse))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Warning: Platform '{0}' invalid for Division '{1}'. Resetting.", platformToUse, divisionToUse));
                    platformToUse = await _platformValidationService.GetDefaultPlatformForDivisionAsync(divisionToUse);
                    // Update session async (fire and forget - simple approach)
                    var ignoredTask = _sessionManager.SetCurrentPlatformAsync(platformToUse);
                }

                // --- Get Page Size (Async) ---
                if (pageSize <= 0)
                {
                    try { resolvedPageSize = await _userPreferenceService.GetItemsPerPageAsync(Username); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Error getting page size preference: " + ex.Message); }
                    if (resolvedPageSize <= 0) resolvedPageSize = 20; // Ensure valid default
                }
                else
                {
                    resolvedPageSize = pageSize;
                }

                // --- Get Pallet Data (Async) ---
                palletsResult = await _palletService.GetPagedPalletsAsync(
                    page, resolvedPageSize, divisionToUse, platformToUse, isClosed, keyword);

                // Ensure result object and items list are not null
                if (palletsResult == null)
                {
                    palletsResult = new PagedResultDto<PalletListDto> { Items = new List<PalletListDto>() };
                }
                palletsResult.Items = palletsResult.Items ?? new List<PalletListDto>();
                palletsResult.PageNumber = page;
                palletsResult.PageSize = resolvedPageSize;


                // --- Get Async UI Data Concurrently ---
                var displayNameTask = GetDisplayName();
                var canCreatePalletTask = UserContext.CanEditPalletsAsync();

                await Task.WhenAll(displayNameTask, canCreatePalletTask);

                displayName = await displayNameTask;
                canCreatePallet = await canCreatePalletTask;
                touchModeEnabled = _sessionManager.IsTouchModeEnabled(); // Sync Session Getter

                // --- Populate ViewModel ---
                var viewModel = new PalletListViewModel
                {
                    Pallets = palletsResult,
                    SearchKeyword = keyword,
                    IsClosed = isClosed,
                    PageNumber = page,
                    PageSize = resolvedPageSize,
                    SelectedDivision = divisionToUse,
                    SelectedPlatform = platformToUse,
                    AvailablePlatforms = availablePlatforms,

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = divisionToUse,
                    CurrentPlatform = platformToUse,
                    TouchModeEnabled = touchModeEnabled,
                    CanCreatePallet = canCreatePallet
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error retrieving pallets for Index: {0}", ex));
                errorMessage = string.Format("An error occurred while retrieving pallets: {0}. Please try again or contact support.", ex.Message);
                ModelState.AddModelError("", errorMessage);

                // Build fallback view model safely
                Division errorDivision = divisionParam.HasValue ? divisionParam.Value : Division.MA;
                Platform errorPlatform = platformParam.HasValue ? platformParam.Value : defaultPlatform;
                try { availablePlatforms = (await _platformValidationService.GetPlatformsForDivisionAsync(errorDivision)).ToList(); } catch { availablePlatforms = new List<Platform>(); }
                pageSize = resolvedPageSize > 0 ? resolvedPageSize : 20; // Use calculated or default

                try { displayName = await GetDisplayName(); } catch { displayName = Username; }
                try { canCreatePallet = await UserContext.CanEditPalletsAsync(); } catch { canCreatePallet = false; }
                try { touchModeEnabled = _sessionManager.IsTouchModeEnabled(); } catch { touchModeEnabled = false; }

                var errorViewModel = new PalletListViewModel
                {
                    Pallets = new PagedResultDto<PalletListDto> { Items = new List<PalletListDto>(), PageNumber = page, PageSize = pageSize, TotalCount = 0 },
                    SearchKeyword = keyword,
                    IsClosed = isClosed,
                    PageNumber = page,
                    PageSize = pageSize,
                    SelectedDivision = errorDivision,
                    SelectedPlatform = errorPlatform,
                    AvailablePlatforms = availablePlatforms,
                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = errorDivision,
                    CurrentPlatform = errorPlatform,
                    TouchModeEnabled = touchModeEnabled,
                    CanCreatePallet = canCreatePallet,
                    ErrorMessage = errorMessage
                };

                return View(errorViewModel);
            }
        }

        // GET: Pallets/Details/5
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                var palletTask = _palletService.GetPalletDetailByIdAsync(id);
                var canClosePalletTask = UserContext.CanClosePalletsAsync();
                var canEditPalletTask = UserContext.CanEditPalletsAsync();
                var displayNameTask = GetDisplayName();
                // Assuming GetTouchModeEnabledAsync exists and is preferable
                var touchModeEnabledTask = _userPreferenceService.GetTouchModeEnabledAsync(Username);

                var pallet = await palletTask;
                if (pallet == null) return HttpNotFound();

                // Await the UI/permission data
                bool canClose = !pallet.IsClosed && await canClosePalletTask;
                bool canEdit = !pallet.IsClosed && await canEditPalletTask;
                string displayName = await displayNameTask;
                bool touchModeEnabled = false; // Default
                try { touchModeEnabled = await touchModeEnabledTask; } catch { /* Log */ }


                // Prepare sync data
                var clientSummary = pallet.Items.GroupBy(i => i.ClientName).ToDictionary(g => g.Key, g => g.Count());
                var activityLogs = CreateActivityLog(pallet); // Sync helper

                var viewModel = new PalletDetailViewModel
                {
                    Pallet = pallet,
                    ClientSummary = clientSummary,
                    ActivityLogs = activityLogs,
                    CanClose = canClose,
                    CanEdit = canEdit,
                    CanPrint = true,

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = _sessionManager.GetCurrentDivision(), // Sync Get
                    CurrentPlatform = _sessionManager.GetCurrentPlatform(), // Sync Get
                    TouchModeEnabled = touchModeEnabled
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error retrieving pallet details {0}: {1}", id, ex));
                TempData["ErrorMessage"] = string.Format("Error retrieving pallet details: {0}", ex.Message);
                return RedirectToAction("Index");
            }
        }

        // Helper to create activity log (remains synchronous)
        private List<ActivityLogItem> CreateActivityLog(PalletDetailDto pallet)
        {
            var activityLogs = new List<ActivityLogItem> {
                new ActivityLogItem { ActivityType = "Created", Description = string.Format("Pallet {0} created", pallet.PalletNumber), Timestamp = pallet.CreatedDate, Username = pallet.CreatedBy, BadgeClass = "badge-primary", IconClass = "fa fa-pallet" }
             };
            if (pallet.IsClosed && pallet.ClosedDate.HasValue)
            {
                activityLogs.Add(new ActivityLogItem { ActivityType = "Closed", Description = string.Format("Pallet {0} closed", pallet.PalletNumber), Timestamp = pallet.ClosedDate.Value, Username = pallet.CreatedBy, BadgeClass = "badge-success", IconClass = "fa fa-lock" });
            }
            foreach (var item in pallet.Items)
            {   //TODO
                //string itemCreatedBy = item.CreatedBy ?? Username; // Use CreatedBy from ItemListDto if available
                string itemCreatedBy = this.Username;
                activityLogs.Add(new ActivityLogItem { ActivityType = "Item Added", Description = string.Format("Item {0} added to pallet", item.ItemNumber), Timestamp = item.CreatedDate, Username = itemCreatedBy, BadgeClass = "badge-info", IconClass = "fa fa-box" });
            }
            return activityLogs.OrderByDescending(l => l.Timestamp).ToList();
        }


        // GET: Pallets/Create
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Create()
        {
            try
            {
                if (!await UserContext.CanEditPalletsAsync())
                { // Async check
                    TempData["ErrorMessage"] = "You do not have permission to create pallets.";
                    return RedirectToAction("Index");
                }

                var currentDivision = _sessionManager.GetCurrentDivision();
                var currentPlatform = _sessionManager.GetCurrentPlatform();

                var platformsTask = _platformValidationService.GetPlatformsForDivisionAsync(currentDivision);
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session

                // Await concurrently
                var platforms = await platformsTask;
                var displayName = await displayNameTask;
                var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch {/*log*/}

                var viewModel = new CreatePalletViewModel
                {
                    Division = currentDivision,
                    Platform = currentPlatform,
                    UnitOfMeasure = UnitOfMeasure.PC,

                    DivisionOptions = GetEnumSelectList<Division>(currentDivision),
                    PlatformOptions = GetPlatformSelectList(platforms, currentPlatform),
                    UnitOfMeasureOptions = GetEnumSelectList<UnitOfMeasure>(UnitOfMeasure.PC),

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = currentDivision,
                    CurrentPlatform = currentPlatform,
                    TouchModeEnabled = touchModeEnabled,
                    CanEdit = true // Already checked permission
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing pallet creation: {0}", ex));
                TempData["ErrorMessage"] = string.Format("Error preparing pallet creation: {0}", ex.Message);
                return RedirectToAction("Index");
            }
        }

        // POST: Pallets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Create(CreatePalletViewModel viewModel)
        {
            if (!await UserContext.CanEditPalletsAsync()) { return RedirectToAction("Index"); } // Re-check POST

            try
            {
                bool isValidPlatform = await _platformValidationService.IsValidPlatformForDivisionAsync(
                    viewModel.Platform, viewModel.Division);

                if (!isValidPlatform)
                {
                    ModelState.AddModelError("Platform", string.Format("Platform '{0}' is not valid for division '{1}'.", viewModel.Platform.GetDescription(), viewModel.Division.GetDescription()));
                }

                if (!ModelState.IsValid)
                {
                    await RepopulateCreatePalletDropdownsAsync(viewModel);
                    return View(viewModel);
                }

                var pallet = await _palletService.CreatePalletAsync(
                    viewModel.ManufacturingOrder, viewModel.Division,
                    viewModel.Platform, viewModel.UnitOfMeasure, Username);

                TempData["SuccessMessage"] = string.Format("Pallet {0} created successfully", pallet.PalletNumber);
                return RedirectToAction("Details", new { id = pallet.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error creating pallet: {0}", ex));
                ModelState.AddModelError("", string.Format("Error creating pallet: {0}", ex.Message));
                await RepopulateCreatePalletDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }

        // Async Helper method to repopulate dropdowns for create pallet
        private async Task RepopulateCreatePalletDropdownsAsync(CreatePalletViewModel viewModel)
        {
            var platformsTask = _platformValidationService.GetPlatformsForDivisionAsync(viewModel.Division);
            var displayNameTask = GetDisplayName();
            var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
            var canEditTask = UserContext.CanEditPalletsAsync(); // Still need this for the view model property

            // Await necessary data
            var platforms = await platformsTask;
            var displayName = await displayNameTask;
            var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch {/*log*/}
            var canEdit = await canEditTask;

            // Populate dropdowns
            viewModel.DivisionOptions = GetEnumSelectList<Division>(viewModel.Division);
            viewModel.PlatformOptions = GetPlatformSelectList(platforms, viewModel.Platform);
            viewModel.UnitOfMeasureOptions = GetEnumSelectList<UnitOfMeasure>(viewModel.UnitOfMeasure);

            // Populate base properties
            viewModel.Username = Username; viewModel.DisplayName = displayName;
            viewModel.CurrentDivision = _sessionManager.GetCurrentDivision(); // Sync get
            viewModel.CurrentPlatform = _sessionManager.GetCurrentPlatform(); // Sync get
            viewModel.TouchModeEnabled = touchModeEnabled; viewModel.CanEdit = canEdit;
        }


        // GET: Pallets/Close/5
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Close(int id)
        {
            try
            {
                var palletTask = _palletService.GetPalletByIdAsync(id);
                var canCloseTask = UserContext.CanClosePalletsAsync();

                var pallet = await palletTask;
                if (pallet == null) return HttpNotFound();
                if (pallet.IsClosed)
                {
                    TempData["WarningMessage"] = "Pallet is already closed.";
                    return RedirectToAction("Details", new { id });
                }

                if (!await canCloseTask)
                { // Await permission check
                    TempData["ErrorMessage"] = "You do not have permission to close pallets.";
                    return RedirectToAction("Details", new { id });
                }

                var preferencesTask = _userPreferenceService.GetAllPreferencesAsync(Username);
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
                var canEditTask = UserContext.CanEditPalletsAsync(); // Needed for view model

                // Await remaining UI data
                var preferences = await preferencesTask;
                var displayName = await displayNameTask;
                var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch {/*log*/}
                var canEdit = await canEditTask;

                var viewModel = new ClosePalletViewModel
                {
                    PalletId = pallet.Id,
                    PalletNumber = pallet.PalletNumber,
                    ManufacturingOrder = pallet.ManufacturingOrder,
                    ItemCount = pallet.ItemCount,
                    PrintPalletList = preferences?.AutoPrintPalletList ?? true,

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = _sessionManager.GetCurrentDivision(),
                    CurrentPlatform = _sessionManager.GetCurrentPlatform(),
                    TouchModeEnabled = touchModeEnabled,
                    CanEdit = canEdit
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing pallet close {0}: {1}", id, ex));
                TempData["ErrorMessage"] = string.Format("Error preparing pallet closure: {0}", ex.Message);
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Pallets/Close
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> Close(ClosePalletViewModel viewModel)
        {
            if (!await UserContext.CanClosePalletsAsync()) { return RedirectToAction("Index"); } // Re-check permission

            try
            {
                if (!viewModel.IsValid())
                {
                    ModelState.AddModelError("ConfirmationText", "Please type 'CLOSE' to confirm.");
                    await PopulateClosePalletViewModelAsync(viewModel); // Async helper
                    return View(viewModel);
                }

                var closedPallet = await _palletService.ClosePalletAsync(
                    viewModel.PalletId, viewModel.PrintPalletList);

                TempData["SuccessMessage"] = string.Format("Pallet {0} closed successfully.", closedPallet.PalletNumber);
                return RedirectToAction("Details", new { id = viewModel.PalletId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error closing pallet {0}: {1}", viewModel.PalletId, ex));
                ModelState.AddModelError("", string.Format("Error closing pallet: {0}", ex.Message));
                await PopulateClosePalletViewModelAsync(viewModel); // Async helper
                return View(viewModel);
            }
        }

        // Helper to populate ClosePalletViewModel base properties async
        private async Task PopulateClosePalletViewModelAsync(ClosePalletViewModel viewModel)
        {
            var displayNameTask = GetDisplayName();
            var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
            var canEditTask = UserContext.CanEditPalletsAsync(); // Or use CanClose result?

            // Await UI data
            var displayName = await displayNameTask;
            var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch { /* log */ }
            var canEdit = await canEditTask;

            viewModel.Username = Username; viewModel.DisplayName = displayName;
            viewModel.CurrentDivision = _sessionManager.GetCurrentDivision();
            viewModel.CurrentPlatform = _sessionManager.GetCurrentPlatform();
            viewModel.TouchModeEnabled = touchModeEnabled; viewModel.CanEdit = canEdit;
        }


        // GET: Pallets/Print/5
        public async Task<ActionResult> Print(int id)
        {
            try
            {
                var palletTask = _palletService.GetPalletDetailByIdAsync(id);
                var printersTask = _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);
                var defaultPrinterTask = _printerService.GetDefaultPalletListPrinterAsync(Username);
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
                var canEditTask = UserContext.CanEditPalletsAsync(); // Needed for view model

                // Await all concurrently
                await Task.WhenAll(palletTask, printersTask, defaultPrinterTask, displayNameTask, touchModeTask, canEditTask);

                var pallet = await palletTask;
                if (pallet == null) return HttpNotFound();

                var availablePrinters = await printersTask;
                var defaultPrinter = await defaultPrinterTask;
                var displayName = await displayNameTask;
                var touchModeEnabled = await touchModeTask; // Or _sessionManager.IsTouchModeEnabled();
                var canEdit = await canEditTask;


                var viewModel = new PrintPalletViewModel
                {
                    PalletId = pallet.Id,
                    Pallet = pallet,
                    PrinterName = defaultPrinter,
                    AvailablePrinters = availablePrinters
                        .Select(p => new SelectListItem { Text = p, Value = p, Selected = p == defaultPrinter })
                        .ToList(),

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = _sessionManager.GetCurrentDivision(),
                    CurrentPlatform = _sessionManager.GetCurrentPlatform(),
                    TouchModeEnabled = touchModeEnabled,
                    CanEdit = canEdit
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing print for pallet {0}: {1}", id, ex));
                TempData["ErrorMessage"] = string.Format("Error preparing print: {0}", ex.Message);
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
                if (string.IsNullOrEmpty(viewModel.PrinterName))
                {
                    ModelState.AddModelError("PrinterName", "Please select a printer.");
                }
                if (!ModelState.IsValid)
                {
                    await RepopulatePrintViewModelAsync(viewModel); // Async helper
                    return View(viewModel);
                }

                Task saveDefaultTask = Task.CompletedTask; // Placeholder task
                if (viewModel.SaveAsDefault)
                {
                    saveDefaultTask = _printerService.SetDefaultPalletListPrinterAsync(Username, viewModel.PrinterName);
                    // Don't await here yet, run concurrently with print if possible, handle potential save error later
                }

                bool success = await _printerService.PrintPalletListAsync(viewModel.PalletId);

                // Now check if saving the default printer failed (if attempted)
                try { await saveDefaultTask; }
                catch (Exception prefEx)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Warning: Failed to save default printer preference: {0}", prefEx.Message));
                    TempData["WarningMessage"] = "Could not save printer as default, but print job was sent.";
                    // Override success message if needed, or append warning
                }

                if (success) TempData["SuccessMessage"] = "Pallet list print job sent successfully.";
                else TempData["ErrorMessage"] = "Failed to send print job for pallet list."; // Printer service indicates failure


                return RedirectToAction("Details", new { id = viewModel.PalletId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error printing pallet list {0}: {1}", viewModel.PalletId, ex));
                ModelState.AddModelError("", string.Format("Error printing pallet list: {0}", ex.Message));
                await RepopulatePrintViewModelAsync(viewModel); // Async helper
                return View(viewModel);
            }
        }

        // Helper to repopulate PrintPalletViewModel async
        private async Task RepopulatePrintViewModelAsync(PrintPalletViewModel viewModel)
        {
            var palletTask = _palletService.GetPalletDetailByIdAsync(viewModel.PalletId);
            var printersTask = _printerService.GetAvailablePrintersAsync(PrinterType.PalletList);
            var displayNameTask = GetDisplayName();
            var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
            var canEditTask = UserContext.CanEditPalletsAsync();

            // Await UI Data
            var displayName = await displayNameTask;
            var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch { /* log */ }
            var canEdit = await canEditTask;

            viewModel.Pallet = await palletTask;
            if (viewModel.Pallet == null)
            {
                viewModel.AvailablePrinters = new List<SelectListItem>();
            }
            else
            {
                var availablePrinters = await printersTask;
                viewModel.AvailablePrinters = availablePrinters
                   .Select(p => new SelectListItem { Text = p, Value = p, Selected = p == viewModel.PrinterName })
                   .ToList();
            }

            viewModel.Username = Username; viewModel.DisplayName = displayName;
            viewModel.CurrentDivision = _sessionManager.GetCurrentDivision();
            viewModel.CurrentPlatform = _sessionManager.GetCurrentPlatform();
            viewModel.TouchModeEnabled = touchModeEnabled; viewModel.CanEdit = canEdit;
        }


        // GET: Pallets/MoveItem/5 (itemId)
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> MoveItem(int id /* Item Id */)
        {
            try
            {
                if (!await UserContext.CanMoveItemsAsync())
                { // Async check
                    TempData["ErrorMessage"] = "You do not have permission to move items.";
                    return RedirectToAction("Details", "Items", new { id = id });
                }

                var itemTask = _itemService.GetItemDetailByIdAsync(id);
                var division = _sessionManager.GetCurrentDivision();
                var platform = _sessionManager.GetCurrentPlatform();
                var palletsTask = _palletService.GetPagedPalletsAsync(1, 200, division, platform, false);
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
                var canEditTask = UserContext.CanEditPalletsAsync(); // Needed for view model

                // Await all
                await Task.WhenAll(itemTask, palletsTask, displayNameTask, touchModeTask, canEditTask);

                var item = await itemTask;
                if (item == null) return HttpNotFound();
                if (item.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot move item from a closed pallet.";
                    return RedirectToAction("Details", "Items", new { id });
                }

                var openPalletsResult = await palletsTask;
                var availablePallets = openPalletsResult.Items.Where(p => p.Id != item.PalletId).ToList();
                var displayName = await displayNameTask;
                var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch { }
                var canEdit = await canEditTask;

                var viewModel = new MovePalletItemViewModel
                {
                    ItemId = item.Id,
                    ItemNumber = item.ItemNumber,
                    Item = item,
                    SourcePalletId = item.PalletId,
                    SourcePalletNumber = item.Pallet?.PalletNumber,
                    AvailablePallets = availablePallets,
                    CreateNewPallet = false,
                    NewPalletManufacturingOrder = item.ManufacturingOrder,
                    NewPalletUnitOfMeasure = item.QuantityUnit,

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = division,
                    CurrentPlatform = platform,
                    TouchModeEnabled = touchModeEnabled,
                    CanEdit = canEdit
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing move for item {0}: {1}", id, ex));
                TempData["ErrorMessage"] = string.Format("Error preparing to move item: {0}", ex.Message);
                return RedirectToAction("Details", "Items", new { id });
            }
        }

        // POST: Pallets/MoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<ActionResult> MoveItem(MovePalletItemViewModel viewModel)
        {
            if (!await UserContext.CanMoveItemsAsync()) { /* Handle */ }

            int targetPalletId = viewModel.TargetPalletId;
            UnitOfMeasure unitOfMeasure = UnitOfMeasure.PC; // Default

            try
            {
                // Validate common required fields first
                if (viewModel.ItemId <= 0) ModelState.AddModelError("ItemId", "Invalid Item ID.");

                if (viewModel.CreateNewPallet)
                {
                    if (string.IsNullOrWhiteSpace(viewModel.NewPalletManufacturingOrder))
                    {
                        ModelState.AddModelError("NewPalletManufacturingOrder", "Manufacturing order is required for new pallet.");
                    }
                    if (string.IsNullOrWhiteSpace(viewModel.NewPalletUnitOfMeasure) ||
                        !Enum.TryParse<UnitOfMeasure>(viewModel.NewPalletUnitOfMeasure, out unitOfMeasure))
                    { // Parse here
                        ModelState.AddModelError("NewPalletUnitOfMeasure", "Valid unit of measure is required.");
                    }
                }
                else if (targetPalletId <= 0) // Only required if not creating new
                {
                    ModelState.AddModelError("TargetPalletId", "Please select a target pallet.");
                }

                if (!ModelState.IsValid)
                {
                    await RepopulateMovePalletItemViewModelAsync(viewModel);
                    return View(viewModel);
                }

                // --- Logic ----
                if (viewModel.CreateNewPallet)
                {
                    var division = _sessionManager.GetCurrentDivision();
                    var platform = _sessionManager.GetCurrentPlatform();
                    var newPallet = await _palletService.CreatePalletAsync(
                        viewModel.NewPalletManufacturingOrder, division, platform, unitOfMeasure, Username);

                    if (viewModel.MoveToNewPallet) targetPalletId = newPallet.Id;
                    else { /* Redirect after create */ TempData["SuccessMessage"] = string.Format("New pallet {0} created.", newPallet.PalletNumber); return RedirectToAction("Details", new { id = newPallet.Id }); }
                }

                // Now targetPalletId is set (either original or new one)
                bool canMove = await _itemService.CanMoveItemToPalletAsync(viewModel.ItemId, targetPalletId);
                if (!canMove)
                {
                    ModelState.AddModelError("", "Item cannot be moved to the selected target pallet.");
                    await RepopulateMovePalletItemViewModelAsync(viewModel);
                    return View(viewModel);
                }

                var movedItem = await _itemService.MoveItemToPalletAsync(viewModel.ItemId, targetPalletId);

                TempData["SuccessMessage"] = string.Format("Item {0} moved successfully to pallet {1}.", movedItem.ItemNumber, movedItem.Pallet.PalletNumber);
                return RedirectToAction("Details", new { id = targetPalletId }); // Redirect to target
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error moving item {0}: {1}", viewModel.ItemId, ex));
                ModelState.AddModelError("", string.Format("Error moving item: {0}", ex.Message));
                await RepopulateMovePalletItemViewModelAsync(viewModel);
                return View(viewModel);
            }
        }

        // Async Helper method to repopulate move item view model
        private async Task RepopulateMovePalletItemViewModelAsync(MovePalletItemViewModel viewModel)
        {
            if (viewModel.ItemId <= 0) return; // Cannot repopulate without item ID

            var itemTask = _itemService.GetItemDetailByIdAsync(viewModel.ItemId);
            var division = _sessionManager.GetCurrentDivision();
            var platform = _sessionManager.GetCurrentPlatform();
            var palletsTask = _palletService.GetPagedPalletsAsync(1, 200, division, platform, false);
            var displayNameTask = GetDisplayName();
            var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session
            var canEditTask = UserContext.CanEditPalletsAsync();

            // Await all
            await Task.WhenAll(itemTask, palletsTask, displayNameTask, touchModeTask, canEditTask);

            viewModel.Item = await itemTask;
            if (viewModel.Item != null)
            {
                viewModel.SourcePalletId = viewModel.Item.PalletId;
                viewModel.SourcePalletNumber = viewModel.Item.Pallet?.PalletNumber;
                if (string.IsNullOrEmpty(viewModel.NewPalletManufacturingOrder)) viewModel.NewPalletManufacturingOrder = viewModel.Item.ManufacturingOrder;
                if (string.IsNullOrEmpty(viewModel.NewPalletUnitOfMeasure)) viewModel.NewPalletUnitOfMeasure = viewModel.Item.QuantityUnit;
            }

            var openPalletsResult = await palletsTask;
            viewModel.AvailablePallets = openPalletsResult.Items.Where(p => p.Id != viewModel.SourcePalletId).ToList();

            viewModel.Username = Username;
            viewModel.DisplayName = await displayNameTask;
            viewModel.CurrentDivision = division; viewModel.CurrentPlatform = platform;
            viewModel.TouchModeEnabled = false; try { viewModel.TouchModeEnabled = await touchModeTask; } catch { }
            viewModel.CanEdit = await canEditTask;
        }




        // POST: Pallets/Filter
        [HttpPost]
        public async Task<ActionResult> Filter(PalletFilterViewModel filter)
        {
            try
            {
                int pageSize = 20;
                try { pageSize = await _userPreferenceService.GetItemsPerPageAsync(Username); } catch { /* Log */ }
                if (pageSize <= 0) pageSize = 20;

                return RedirectToAction("Index", new
                {
                    keyword = filter.Keyword,
                    isClosed = filter.IsClosed,
                    page = 1,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error applying filter: {0}", ex));
                TempData["ErrorMessage"] = string.Format("Error applying filter: {0}", ex.Message);
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
                    return Json(new { success = false, message = "Search term must be at least 2 characters." });
                }

                var division = _sessionManager.GetCurrentDivision(); // Sync Get
                var platform = _sessionManager.GetCurrentPlatform(); // Sync Get

                var palletDtos = await _palletService.SearchPalletsAsync(term); // Async call

                var results = palletDtos
                    .Where(p => p.Division == division.ToString() && p.Platform == platform.ToString())
                    .Select(p => new {
                        id = p.Id,
                        text = p.PalletNumber,
                        info = string.Format("MO: {0}, Items: {1}, Status: {2}", p.ManufacturingOrder, p.ItemCount, (p.IsClosed ? "Closed" : "Open"))
                    })
                    .ToList();

                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error during pallet search for '{0}': {1}", term, ex));
                return Json(new { success = false, message = string.Format("An error occurred during search: {0}", ex.Message) });
            }
        }

        // --- Dropdown Helper Methods (Synchronous is fine) ---
        private List<SelectListItem> GetEnumSelectList<TEnum>(TEnum selectedValue) where TEnum : struct, Enum
        {
            return Enum.GetValues(typeof(TEnum))
               .Cast<TEnum>()
               .Select(e => new SelectListItem
               {
                   Text = e.GetDescription(), // Assumes EnumExtensions.GetDescription exists
                   Value = e.ToString(),
                   Selected = e.Equals(selectedValue)
               }).ToList();
        }

        private List<SelectListItem> GetPlatformSelectList(IEnumerable<Platform> platforms, Platform selectedValue)
        {
            return platforms
                .Select(p => new SelectListItem
                {
                    Text = p.GetDescription(),
                    Value = p.ToString(),
                    Selected = p == selectedValue
                }).ToList();
        }
    }
}