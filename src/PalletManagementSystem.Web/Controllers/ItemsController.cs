using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Logging;
using PalletManagementSystem.Web.ViewModels.ItemViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller for item operations
    /// </summary>
    [Authorize]
    public class ItemsController : BaseController
    {
        private readonly IItemService _itemService;
        private readonly IPalletService _palletService;
        private readonly IPrinterService _printerService;
        private readonly UserContext _userContext;
        private readonly LoggingService _loggingService;
        private readonly ILogger<ItemsController> _logger;

        /// <summary>
        /// Initializes a new instance of ItemsController
        /// </summary>
        public ItemsController(
            IItemService itemService,
            IPalletService palletService,
            IPrinterService printerService,
            UserContext userContext,
            LoggingService loggingService,
            ILogger<ItemsController> logger)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _printerService = printerService ?? throw new ArgumentNullException(nameof(printerService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: Items/Details/5
        /// Shows details for a specific item
        /// </summary>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                // Get the item with its associated pallet
                var item = await _itemService.GetItemWithPalletAsync(id);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Create the view model
                var viewModel = ItemDetailsViewModel.FromItemAndPalletDto(item, item.Pallet);

                // Log the view action
                _loggingService.LogAudit("View", "Item", item.Id.ToString(),
                    $"Viewed item {item.ItemNumber}");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving item details for ID {id}");
                SetErrorMessage("An error occurred while retrieving the item details. Please try again.");
                return RedirectToAction("Index", "Pallets");
            }
        }

        /// <summary>
        /// GET: Items/CreateItem?palletId=5
        /// Shows form to create a new item on a pallet
        /// </summary>
        public async Task<ActionResult> CreateItem(int palletId)
        {
            try
            {
                // Check if user has permission to create items
                if (!_userContext.CanEditItems())
                {
                    SetErrorMessage("You do not have permission to create items.");
                    return RedirectToAction("Details", "Pallets", new { id = palletId });
                }

                // Get the pallet to verify it exists and is not closed
                var pallet = await _palletService.GetPalletByIdAsync(palletId);
                if (pallet == null)
                {
                    SetErrorMessage("Pallet not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                if (pallet.IsClosed)
                {
                    SetErrorMessage("Cannot add items to a closed pallet.");
                    return RedirectToAction("Details", "Pallets", new { id = palletId });
                }

                // Get touch mode status
                bool touchModeEnabled = Session["TouchModeEnabled"] != null &&
                                       (bool)Session["TouchModeEnabled"];

                // Create view model
                var viewModel = new CreateItemViewModel(palletId, pallet.PalletNumber, touchModeEnabled);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading item creation form for pallet ID {palletId}");
                SetErrorMessage("An error occurred while loading the item creation form. Please try again.");
                return RedirectToAction("Details", "Pallets", new { id = palletId });
            }
        }

        /// <summary>
        /// POST: Items/CreateItem
        /// Processes the item creation form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateItem(CreateItemViewModel model)
        {
            try
            {
                // Check if user has permission to create items
                if (!_userContext.CanEditItems())
                {
                    SetErrorMessage("You do not have permission to create items.");
                    return RedirectToAction("Details", "Pallets", new { id = model.PalletId });
                }

                // Get the pallet to verify it exists and is not closed
                var pallet = await _palletService.GetPalletByIdAsync(model.PalletId);
                if (pallet == null)
                {
                    SetErrorMessage("Pallet not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                if (pallet.IsClosed)
                {
                    SetErrorMessage("Cannot add items to a closed pallet.");
                    return RedirectToAction("Details", "Pallets", new { id = model.PalletId });
                }

                if (ModelState.IsValid)
                {
                    // Map view model to DTO
                    var itemDto = new ItemDto
                    {
                        PalletId = model.PalletId,
                        ManufacturingOrder = model.ManufacturingOrder,
                        ManufacturingOrderLine = model.ManufacturingOrderLine,
                        ServiceOrder = model.ServiceOrder,
                        ServiceOrderLine = model.ServiceOrderLine,
                        FinalOrder = model.FinalOrder,
                        FinalOrderLine = model.FinalOrderLine,
                        ClientCode = model.ClientCode,
                        ClientName = model.ClientName,
                        Reference = model.Reference,
                        Finish = model.Finish,
                        Color = model.Color,
                        Quantity = model.Quantity,
                        QuantityUnit = model.QuantityUnit,
                        Weight = model.Weight,
                        WeightUnit = model.WeightUnit,
                        Width = model.Width,
                        WidthUnit = model.WidthUnit,
                        Quality = model.Quality,
                        Batch = model.Batch
                    };

                    // Create the item
                    var item = await _itemService.CreateItemAsync(itemDto, model.PalletId, _userContext.GetUsername());

                    // Log the creation
                    _loggingService.LogAudit("Create", "Item", item.Id.ToString(),
                        $"Created item {item.ItemNumber} on pallet {pallet.PalletNumber}");

                    SetSuccessMessage($"Item {item.ItemNumber} created successfully.");
                    return RedirectToAction("Details", "Pallets", new { id = model.PalletId });
                }

                // If we got here, there was a validation error
                model.PalletNumber = pallet.PalletNumber; // Ensure pallet number is set for display
                return View(model);
            }
            catch (PalletClosedException pcEx)
            {
                _logger.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Details", "Pallets", new { id = model.PalletId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                ModelState.AddModelError("", "An error occurred while creating the item. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Items/Edit/5
        /// Shows form to edit an item
        /// </summary>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                // Check if user has permission to edit items
                if (!_userContext.CanEditItems())
                {
                    SetErrorMessage("You do not have permission to edit items.");
                    return RedirectToAction("Details", new { id });
                }

                // Get the item with its associated pallet
                var item = await _itemService.GetItemWithPalletAsync(id);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Check if pallet is closed
                if (item.Pallet != null && item.Pallet.IsClosed)
                {
                    SetErrorMessage("Cannot edit items on a closed pallet.");
                    return RedirectToAction("Details", new { id });
                }

                // Get touch mode status
                bool touchModeEnabled = Session["TouchModeEnabled"] != null &&
                                       (bool)Session["TouchModeEnabled"];

                // Create view model
                var viewModel = EditItemViewModel.FromItemDto(
                    item,
                    item.Pallet?.PalletNumber,
                    touchModeEnabled);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading item edit form for ID {id}");
                SetErrorMessage("An error occurred while loading the item edit form. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// POST: Items/Edit
        /// Processes the item edit form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditItemViewModel model)
        {
            try
            {
                // Check if user has permission to edit items
                if (!_userContext.CanEditItems())
                {
                    SetErrorMessage("You do not have permission to edit items.");
                    return RedirectToAction("Details", new { id = model.Id });
                }

                // Get the item to verify it exists
                var item = await _itemService.GetItemWithPalletAsync(model.Id);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Check if pallet is closed
                if (item.Pallet != null && item.Pallet.IsClosed)
                {
                    SetErrorMessage("Cannot edit items on a closed pallet.");
                    return RedirectToAction("Details", new { id = model.Id });
                }

                if (ModelState.IsValid)
                {
                    // Only update the editable fields
                    var updatedItem = await _itemService.UpdateItemAsync(
                        model.Id,
                        model.Weight,
                        model.Width,
                        model.Quality,
                        model.Batch);

                    // Log the update
                    _loggingService.LogAudit("Update", "Item", updatedItem.Id.ToString(),
                        $"Updated item {updatedItem.ItemNumber}");

                    SetSuccessMessage($"Item {updatedItem.ItemNumber} updated successfully.");
                    return RedirectToAction("Details", new { id = model.Id });
                }

                // If we got here, there was a validation error
                return View(model);
            }
            catch (PalletClosedException pcEx)
            {
                _logger.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (ItemValidationException ivEx)
            {
                _logger.LogWarning(ivEx.Message);
                ModelState.AddModelError("", ivEx.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating item with ID {model.Id}");
                ModelState.AddModelError("", "An error occurred while updating the item. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Items/Move/5
        /// Shows form to move an item to another pallet
        /// </summary>
        public async Task<ActionResult> Move(int id)
        {
            try
            {
                // Check if user has permission to move items
                if (!_userContext.CanMoveItems())
                {
                    SetErrorMessage("You do not have permission to move items.");
                    return RedirectToAction("Details", new { id });
                }

                // Get the item with its associated pallet
                var item = await _itemService.GetItemWithPalletAsync(id);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Check if source pallet is closed
                if (item.Pallet != null && item.Pallet.IsClosed)
                {
                    SetErrorMessage("Cannot move items from a closed pallet.");
                    return RedirectToAction("Details", new { id });
                }

                // Get available target pallets (open pallets except the current one)
                var division = _userContext.GetDivision();
                var pallets = await _palletService.GetPalletsByDivisionAndStatusAsync(division, false);

                // Create view model
                var viewModel = MoveItemViewModel.FromItemAndPallets(
                    item,
                    item.Pallet,
                    pallets);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading move item form for ID {id}");
                SetErrorMessage("An error occurred while loading the move item form. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// POST: Items/Move
        /// Processes the move item form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveConfirm(int itemId, int targetPalletId, bool createNewPallet = false, string newPalletManufacturingOrder = null)
        {
            try
            {
                // Check if user has permission to move items
                if (!_userContext.CanMoveItems())
                {
                    SetErrorMessage("You do not have permission to move items.");
                    return RedirectToAction("Details", new { id = itemId });
                }

                // Get the item to verify it exists
                var item = await _itemService.GetItemWithPalletAsync(itemId);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Check if source pallet is closed
                if (item.Pallet != null && item.Pallet.IsClosed)
                {
                    SetErrorMessage("Cannot move items from a closed pallet.");
                    return RedirectToAction("Details", new { id = itemId });
                }

                int finalTargetPalletId = targetPalletId;

                // If creating a new pallet
                if (createNewPallet)
                {
                    if (string.IsNullOrWhiteSpace(newPalletManufacturingOrder))
                    {
                        // Redisplay the form with an error
                        ModelState.AddModelError("NewPalletManufacturingOrder", "Manufacturing order is required when creating a new pallet.");
                        return RedirectToAction("Move", new { id = itemId });
                    }

                    // Create the new pallet
                    var newPallet = await _palletService.CreatePalletAsync(
                        newPalletManufacturingOrder,
                        _userContext.GetDivision(),
                        _userContext.GetPlatform(),
                        UnitOfMeasure.PC, // Default unit
                        _userContext.GetUsername());

                    finalTargetPalletId = newPallet.Id;
                }
                else
                {
                    // Verify target pallet exists and is not closed
                    var targetPallet = await _palletService.GetPalletByIdAsync(targetPalletId);
                    if (targetPallet == null)
                    {
                        SetErrorMessage("Target pallet not found.");
                        return RedirectToAction("Move", new { id = itemId });
                    }

                    if (targetPallet.IsClosed)
                    {
                        SetErrorMessage("Cannot move items to a closed pallet.");
                        return RedirectToAction("Move", new { id = itemId });
                    }
                }

                // Move the item
                var updatedItem = await _itemService.MoveItemToPalletAsync(itemId, finalTargetPalletId);

                // Get target pallet for message
                var finalPallet = await _palletService.GetPalletByIdAsync(finalTargetPalletId);

                // Log the move
                _loggingService.LogAudit("Move", "Item", updatedItem.Id.ToString(),
                    $"Moved item {updatedItem.ItemNumber} to pallet {finalPallet.PalletNumber}");

                SetSuccessMessage($"Item {updatedItem.ItemNumber} moved successfully to pallet {finalPallet.PalletNumber}.");
                return RedirectToAction("Details", new { id = itemId });
            }
            catch (PalletClosedException pcEx)
            {
                _logger.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Move", new { id = itemId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error moving item with ID {itemId} to pallet {targetPalletId}");
                SetErrorMessage("An error occurred while moving the item. Please try again.");
                return RedirectToAction("Move", new { id = itemId });
            }
        }

        /// <summary>
        /// GET: Items/PrintLabel/5
        /// Prints a label for an item
        /// </summary>
        public async Task<ActionResult> PrintLabel(int id)
        {
            try
            {
                // Get the item to verify it exists
                var item = await _itemService.GetItemByIdAsync(id);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Print the label
                await _printerService.PrintItemLabelAsync(id);

                // Log the print action
                _loggingService.LogAudit("Print", "Item", item.Id.ToString(),
                    $"Printed label for item {item.ItemNumber}");

                SetSuccessMessage($"Print job for item {item.ItemNumber} sent successfully.");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing label for item ID {id}");
                SetErrorMessage("An error occurred while printing the item label. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }
    }
}