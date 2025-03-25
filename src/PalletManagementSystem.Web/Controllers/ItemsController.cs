// src/PalletManagementSystem.Web/Controllers/ItemsController.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web.ViewModels.Items;
using PalletManagementSystem.Web.ViewModels.Pallets;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.Controllers
{
    [Authorize]
    public class ItemsController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IPalletService _palletService;
        private readonly IPrinterService _printerService;
        private readonly ISearchService _searchService;
        private readonly IUserPreferenceService _userPreferenceService;

        public ItemsController(
            IUserContext userContext,
            IItemService itemService,
            IPalletService palletService,
            IPrinterService printerService,
            ISearchService searchService,
            IUserPreferenceService userPreferenceService)
            : base(userContext)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
        }

        // GET: Items
        public async Task<ActionResult> Index(int? palletId = null, string clientCode = null, string manufacturingOrder = null,
            string keyword = null, int page = 1, int pageSize = 20)
        {
            try
            {
                // If no page size is specified, get the user's preference
                if (pageSize <= 0)
                {
                    pageSize = await _userPreferenceService.GetItemsPerPageAsync(Username);
                }

                // Get items based on filters
                var items = await _itemService.GetPagedItemsAsync(
                    page, pageSize, palletId, clientCode, manufacturingOrder, keyword);

                // Prepare view model
                var viewModel = new ItemListViewModel
                {
                    Items = items,
                    PalletId = palletId,
                    ClientCode = clientCode,
                    ManufacturingOrder = manufacturingOrder,
                    SearchKeyword = keyword,
                    PageNumber = page,
                    PageSize = pageSize,
                    CanCreate = UserContext.CanEditItems(),
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
                ModelState.AddModelError("", $"Error retrieving items: {ex.Message}");
                return View(new ItemListViewModel
                {
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform()
                });
            }
        }

        // GET: Items/Details/5
        public async Task<ActionResult> Details(int id)
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
                    ReturnUrl = Request.UrlReferrer?.PathAndQuery ?? Url.Action("Index", "Pallets"),
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
                ModelState.AddModelError("", $"Error retrieving item details: {ex.Message}");
                return RedirectToAction("Index", "Pallets");
            }
        }

        // GET: Items/Create/5 (palletId)
        public async Task<ActionResult> Create(int palletId)
        {
            try
            {
                var pallet = await _palletService.GetPalletByIdAsync(palletId);
                if (pallet == null)
                {
                    return HttpNotFound();
                }

                if (pallet.IsClosed)
                {
                    ModelState.AddModelError("", "Cannot add items to a closed pallet");
                    return RedirectToAction("Details", "Pallets", new { id = palletId });
                }

                if (!UserContext.CanEditItems())
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                var viewModel = new CreateItemViewModel
                {
                    PalletId = palletId,
                    PalletNumber = pallet.PalletNumber,
                    ManufacturingOrder = pallet.ManufacturingOrder,
                    QuantityUnit = "PC",
                    WeightUnit = "KG",
                    WidthUnit = "CM",
                    Quality = "Standard",
                    ReturnUrl = Request.UrlReferrer?.PathAndQuery ?? Url.Action("Details", "Pallets", new { id = palletId }),
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    EnableTouchMode = await _userPreferenceService.GetTouchModeEnabledAsync(Username)
                };

                // Populate dropdown options
                viewModel.QuantityUnitOptions = GetQuantityUnitOptions();
                viewModel.WeightUnitOptions = GetWeightUnitOptions();
                viewModel.WidthUnitOptions = GetWidthUnitOptions();
                viewModel.QualityOptions = GetQualityOptions();

                return View(viewModel);
            }
            catch (Exception ex)
            {
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
                // Re-populate dropdown options
                viewModel.QuantityUnitOptions = GetQuantityUnitOptions();
                viewModel.WeightUnitOptions = GetWeightUnitOptions();
                viewModel.WidthUnitOptions = GetWidthUnitOptions();
                viewModel.QualityOptions = GetQualityOptions();

                // Update common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                if (!UserContext.CanEditItems())
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                // Check if pallet exists and is open
                var pallet = await _palletService.GetPalletByIdAsync(viewModel.PalletId);
                if (pallet == null)
                {
                    ModelState.AddModelError("", "Pallet not found");
                    return View(viewModel);
                }

                if (pallet.IsClosed)
                {
                    ModelState.AddModelError("", "Cannot add items to a closed pallet");
                    return View(viewModel);
                }

                // Create the DTO
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
                var newItem = await _itemService.CreateItemAsync(itemDto, viewModel.PalletId, Username);

                TempData["SuccessMessage"] = $"Item created successfully";
                return RedirectToAction("Details", "Pallets", new { id = viewModel.PalletId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating item: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Items/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                if (item.Pallet?.IsClosed ?? false)
                {
                    ModelState.AddModelError("", "Cannot edit items on a closed pallet");
                    return RedirectToAction("Details", new { id });
                }

                if (!UserContext.CanEditItems())
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                var viewModel = new ItemEditViewModel
                {
                    Username = Username,
                    DisplayName = await GetDisplayName(),
                    CurrentDivision = UserContext.GetDivision(),
                    CurrentPlatform = UserContext.GetPlatform(),
                    TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    EnableTouchMode = await _userPreferenceService.GetTouchModeEnabledAsync(Username),
                    ReturnUrl = Request.UrlReferrer?.PathAndQuery ?? Url.Action("Details", new { id })
                };

                // Populate the ViewModel from the DTO
                viewModel.PopulateFromDto(item);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error retrieving item for editing: {ex.Message}");
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
                // Update common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                if (!UserContext.CanEditItems())
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                // Check if the item is on a closed pallet
                var existingItem = await _itemService.GetItemDetailByIdAsync(viewModel.ItemId);
                if (existingItem == null)
                {
                    return HttpNotFound();
                }

                if (existingItem.Pallet?.IsClosed ?? false)
                {
                    ModelState.AddModelError("", "Cannot edit items on a closed pallet");
                    return View(viewModel);
                }

                // Create the update DTO
                var updateDto = new UpdateItemDto
                {
                    Weight = viewModel.Weight,
                    Width = viewModel.Width,
                    Quality = viewModel.Quality,
                    Batch = viewModel.Batch
                };

                // Update the item
                await _itemService.UpdateItemAsync(viewModel.ItemId, updateDto);

                TempData["SuccessMessage"] = "Item updated successfully";
                return !string.IsNullOrEmpty(viewModel.ReturnUrl)
                    ? Redirect(viewModel.ReturnUrl)
                    : RedirectToAction("Details", new { id = viewModel.ItemId });
            }
            catch (PalletClosedException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating item: {ex.Message}");
                return View(viewModel);
            }
        }

        // GET: Items/Move/5
        public async Task<ActionResult> Move(int id)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                if (item.Pallet?.IsClosed ?? false)
                {
                    ModelState.AddModelError("", "Cannot move items from a closed pallet");
                    return RedirectToAction("Details", new { id });
                }

                if (!UserContext.CanMoveItems())
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                }

                // Get open pallets to move to
                var openPallets = await _palletService.GetPalletsByStatusAsync(false);
                var filteredPallets = openPallets.Where(p => p.Id != item.PalletId).ToList();

                var viewModel = new MovePalletItemViewModel
                {
                    ItemId = item.Id,
                    ItemNumber = item.ItemNumber,
                    Item = item,
                    SourcePalletId = item.PalletId,
                    SourcePalletNumber = item.Pallet?.PalletNumber,
                    AvailablePallets = filteredPallets.ToList(),
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
                ModelState.AddModelError("", $"Error preparing item move: {ex.Message}");
                return RedirectToAction("Index", "Pallets");
            }
        }

        // POST: Items/Move
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Move(MovePalletItemViewModel viewModel)
        {
            try
            {
                // Update common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                if (viewModel.CreateNewPallet)
                {
                    // Logic for creating a new pallet and moving the item there
                    if (string.IsNullOrWhiteSpace(viewModel.NewPalletManufacturingOrder))
                    {
                        ModelState.AddModelError("NewPalletManufacturingOrder", "Manufacturing Order is required");

                        // Re-populate available pallets
                        var openPallets = await _palletService.GetPalletsByStatusAsync(false);
                        viewModel.AvailablePallets = openPallets.Where(p => p.Id != viewModel.SourcePalletId).ToList();

                        return View(viewModel);
                    }

                    // Create the new pallet
                    var division = UserContext.GetDivision();
                    var platform = UserContext.GetPlatform();
                    var unitOfMeasure = Enum.Parse<UnitOfMeasure>(viewModel.NewPalletUnitOfMeasure);

                    var newPallet = await _palletService.CreatePalletAsync(
                        viewModel.NewPalletManufacturingOrder,
                        division,
                        platform,
                        unitOfMeasure,
                        Username);

                    if (viewModel.MoveToNewPallet)
                    {
                        // Move the item to the new pallet
                        await _itemService.MoveItemToPalletAsync(viewModel.ItemId, newPallet.Id);
                        TempData["SuccessMessage"] = $"Item {viewModel.ItemNumber} moved to new pallet {newPallet.PalletNumber}";
                        return RedirectToAction("Details", "Pallets", new { id = newPallet.Id });
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"New pallet {newPallet.PalletNumber} created";
                        return RedirectToAction("Details", "Pallets", new { id = newPallet.Id });
                    }
                }
                else
                {
                    if (!ModelState.IsValid)
                    {
                        // Re-populate available pallets
                        var openPallets = await _palletService.GetPalletsByStatusAsync(false);
                        viewModel.AvailablePallets = openPallets.Where(p => p.Id != viewModel.SourcePalletId).ToList();

                        return View(viewModel);
                    }

                    if (!UserContext.CanMoveItems())
                    {
                        return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                    }

                    // Check if the item can be moved
                    bool canMove = await _itemService.CanMoveItemToPalletAsync(viewModel.ItemId, viewModel.TargetPalletId);
                    if (!canMove)
                    {
                        ModelState.AddModelError("", "Cannot move item to selected pallet");

                        // Re-populate available pallets
                        var openPallets = await _palletService.GetPalletsByStatusAsync(false);
                        viewModel.AvailablePallets = openPallets.Where(p => p.Id != viewModel.SourcePalletId).ToList();

                        return View(viewModel);
                    }

                    // Move the item
                    await _itemService.MoveItemToPalletAsync(viewModel.ItemId, viewModel.TargetPalletId);

                    // Get the target pallet number for display
                    var targetPallet = await _palletService.GetPalletByIdAsync(viewModel.TargetPalletId);
                    TempData["SuccessMessage"] = $"Item {viewModel.ItemNumber} moved to pallet {targetPallet.PalletNumber}";

                    return RedirectToAction("Details", "Pallets", new { id = viewModel.TargetPalletId });
                }
            }
            catch (PalletClosedException ex)
            {
                ModelState.AddModelError("", ex.Message);

                // Re-populate available pallets
                var openPallets = await _palletService.GetPalletsByStatusAsync(false);
                viewModel.AvailablePallets = openPallets.Where(p => p.Id != viewModel.SourcePalletId).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error moving item: {ex.Message}");

                // Re-populate available pallets
                var openPallets = await _palletService.GetPalletsByStatusAsync(false);
                viewModel.AvailablePallets = openPallets.Where(p => p.Id != viewModel.SourcePalletId).ToList();

                return View(viewModel);
            }
        }

        // GET: Items/Print/5
        public async Task<ActionResult> Print(int id)
        {
            try
            {
                var item = await _itemService.GetItemDetailByIdAsync(id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                // Get available printers
                var printers = await _printerService.GetAvailablePrintersAsync(PrinterType.ItemLabel);

                // Get user's default printer
                var defaultPrinter = await _printerService.GetDefaultItemLabelPrinterAsync(Username);

                var viewModel = new PrintItemViewModel
                {
                    ItemId = id,
                    Item = item,
                    PrinterName = defaultPrinter,
                    AvailablePrinters = printers.Select(p => new SelectListItem { Text = p, Value = p }).ToList(),
                    SaveAsDefault = false,
                    ShowPreview = true,
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
                ModelState.AddModelError("", $"Error preparing to print item label: {ex.Message}");
                return RedirectToAction("Details", new { id });
            }
        }

        // POST: Items/Print
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Print(PrintItemViewModel viewModel)
        {
            try
            {
                // Update common ViewModel properties
                viewModel.Username = Username;
                viewModel.DisplayName = await GetDisplayName();
                viewModel.CurrentDivision = UserContext.GetDivision();
                viewModel.CurrentPlatform = UserContext.GetPlatform();
                viewModel.TouchModeEnabled = await _userPreferenceService.GetTouchModeEnabledAsync(Username);

                if (!ModelState.IsValid)
                {
                    // Re-populate printer options
                    var printers = await _printerService.GetAvailablePrintersAsync(PrinterType.ItemLabel);
                    viewModel.AvailablePrinters = printers.Select(p => new SelectListItem { Text = p, Value = p }).ToList();

                    return View(viewModel);
                }

                // Get the item to make sure it exists
                var item = await _itemService.GetItemDetailByIdAsync(viewModel.ItemId);
                if (item == null)
                {
                    return HttpNotFound();
                }

                // Print the item label
                bool printResult = await _printerService.PrintItemLabelAsync(viewModel.ItemId);

                if (!printResult)
                {
                    ModelState.AddModelError("", "Failed to print item label");

                    // Re-populate printer options
                    var printers = await _printerService.GetAvailablePrintersAsync(PrinterType.ItemLabel);
                    viewModel.AvailablePrinters = printers.Select(p => new SelectListItem { Text = p, Value = p }).ToList();

                    return View(viewModel);
                }

                // Save printer preference if requested
                if (viewModel.SaveAsDefault)
                {
                    await _printerService.SetDefaultItemLabelPrinterAsync(Username, viewModel.PrinterName);
                }

                TempData["SuccessMessage"] = $"Item label printed successfully to {viewModel.PrinterName}";
                return RedirectToAction("Details", new { id = viewModel.ItemId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error printing item label: {ex.Message}");

                // Re-populate printer options
                var printers = await _printerService.GetAvailablePrintersAsync(PrinterType.ItemLabel);
                viewModel.AvailablePrinters = printers.Select(p => new SelectListItem { Text = p, Value = p }).ToList();

                return View(viewModel);
            }
        }

        // Helper methods for dropdown options
        private List<SelectListItem> GetQuantityUnitOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Piece (PC)", Value = "PC", Selected = true },
                new SelectListItem { Text = "Kilogram (KG)", Value = "KG" },
                new SelectListItem { Text = "Box (BOX)", Value = "BOX" },
                new SelectListItem { Text = "Roll (ROLL)", Value = "ROLL" }
            };
        }

        private List<SelectListItem> GetWeightUnitOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Kilogram (KG)", Value = "KG", Selected = true },
                new SelectListItem { Text = "Gram (G)", Value = "G" },
                new SelectListItem { Text = "Pound (LB)", Value = "LB" }
            };
        }

        private List<SelectListItem> GetWidthUnitOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Centimeter (CM)", Value = "CM", Selected = true },
                new SelectListItem { Text = "Millimeter (MM)", Value = "MM" },
                new SelectListItem { Text = "Inch (IN)", Value = "IN" }
            };
        }

        private List<SelectListItem> GetQualityOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Premium", Value = "Premium" },
                new SelectListItem { Text = "Standard", Value = "Standard", Selected = true },
                new SelectListItem { Text = "Economy", Value = "Economy" },
                new SelectListItem { Text = "Special", Value = "Special" }
            };
        }
    }

    // This class is needed to support the ItemsController
    public class ItemListViewModel : ViewModelBase
    {
        public PagedResultDto<ItemListDto> Items { get; set; }
        public int? PalletId { get; set; }
        public string ClientCode { get; set; }
        public string ManufacturingOrder { get; set; }
        public string SearchKeyword { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool CanCreate { get; set; }
    }

    // This class is needed to support the ItemsController
    public class PrintItemViewModel : ViewModelBase
    {
        public int ItemId { get; set; }
        public ItemDetailDto Item { get; set; }

        [Required(ErrorMessage = "Printer is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; }

        public List<SelectListItem> AvailablePrinters { get; set; } = new List<SelectListItem>();

        [Display(Name = "Save as default printer")]
        public bool SaveAsDefault { get; set; } = false;

        [Display(Name = "Print preview")]
        public bool ShowPreview { get; set; } = true;
    }
}