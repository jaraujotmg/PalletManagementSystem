// src/PalletManagementSystem.Web2/Controllers/ItemsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc; // Ensure using System.Web.Mvc
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Extensions; // For EnumExtensions if used
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web2.Services; // For ISessionManager if needed, but seems not used here
using PalletManagementSystem.Web2.ViewModels.Items;
using PalletManagementSystem.Web2.ViewModels.Pallets;
// Remove unused using for Pallet view models if not needed


namespace PalletManagementSystem.Web2.Controllers
{
    //[Authorize] 
    public class ItemsController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IPalletService _palletService;
        private readonly IPrinterService _printerService;
        private readonly IUserPreferenceService _userPreferenceService;
        // No ISessionManager needed here based on current code

        public ItemsController(
            IUserContext userContext,
            IItemService itemService,
            IPalletService palletService,
            IPrinterService printerService,
            IUserPreferenceService userPreferenceService)
            : base(userContext)
        {
            _itemService = itemService ?? throw new ArgumentNullException("itemService");
            _palletService = palletService ?? throw new ArgumentNullException("palletService");
            _printerService = printerService ?? throw new ArgumentNullException("printerService");
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException("userPreferenceService");
        }

        // GET: Items/Details/5
        public async Task<ActionResult> Details(int id, string returnUrl = null)
        {
            try
            {
                var itemTask = _itemService.GetItemDetailByIdAsync(id);
                // Fetch permissions and UI data concurrently
                var canEditTask = UserContext.CanEditItemsAsync();
                var canMoveTask = UserContext.CanMoveItemsAsync();
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username);

                var item = await itemTask;
                if (item == null) return HttpNotFound();

                // Determine effective permissions based on pallet status
                bool palletIsClosed = item.Pallet?.IsClosed ?? false;
                bool effectiveCanEdit = (!palletIsClosed) && (await canEditTask);
                bool effectiveCanMove = (!palletIsClosed) && (await canMoveTask);

                // Await remaining UI data
                string displayName = await displayNameTask;
                bool touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch {/* log */}

                var viewModel = new ItemDetailViewModel
                {
                    Item = item,
                    CanEdit = effectiveCanEdit,
                    CanMove = effectiveCanMove,
                    CanPrint = true, // Assuming always true
                    ReturnUrl = returnUrl ?? Url.Action("Details", "Pallets", new { id = item.PalletId }),

                    // Common ViewModel properties
                    Username = Username, // Sync
                    DisplayName = displayName,
                    CurrentDivision = UserContext.GetDivision(), // Sync
                    CurrentPlatform = UserContext.GetPlatform(), // Sync
                    TouchModeEnabled = touchModeEnabled
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error retrieving item details {0}: {1}", id, ex));
                ModelState.AddModelError("", string.Format("Error retrieving item details: {0}", ex.Message));
                // Redirect to a safe place, maybe pallet list or home
                return RedirectToAction("Index", "Pallets");
            }
        }

        // GET: Items/Edit/5
        public async Task<ActionResult> Edit(int id, string returnUrl = null)
        {
            try
            {
                // Check permission first
                if (!await UserContext.CanEditItemsAsync()) // Async check
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit items.";
                    return RedirectToAction("Details", new { id = id });
                }

                var itemTask = _itemService.GetItemDetailByIdAsync(id);
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username);

                var item = await itemTask;
                if (item == null) return HttpNotFound();

