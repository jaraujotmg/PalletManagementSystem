using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Initializes a new instance of the <see cref="ItemsController"/> class
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
        /// <param name="id">The item ID</param>
        /// <returns>Item details view</returns>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                // Get item with pallet
                var item = await _itemService.GetItemWithPalletAsync(id);
                if (item == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Log the view
                _loggingService.LogAudit("View", "Item", item.Id.ToString(), $"Viewed item {item.ItemNumber}");

                return View(item);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error retrieving item details for ID {id}");
                SetErrorMessage("There was an error retrieving the item details. Please try again.");
                return RedirectToAction("Index", "Pallets");
            }
        }

        /// <summary>
        /// GET: Items/CreateItem?palletId=5
        /// Shows form to create a new item on a pallet
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <returns>Create item view</returns>
        public async Task<ActionResult> CreateItem(int palletId)
        {
            try
            {
                // Check if user can edit items
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

                // Create a new model
                var model = new ItemDto
                {
                    PalletId = palletId
                };

                // Pass pallet info to view for display
                ViewBag.Pallet = pallet;

                return View(model);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error loading item creation form for pallet ID {palletId}");
                SetErrorMessage("There was an error loading the item creation form. Please try again.");
                return RedirectToAction("Details", "Pallets", new { id = palletId });
            }
        }

        /// <summary>
        /// POST: Items/CreateItem
        /// Processes the create item form
        /// </summary>
        /// <param name="model">The item data</param>
        /// <returns>Redirect to pallet details or back to form with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateItem(ItemDto model)
        {
            try
            {
                // Check if user can edit items
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

                // Pass pallet info to view for redisplay if needed
                ViewBag.Pallet = pallet;

                if (ModelState.IsValid)
                {
                    // Create the item
                    var item = await _itemService.CreateItemAsync(model, model.PalletId, _userContext.GetUsername());

                    // Log the creation
                    _loggingService.LogAudit("Create", "Item", item.Id.ToString(),
                        $"Created item {item.ItemNumber} on pallet {pallet.PalletNumber}");

                    SetSuccessMessage($"Item {item.ItemNumber} created successfully.");
                    return RedirectToAction("Details", "Pallets", new { id = model.PalletId });
                }

                return View(model);
            }
            catch (PalletClosedException pcEx)
            {
                _loggingService.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Details", "Pallets", new { id = model.PalletId });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating item");
                ModelState.AddModelError("", "There was an error creating the item. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Items/Edit/5
        /// Shows form to edit an item
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>Edit item view</returns>
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                // Check if user can edit items
                if (!_userContext.CanEditItems())
                {
                    SetErrorMessage("You do not have permission to edit items.");
                    return RedirectToAction("Details", new { id });
                }

                // Get the item with pallet
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

                return View(item);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error loading item edit form for ID {id}");
                SetErrorMessage("There was an error loading the item edit form. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// POST: Items/Edit/5
        /// Processes the edit item form
        /// </summary>
        /// <param name="model">The item data</param>
        /// <returns>Redirect to item details or back to form with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ItemDto model)
        {
            try
            {
                // Check if user can edit items
                if (!_userContext.CanEditItems())
                {
                    SetErrorMessage("You do not have permission to edit items.");
                    return RedirectToAction("Details", new { id = model.Id });
                }

                // Get the original item to check if pallet is closed
                var originalItem = await _itemService.GetItemWithPalletAsync(model.Id);
                if (originalItem == null)
                {
                    SetErrorMessage("Item not found.");
                    return RedirectToAction("Index", "Pallets");
                }

                // Check if pallet is closed
                if (originalItem.Pallet != null && originalItem.Pallet.IsClosed)
                {
                    SetErrorMessage("Cannot edit items on a closed pallet.");
                    return RedirectToAction("Details", new { id = model.Id });
                }

                if (ModelState.IsValid)
                {
                    // Only update editable properties (Weight, Width, Quality, Batch)
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

                return View(model);
            }
            catch (PalletClosedException pcEx)
            {
                _loggingService.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Details", new { id = model.Id });
            }
            catch (ItemValidationException ivEx)
            {
                _loggingService.LogWarning(ivEx.Message);
                ModelState.AddModelError("", ivEx.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error updating item with ID {model.Id}");
                ModelState.AddModelError("", "There was an error updating the item. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Items/Move/5
        /// Shows form to move an item to another pallet
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>Move item view</returns>
        public async Task<ActionResult> Move(int id)
        {
            try
            {
                // Check if user can move items
                if (!_userContext.CanMoveItems())
                {
                    SetErrorMessage("You do not have permission to move items.");
                    return RedirectToAction("Details", new { id });
                }

                // Get the item with pallet
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

                // Get current division and platform
                var division = _userContext.GetDivision();
                var platform = _userContext.GetPlatform();

                // Get open pallets for the division and platform
                var pallets = await _palletService.GetPalletsByDivisionAndStatusAsync(division, false);

                // Exclude the current pallet
                var availablePallets = pallets.Where(p => p.Id != item.PalletId).ToList();

                ViewBag.Item = item;
                ViewBag.SourcePallet = item.Pallet;
                ViewBag.AvailablePallets = availablePallets;

                return View();
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error loading move item form for ID {id}");
                SetErrorMessage("There was an error loading the move item form. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// POST: Items/Move
        /// Processes the move item form
        /// </summary>
        /// <param name="itemId">The item ID</param>
        /// <param name="targetPalletId">The target pallet ID</param>
        /// <returns>Redirect to item details or back to form with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Move(int itemId, int targetPalletId)
        {
            try
            {
                // Check if user can move items
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

                // Get the target pallet to verify it exists and is not closed
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

                // Move the item
                var updatedItem = await _itemService.MoveItemToPalletAsync(itemId, targetPalletId);

                // Log the move
                _loggingService.LogAudit("Move", "Item", updatedItem.Id.ToString(),
                    $"Moved item {updatedItem.ItemNumber} to pallet {targetPallet.PalletNumber}");

                SetSuccessMessage($"Item {updatedItem.ItemNumber} moved successfully to pallet {targetPallet.PalletNumber}.");
                return RedirectToAction("Details", new { id = itemId });
            }
            catch (PalletClosedException pcEx)
            {
                _loggingService.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Move", new { id = itemId });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error moving item with ID {itemId} to pallet {targetPalletId}");
                SetErrorMessage("There was an error moving the item. Please try again.");
                return RedirectToAction("Move", new { id = itemId });
            }
        }

        /// <summary>
        /// GET: Items/PrintLabel/5
        /// Prints a label for an item
        /// </summary>
        /// <param name="id">The item ID</param>
        /// <returns>Redirect to item details with result</returns>
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

                // Log the print
                _loggingService.LogAudit("Print", "Item", item.Id.ToString(),
                    $"Printed label for item {item.ItemNumber}");

                SetSuccessMessage($"Print job for item {item.ItemNumber} sent successfully.");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error printing label for item ID {id}");
                SetErrorMessage("There was an error printing the item label. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }
    }
}