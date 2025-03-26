// src/PalletManagementSystem.Web/Controllers/ItemsController.cs
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
using PalletManagementSystem.Web.ViewModels.Items;
using PalletManagementSystem.Web.ViewModels.Pallets;

namespace PalletManagementSystem.Web.Controllers
{
    [Authorize]
    public class ItemsController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IPalletService _palletService;
        private readonly IPrinterService _printerService;
        private readonly IUserPreferenceService _userPreferenceService;

        public ItemsController(
            IUserContext userContext,
            IItemService itemService,
            IPalletService palletService,
            IPrinterService printerService,
            IUserPreferenceService userPreferenceService)
            : base(userContext)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
        }

        // GET: Items/Details/5
        public async Task<ActionResult> Details(int id, string returnUrl = null)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                var viewModel = new ItemDetailViewModel
                {
                    Item = item,
                    CanEdit = UserContext.CanEditItems() && !(item.Pallet?.IsClosed ?? false),
                    CanMove = UserContext.CanMoveItems() && !(item.Pallet?.IsClosed ?? false),
                    CanPrint = true,
                    ReturnUrl = returnUrl ?? Url.Action("Details", "Pallets", new { id = item.PalletId }),

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
                // Log the exception
                ModelState.AddModelError("", $"Error retrieving item details: {ex.Message}");
                return RedirectToAction("Index", "Pallets");
            }
        }

        // GET: Items/Edit/5
        public async Task<ActionResult> Edit(int id, string returnUrl = null)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                // Check if user has edit permission
                if (!UserContext.CanEditItems())
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit items.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Check if item is on a closed pallet
                if (item.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot edit items on a closed pallet.";
                    return RedirectToAction("Details", new { id = id });
                }

                var viewModel = new ItemEditViewModel();
                viewModel.PopulateFromDto(item);
                viewModel.ReturnUrl = returnUrl ?? Url.Action("Details", new { id = id });

                // Common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);
                viewModel.EnableTouchMode = viewModel.TouchModeEnabled;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", $"Error preparing item for edit: {ex.Message}");
                return RedirectToAction("Index", "Pallets");
            }
        }

        // POST: Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemEditViewModel viewModel)
        {
            try
            {
                // Check if user has edit permission
                if (!UserContext.CanEditItems())
                {
                    TempData["ErrorMessage"] = "You don't have permission to edit items.";
                    return RedirectToAction("Details", new { id = viewModel.ItemId });
                }

                // Check if the pallet is closed
                var item = await _itemService.GetItemDetailByIdAsync(viewModel.ItemId);
                if (item?.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot edit items on a closed pallet.";
                    return RedirectToAction("Details", new { id = viewModel.ItemId });
                }

                if (!ModelState.IsValid)
                {
                    // Common ViewModel properties
                    viewModel.Username = Username;
                    viewModel.DisplayName = await GetDisplayName();
                    viewModel.CurrentDivision = UserContext.GetDivision();
                    viewModel.CurrentPlatform = UserContext.GetPlatform();
                    viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                    return View(viewModel);
                }

                // Create update DTO
                var updateDto = new UpdateItemDto
                {
                    Weight = viewModel.Weight,
                    Width = viewModel.Width,
                    Quality = viewModel.Quality,
                    Batch = viewModel.Batch
                };

                // Update the item
                await _itemService.UpdateItemAsync(viewModel.ItemId, updateDto);

                TempData["SuccessMessage"] = "Item updated successfully.";
                return Redirect(viewModel.ReturnUrl ?? Url.Action("Details", new { id = viewModel.ItemId }));
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", $"Error updating item: {ex.Message}");

                // Common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                return View(viewModel);
            }
        }

        // GET: Items/Create
        public async Task<ActionResult> Create(int palletId, string returnUrl = null)
        {
            try
            {
                // Get pallet to check if it's closed and get manufacturing order
                var pallet = await _palletService.GetPalletByIdAsync(palletId);
                if (pallet == null)
                {
                    return HttpNotFound();
                }

                // Check if user has edit permission
                if (!UserContext.CanEditItems())
                {
                    TempData["ErrorMessage"] = "You don't have permission to create items.";
                    return RedirectToAction("Details", "Pallets", new { id = palletId });
                }

                // Check if pallet is closed
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
                    QuantityUnit = pallet.UnitOfMeasure,
                    WeightUnit = "KG",
                    WidthUnit = "CM",
                    Quality = "Standard",
                    ReturnUrl = returnUrl ?? Url.Action("Details", "Pallets", new { id = palletId }),

                    // Common ViewModel properties
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    EnableTouchMode = await _userPreferenceService.GetTouchModeEnabledAsync(Username)
                };

                // Populate dropdown options
                viewModel.QuantityUnitOptions = GetUnitOfMeasureOptions(viewModel.QuantityUnit);
                viewModel.WeightUnitOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Kilogram (KG)", Value = "KG", Selected = viewModel.WeightUnit == "KG" },
                    new SelectListItem { Text = "Pound (LB)", Value = "LB", Selected = viewModel.WeightUnit == "LB" }
                };
                viewModel.WidthUnitOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Centimeter (CM)", Value = "CM", Selected = viewModel.WidthUnit == "CM" },
                    new SelectListItem { Text = "Inch (IN)", Value = "IN", Selected = viewModel.WidthUnit == "IN" }
                };
                viewModel.QualityOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Premium", Value = "Premium", Selected = viewModel.Quality == "Premium" },
                    new SelectListItem { Text = "Standard", Value = "Standard", Selected = viewModel.Quality == "Standard" },
                    new SelectListItem { Text = "Economy", Value = "Economy", Selected = viewModel.Quality == "Economy" },
                    new SelectListItem { Text = "Special", Value = "Special", Selected = viewModel.Quality == "Special" }
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", $"Error preparing item creation: {ex.Message}");
                return RedirectToAction("Details", "Pallets", new { id = palletId });
            }
        }

        // POST: Items/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateItemViewModel viewModel)
        {
            try
            {
                // Check if user has edit permission
                if (!UserContext.CanEditItems())
                {
                    TempData["ErrorMessage"] = "You don't have permission to create items.";
                    return RedirectToAction("Details", "Pallets", new { id = viewModel.PalletId });
                }

                // Check if pallet is closed
                var pallet = await _palletService.GetPalletByIdAsync(viewModel.PalletId);
                if (pallet == null)
                {
                    return HttpNotFound();
                }

                if (pallet.IsClosed)
                {
                    TempData["ErrorMessage"] = "Cannot add items to a closed pallet.";
                    return RedirectToAction("Details", "Pallets", new { id = viewModel.PalletId });
                }

                if (!ModelState.IsValid)
                {
                    // Re-populate dropdown options
                    viewModel.QuantityUnitOptions = GetUnitOfMeasureOptions(viewModel.QuantityUnit);
                    viewModel.WeightUnitOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "Kilogram (KG)", Value = "KG", Selected = viewModel.WeightUnit == "KG" },
                        new SelectListItem { Text = "Pound (LB)", Value = "LB", Selected = viewModel.WeightUnit == "LB" }
                    };
                    viewModel.WidthUnitOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "Centimeter (CM)", Value = "CM", Selected = viewModel.WidthUnit == "CM" },
                        new SelectListItem { Text = "Inch (IN)", Value = "IN", Selected = viewModel.WidthUnit == "IN" }
                    };
                    viewModel.QualityOptions = new List<SelectListItem>
                    {
                        new SelectListItem { Text = "Premium", Value = "Premium", Selected = viewModel.Quality == "Premium" },
                        new SelectListItem { Text = "Standard", Value = "Standard", Selected = viewModel.Quality == "Standard" },
                        new SelectListItem { Text = "Economy", Value = "Economy", Selected = viewModel.Quality == "Economy" },
                        new SelectListItem { Text = "Special", Value = "Special", Selected = viewModel.Quality == "Special" }
                    };

                    // Common ViewModel properties
                    viewModel.Username = Username;
                    viewModel.DisplayName = await GetDisplayName();
                    viewModel.CurrentDivision = UserContext.GetDivision();
                    viewModel.CurrentPlatform = UserContext.GetPlatform();
                    viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                    return View(viewModel);
                }

                // Create item DTO
                var itemDto = new ItemDto
                {
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

                // Create the item
                await _itemService.CreateItemAsync(itemDto, viewModel.PalletId, Username);

                TempData["SuccessMessage"] = "Item created successfully.";
                return Redirect(viewModel.ReturnUrl ?? Url.Action("Details", "Pallets", new { id = viewModel.PalletId }));
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", $"Error creating item: {ex.Message}");

                // Re-populate dropdown options
                viewModel.QuantityUnitOptions = GetUnitOfMeasureOptions(viewModel.QuantityUnit);
                viewModel.WeightUnitOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Kilogram (KG)", Value = "KG", Selected = viewModel.WeightUnit == "KG" },
                    new SelectListItem { Text = "Pound (LB)", Value = "LB", Selected = viewModel.WeightUnit == "LB" }
                };
                viewModel.WidthUnitOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Centimeter (CM)", Value = "CM", Selected = viewModel.WidthUnit == "CM" },
                    new SelectListItem { Text = "Inch (IN)", Value = "IN", Selected = viewModel.WidthUnit == "IN" }
                };
                viewModel.QualityOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Premium", Value = "Premium", Selected = viewModel.Quality == "Premium" },
                    new SelectListItem { Text = "Standard", Value = "Standard", Selected = viewModel.Quality == "Standard" },
                    new SelectListItem { Text = "Economy", Value = "Economy", Selected = viewModel.Quality == "Economy" },
                    new SelectListItem { Text = "Special", Value = "Special", Selected = viewModel.Quality == "Special" }
                };

                // Common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                return View(viewModel);
            }
        }

        // GET: Items/Move/5
        public async Task<ActionResult> Move(int id, string returnUrl = null)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                // Check if user has move permission
                if (!UserContext.CanMoveItems())
                {
                    TempData["ErrorMessage"] = "You don't have permission to move items.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Check if the pallet is closed
                if (item.Pallet?.IsClosed ?? false)
                {
                    TempData["ErrorMessage"] = "Cannot move items from a closed pallet.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Get available pallets
                var division = UserContext.GetDivision();
                var platform = UserContext.GetPlatform();
                var pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
                var openPallets = pallets.Where(p => !p.IsClosed && p.Id != item.PalletId).ToList();

                var viewModel = new MovePalletItemViewModel
                {
                    ItemId = item.Id,
                    ItemNumber = item.ItemNumber,
                    Item = item,
                    SourcePalletId = item.PalletId,
                    SourcePalletNumber = item.Pallet?.PalletNumber,
                    AvailablePallets = openPallets.ToList(),

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
                // Log the exception
                ModelState.AddModelError("", $"Error preparing item move: {ex.Message}");
                return RedirectToAction("Details", new { id = id });
            }
        }

        // POST: Items/Move
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Move(MovePalletItemViewModel viewModel)
        {
            try
            {
                // Check if user has move permission
                if (!UserContext.CanMoveItems())
                {
                    TempData["ErrorMessage"] = "You don't have permission to move items.";
                    return RedirectToAction("Details", new { id = viewModel.ItemId });
                }

                // Check if the item can be moved to the target pallet
                bool canMove = await _itemService.CanMoveItemToPalletAsync(viewModel.ItemId, viewModel.TargetPalletId);
                if (!canMove)
                {
                    TempData["ErrorMessage"] = "Item cannot be moved to the target pallet.";

                    // Get available pallets for redisplay
                    var division = UserContext.GetDivision();
                    var platform = UserContext.GetPlatform();
                    var pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
                    viewModel.AvailablePallets = pallets.Where(p => !p.IsClosed && p.Id != viewModel.SourcePalletId).ToList();

                    // Common ViewModel properties
                    viewModel.Username = Username;
                    viewModel.DisplayName = await GetDisplayName();
                    viewModel.CurrentDivision = UserContext.GetDivision();
                    viewModel.CurrentPlatform = UserContext.GetPlatform();
                    viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                    return View(viewModel);
                }

                // Move the item
                await _itemService.MoveItemToPalletAsync(viewModel.ItemId, viewModel.TargetPalletId);

                TempData["SuccessMessage"] = "Item moved successfully.";
                return RedirectToAction("Details", new { id = viewModel.ItemId });
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", $"Error moving item: {ex.Message}");

                // Get available pallets for redisplay
                var division = UserContext.GetDivision();
                var platform = UserContext.GetPlatform();
                var pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);
                viewModel.AvailablePallets = pallets.Where(p => !p.IsClosed && p.Id != viewModel.SourcePalletId).ToList();

                // Common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                return View(viewModel);
            }
        }

        // POST: Items/Print/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Print(int id)
        {
            try
            {
                // Check if item exists
                var item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                // Print the item label
                bool success = await _printerService.PrintItemLabelAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Item label printed successfully.";
                }
                else
                {
                    TempData["WarningMessage"] = "Item label print request sent, but status could not be confirmed.";
                }

                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["ErrorMessage"] = $"Error printing item label: {ex.Message}";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // Helper method for UnitOfMeasure options
        private List<SelectListItem> GetUnitOfMeasureOptions(string selectedUnit)
        {
            var units = Enum.GetValues(typeof(UnitOfMeasure)).Cast<UnitOfMeasure>();
            return units.Select(u => new SelectListItem
            {
                Text = $"{u.GetDescription()} ({u})",
                Value = u.ToString(),
                Selected = u.ToString() == selectedUnit
            }).ToList();
        }
    }
}