                // Check if item is on a closed pallet (can happen between permission check and fetch)
                if (item.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot edit items on a closed pallet.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Await UI data
                var displayName = await displayNameTask;
                var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch { }

                var viewModel = new ItemEditViewModel();
                viewModel.PopulateFromDto(item); // Assumes this method exists and is synchronous
                viewModel.ReturnUrl = returnUrl ?? Url.Action("Details", new { id = id });

                // Common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = displayName;
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = touchModeEnabled;
                viewModel.EnableTouchMode = touchModeEnabled; // Assuming this mirrors TouchModeEnabled

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing item {0} for edit: {1}", id, ex));
                ModelState.AddModelError("", string.Format("Error preparing item for edit: {0}", ex.Message));
                // Redirect to a safe place
                return RedirectToAction("Index", "Pallets");
            }
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemEditViewModel viewModel)
        {
            // Re-check permission on POST
            if (!await UserContext.CanEditItemsAsync())
            {
                TempData["ErrorMessage"] = "You don't have permission to edit items.";
                return RedirectToAction("Details", new { id = viewModel.ItemId });
            }

            try
            {
                // Check if the pallet is closed (re-check for safety)
                var itemTask = _itemService.GetItemDetailByIdAsync(viewModel.ItemId);
                // No need to fetch entire item just for pallet status if service can check?
                // If not, fetch it:
                var item = await itemTask;
                if (item?.Pallet?.IsClosed ?? false) // Check if item exists and pallet is closed
                {
                    TempData["ErrorMessage"] = "Cannot edit items on a closed pallet.";
                    return RedirectToAction("Details", new { id = viewModel.ItemId });
                }

                if (!ModelState.IsValid)
                {
                    // Repopulate common properties if returning view
                    var displayName = await GetDisplayName();
                    var touchModeEnabled = false; try { touchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username); } catch { }
                    viewModel.Username = Username;
                    viewModel.DisplayName = displayName;
                    viewModel.CurrentDivision = UserContext.GetDivision();
                    viewModel.CurrentPlatform = UserContext.GetPlatform();
                    viewModel.TouchModeEnabled = touchModeEnabled;
                    viewModel.EnableTouchMode = touchModeEnabled; // Assuming this mirrors TouchModeEnabled
                    return View(viewModel);
                }

                var updateDto = new UpdateItemDto
                {
                    Weight = viewModel.Weight,
                    Width = viewModel.Width,
                    Quality = viewModel.Quality,
                    Batch = viewModel.Batch
                };

                // Update the item (service call is async)
                await _itemService.UpdateItemAsync(viewModel.ItemId, updateDto);

                TempData["SuccessMessage"] = "Item updated successfully.";
                // Use IsLocalUrl for safety if ReturnUrl comes from query string
                if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
                {
                    return Redirect(viewModel.ReturnUrl);
                }
                return RedirectToAction("Details", new { id = viewModel.ItemId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error updating item {0}: {1}", viewModel.ItemId, ex));
                ModelState.AddModelError("", string.Format("Error updating item: {0}", ex.Message));

                // Repopulate common properties if returning view
                var displayName = await GetDisplayName();
                var touchModeEnabled = false; try { touchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username); } catch { }
                viewModel.Username = Username;
                viewModel.DisplayName = displayName;
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = touchModeEnabled;
                viewModel.EnableTouchMode = touchModeEnabled;

                return View(viewModel);
            }
        }

        // GET: Items/Create
        // No specific item permission needed here, just permission to edit the PARENT pallet
        public async Task<ActionResult> Create(int palletId, string returnUrl = null)
        {
            try
            {
                // Fetch pallet and permissions/UI data concurrently
                var palletTask = _palletService.GetPalletByIdAsync(palletId); // Get PalletListDto is enough
                var canEditTask = UserContext.CanEditItemsAsync(); // Check if user can edit/create items in general
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username);

                var pallet = await palletTask;
                if (pallet == null) return HttpNotFound();

                // Await permission and UI data
                bool canEditItems = await canEditTask;
                string displayName = await displayNameTask;
                bool touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch { }

                if (!canEditItems)
                {
                    TempData["ErrorMessage"] = "You don't have permission to create items.";
                    return RedirectToAction("Details", "Pallets", new { id = palletId });
                }

                if (pallet.IsClosed)
                {
                    TempData["ErrorMessage"] = "Cannot add items to a closed pallet.";
                    return RedirectToAction("Details", "Pallets", new { id = palletId });
                }

                var viewModel = new CreateItemViewModel
                {
                    PalletId = palletId,
                    PalletNumber = pallet.PalletNumber,
                    ManufacturingOrder = pallet.ManufacturingOrder,
                    QuantityUnit = pallet.UnitOfMeasure, // Use pallet's unit
                    WeightUnit = "KG",
                    WidthUnit = "CM",
                    Quality = "Standard", // Defaults
                    ReturnUrl = returnUrl ?? Url.Action("Details", "Pallets", new { id = palletId }),

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = touchModeEnabled,
                    EnableTouchMode = touchModeEnabled
                };

                // Populate dropdown options (synchronous)
                PopulateCreateItemViewModelDropdowns(viewModel);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing item creation for pallet {0}: {1}", palletId, ex));
                ModelState.AddModelError("", string.Format("Error preparing item creation: {0}", ex.Message));
                return RedirectToAction("Details", "Pallets", new { id = palletId });
            }
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateItemViewModel viewModel)
        {
            // Re-check permission on POST
            if (!await UserContext.CanEditItemsAsync())
            {
                TempData["ErrorMessage"] = "You don't have permission to create items.";
                return RedirectToAction("Details", "Pallets", new { id = viewModel.PalletId });
            }

            try
            {
                // Re-check pallet status on POST
                var pallet = await _palletService.GetPalletByIdAsync(viewModel.PalletId);
                if (pallet == null) return HttpNotFound();
                if (pallet.IsClosed)
                {
                    TempData["ErrorMessage"] = "Cannot add items to a closed pallet.";
                    return RedirectToAction("Details", "Pallets", new { id = viewModel.PalletId });
                }

                if (!ModelState.IsValid)
                {
                    await RepopulateCreateItemViewModelAsync(viewModel); // Use async helper
                    return View(viewModel);
                }

                var itemDto = new ItemDto
                { /* ... map properties from viewModel ... */
                    ManufacturingOrder = viewModel.ManufacturingOrder,
                    ManufacturingOrderLine = viewModel.ManufacturingOrderLine,
                    ServiceOrder = viewModel.ServiceOrder,
                    ServiceOrderLine = viewModel.ServiceOrderLine,
                    FinalOrder = viewModel.FinalOrder,
                    FinalOrderLine = viewModel.FinalOrderLine,
                    ClientCode = viewModel.ClientCode,
                    ClientName = viewModel.ClientName,
                    Reference = viewModel.Reference,
                    Finish = viewModel.Finish,
                    Color = viewModel.Color,
                    Quantity = viewModel.Quantity,
                    QuantityUnit = viewModel.QuantityUnit,
                    Weight = viewModel.Weight,
                    WeightUnit = viewModel.WeightUnit,
                    Width = viewModel.Width,
                    WidthUnit = viewModel.WidthUnit,
                    Quality = viewModel.Quality,
                    Batch = viewModel.Batch
                };

                // Create the item (service call is async)
                await _itemService.CreateItemAsync(itemDto, viewModel.PalletId, Username);

                TempData["SuccessMessage"] = "Item created successfully.";
                if (!string.IsNullOrEmpty(viewModel.ReturnUrl) && Url.IsLocalUrl(viewModel.ReturnUrl))
                {
                    return Redirect(viewModel.ReturnUrl);
                }
                return RedirectToAction("Details", "Pallets", new { id = viewModel.PalletId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error creating item for pallet {0}: {1}", viewModel.PalletId, ex));
                ModelState.AddModelError("", string.Format("Error creating item: {0}", ex.Message));
                await RepopulateCreateItemViewModelAsync(viewModel); // Use async helper
                return View(viewModel);
            }
        }

        // Helper to populate dropdowns synchronously
        private void PopulateCreateItemViewModelDropdowns(CreateItemViewModel viewModel)
        {
            viewModel.QuantityUnitOptions = GetUnitOfMeasureOptions(viewModel.QuantityUnit);
            viewModel.WeightUnitOptions = new List<SelectListItem> {
                 new SelectListItem { Text = "Kilogram (KG)", Value = "KG", Selected = viewModel.WeightUnit == "KG" },
                 new SelectListItem { Text = "Pound (LB)", Value = "LB", Selected = viewModel.WeightUnit == "LB" }
             };
            viewModel.WidthUnitOptions = new List<SelectListItem> {
                 new SelectListItem { Text = "Centimeter (CM)", Value = "CM", Selected = viewModel.WidthUnit == "CM" },
                 new SelectListItem { Text = "Inch (IN)", Value = "IN", Selected = viewModel.WidthUnit == "IN" }
             };
            viewModel.QualityOptions = new List<SelectListItem> {
                 new SelectListItem { Text = "Premium", Value = "Premium", Selected = viewModel.Quality == "Premium" },
                 new SelectListItem { Text = "Standard", Value = "Standard", Selected = viewModel.Quality == "Standard" },
                 new SelectListItem { Text = "Economy", Value = "Economy", Selected = viewModel.Quality == "Economy" },
                 new SelectListItem { Text = "Special", Value = "Special", Selected = viewModel.Quality == "Special" }
             };
        }

        // Async Helper method to repopulate CreateItemViewModel fully
        private async Task RepopulateCreateItemViewModelAsync(CreateItemViewModel viewModel)
        {
            // Populate dropdowns (sync)
            PopulateCreateItemViewModelDropdowns(viewModel);

            // Populate base properties (async)
            var displayNameTask = GetDisplayName();
            var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session

            viewModel.Username = Username;
            viewModel.DisplayName = await displayNameTask;
            viewModel.CurrentDivision = UserContext.GetDivision(); // Sync get
            viewModel.CurrentPlatform = UserContext.GetPlatform(); // Sync get
            viewModel.TouchModeEnabled = false; try { viewModel.TouchModeEnabled = await touchModeTask; } catch { /* log */ }
            viewModel.EnableTouchMode = viewModel.TouchModeEnabled;
        }


        // GET: Items/Move/5
        public async Task<ActionResult> Move(int id, string returnUrl = null)
        {
            try
            {
                // Check permission first
                if (!await UserContext.CanMoveItemsAsync()) // Async check
                {
                    TempData["ErrorMessage"] = "You don't have permission to move items.";
                    return RedirectToAction("Details", new { id = id });
                }

                var itemTask = _itemService.GetItemDetailByIdAsync(id);

                // Fetch necessary data concurrently
                var division = UserContext.GetDivision(); // Sync get
                var platform = UserContext.GetPlatform(); // Sync get
                // Fetch pallets async - Use GetPalletsByDivisionAndPlatformAsync for efficiency if possible
                var palletsTask = _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
                var displayNameTask = GetDisplayName();
                var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session

                var item = await itemTask;
                if (item == null) return HttpNotFound();

                if (item.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot move items from a closed pallet.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Await remaining data
                var allPallets = await palletsTask; // This returns PalletListDto
                var openPallets = allPallets.Where(p => !p.IsClosed && p.Id != item.PalletId).ToList();
                var displayName = await displayNameTask;
                var touchModeEnabled = false; try { touchModeEnabled = await touchModeTask; } catch { }

                var viewModel = new MovePalletItemViewModel
                {
                    ItemId = item.Id,
                    ItemNumber = item.ItemNumber,
                    Item = item,
                    SourcePalletId = item.PalletId,
                    SourcePalletNumber = item.Pallet?.PalletNumber,
                    AvailablePallets = openPallets, // Assign the filtered list

                    Username = Username,
                    DisplayName = displayName,
                    CurrentDivision = division,
                    CurrentPlatform = platform,
                    TouchModeEnabled = touchModeEnabled
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error preparing item move for item {0}: {1}", id, ex));
                ModelState.AddModelError("", string.Format("Error preparing item move: {0}", ex.Message));
                // Redirect to item details on error during GET
                return RedirectToAction("Details", new { id = id });
            }
        }

        // POST: Items/Move
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Move(MovePalletItemViewModel viewModel)
        {
            // Re-check permission on POST
            if (!await UserContext.CanMoveItemsAsync())
            {
                TempData["ErrorMessage"] = "You don't have permission to move items.";
                return RedirectToAction("Details", new { id = viewModel.ItemId });
            }

            try
            {
                // Basic validation
                if (viewModel.TargetPalletId <= 0) ModelState.AddModelError("TargetPalletId", "Please select a target pallet.");
                if (viewModel.ItemId <= 0) ModelState.AddModelError("ItemId", "Invalid Item ID."); // Add validation

                if (!ModelState.IsValid)
                {
                    await RepopulateMoveItemViewModelAsync(viewModel); // Use async helper
                    return View(viewModel);
                }

                // Check if the item can be moved (service call is async)
                bool canMove = await _itemService.CanMoveItemToPalletAsync(viewModel.ItemId, viewModel.TargetPalletId);
                if (!canMove)
                {
                    TempData["ErrorMessage"] = "Item cannot be moved to the selected target pallet (it might be closed or the source might be closed).";
                    await RepopulateMoveItemViewModelAsync(viewModel); // Use async helper
                    return View(viewModel);
                }

                // Move the item (service call is async)
                await _itemService.MoveItemToPalletAsync(viewModel.ItemId, viewModel.TargetPalletId);

                TempData["SuccessMessage"] = "Item moved successfully.";
                // Redirect to the item details page after moving
                return RedirectToAction("Details", new { id = viewModel.ItemId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error moving item {0}: {1}", viewModel.ItemId, ex));
                ModelState.AddModelError("", string.Format("Error moving item: {0}", ex.Message));
                await RepopulateMoveItemViewModelAsync(viewModel); // Use async helper
                return View(viewModel);
            }
        }

        // Async Helper method to repopulate MoveItemViewModel
        private async Task RepopulateMoveItemViewModelAsync(MovePalletItemViewModel viewModel)
        {
            if (viewModel.ItemId <= 0) return;

            var itemTask = _itemService.GetItemDetailByIdAsync(viewModel.ItemId);
            var division = UserContext.GetDivision(); // Sync Get
            var platform = UserContext.GetPlatform(); // Sync Get
            var palletsTask = _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
            var displayNameTask = GetDisplayName();
            var touchModeTask = _userPreferenceService.GetTouchModeEnabledAsync(Username); // Or sync session

            // Await all
            await Task.WhenAll(itemTask, palletsTask, displayNameTask, touchModeTask);

            viewModel.Item = await itemTask;
            if (viewModel.Item != null)
            {
                viewModel.SourcePalletId = viewModel.Item.PalletId;
                viewModel.SourcePalletNumber = viewModel.Item.Pallet?.PalletNumber;
            }

            var allPallets = await palletsTask;
            viewModel.AvailablePallets = allPallets.Where(p => !p.IsClosed && p.Id != viewModel.SourcePalletId).ToList();

            viewModel.Username = Username;
            viewModel.DisplayName = await displayNameTask;
            viewModel.CurrentDivision = division; viewModel.CurrentPlatform = platform;
            viewModel.TouchModeEnabled = false; try { viewModel.TouchModeEnabled = await touchModeTask; } catch { }
        }


        // POST: Items/Print/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Print(int id) // Assuming anyone can print
        {
            try
            {
                var itemTask = _itemService.GetItemByIdAsync(id); // Get list DTO is enough to check existence
                var item = await itemTask;
                if (item == null) return HttpNotFound();

                // Print the item label (service call is async)
                bool success = await _printerService.PrintItemLabelAsync(id);

                if (success) TempData["SuccessMessage"] = "Item label print job sent successfully.";
                else TempData["WarningMessage"] = "Item label print request sent, but status could not be confirmed.";

                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Error printing item label {0}: {1}", id, ex));
                TempData["ErrorMessage"] = string.Format("Error printing item label: {0}", ex.Message);
                return RedirectToAction("Details", new { id = id });
            }
        }

        // Helper method for UnitOfMeasure options (Synchronous)
        private List<SelectListItem> GetUnitOfMeasureOptions(string selectedUnit)
        {
            var units = Enum.GetValues(typeof(UnitOfMeasure)).Cast<UnitOfMeasure>();
            return units.Select(u => new SelectListItem
            {
                Text = string.Format("{0} ({1})", u.GetDescription(), u), // Use string.Format
                Value = u.ToString(),
                Selected = u.ToString() == selectedUnit
            }).ToList();
        }
    }
}