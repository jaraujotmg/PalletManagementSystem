using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Infrastructure.Logging;
using PalletManagementSystem.Web.Models;
using System;
using System.Collections.Generic;
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
        /// Initializes a new instance of the <see cref="PalletsController"/> class
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
        /// Displays a list of all pallets with optional filtering
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="status">Filter by status (all, open, closed)</param>
        /// <param name="searchTerm">Search term</param>
        /// <returns>View with list of pallets</returns>
        public async Task<ActionResult> Index(int page = 1, string status = "all", string searchTerm = null)
        {
            try
            {
                // Get currently selected division and platform from session
                var division = GetCurrentDivision();
                var platform = GetCurrentPlatform();

                // Get page size from user preferences
                int pageSize = await _userPreferenceService.GetItemsPerPageAsync(_userContext.GetUsername());

                // Get pallets based on division, platform and status
                IEnumerable<PalletDto> pallets;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    // Search across all pallets for the search term
                    pallets = await _palletService.SearchPalletsAsync(searchTerm);
                }
                else
                {
                    // Filter by division and platform
                    pallets = await _palletService.GetPalletsByDivisionAndPlatformAsync(division, platform);

                    // Further filter by status if requested
                    if (status == "open")
                    {
                        pallets = pallets.Where(p => !p.IsClosed);
                    }
                    else if (status == "closed")
                    {
                        pallets = pallets.Where(p => p.IsClosed);
                    }
                }

                // Calculate pagination
                int totalItems = pallets.Count();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages));

                // Apply pagination
                var paginatedPallets = pallets
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Set up ViewBag for pagination
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.Status = status;
                ViewBag.SearchTerm = searchTerm;

                // Log info
                _loggingService.LogInfo($"Retrieved {paginatedPallets.Count} pallets for division {division}, platform {platform}");

                return View(paginatedPallets);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error retrieving pallets list");
                SetErrorMessage("There was an error retrieving the pallet list. Please try again.");
                return View(new List<PalletDto>());
            }
        }

        /// <summary>
        /// GET: Pallets/Details/5
        /// Shows detailed view of a specific pallet
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <returns>View with pallet details</returns>
        public async Task<ActionResult> Details(int id)
        {
            try
            {
                // Get pallet with items
                var pallet = await _palletService.GetPalletWithItemsAsync(id);
                if (pallet == null)
                {
                    SetErrorMessage("Pallet not found.");
                    return RedirectToAction("Index");
                }

                // Get activity log - this would normally come from a dedicated service
                // For now, we'll create a simple mock activity log
                var activityLog = new List<dynamic>
                {
                    new {
                        Title = "Pallet created",
                        Subtitle = $"Pallet {pallet.PalletNumber} created",
                        Timestamp = pallet.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                        Username = pallet.CreatedBy,
                        Icon = "fa-pallet",
                        BadgeClass = "badge-primary"
                    }
                };

                if (pallet.Items != null && pallet.Items.Any())
                {
                    // Add item activities
                    foreach (var item in pallet.Items.OrderByDescending(i => i.CreatedDate).Take(3))
                    {
                        activityLog.Add(new
                        {
                            Title = "Item added",
                            Subtitle = $"Item #{item.ItemNumber} added to pallet",
                            Timestamp = item.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                            Username = item.CreatedBy,
                            Icon = "fa-plus",
                            BadgeClass = "badge-primary"
                        });
                    }
                }

                if (pallet.IsClosed && pallet.ClosedDate.HasValue)
                {
                    activityLog.Insert(0, new
                    {
                        Title = "Pallet closed",
                        Subtitle = $"Pallet {pallet.PalletNumber} closed",
                        Timestamp = pallet.ClosedDate.Value.ToString("dd/MM/yyyy HH:mm"),
                        Username = pallet.CreatedBy,
                        Icon = "fa-lock",
                        BadgeClass = "badge-success"
                    });
                }

                ViewBag.ActivityLog = activityLog;

                // Log the view
                _loggingService.LogAudit("View", "Pallet", pallet.Id.ToString(), $"Viewed pallet {pallet.PalletNumber}");

                return View(pallet);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error retrieving pallet details for ID {id}");
                SetErrorMessage("There was an error retrieving the pallet details. Please try again.");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// GET: Pallets/Create
        /// Shows form to create a new pallet
        /// </summary>
        /// <returns>Create pallet view</returns>
        public ActionResult Create()
        {
            try
            {
                // Check if user can create pallets
                if (!_userContext.CanEditPallets())
                {
                    SetErrorMessage("You do not have permission to create pallets.");
                    return RedirectToAction("Index");
                }

                // Create a new model
                var model = new PalletDto
                {
                    Division = GetCurrentDivision().ToString(),
                    Platform = GetCurrentPlatform().ToString(),
                    UnitOfMeasure = UnitOfMeasure.PC.ToString()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error loading pallet creation form");
                SetErrorMessage("There was an error loading the pallet creation form. Please try again.");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST: Pallets/Create
        /// Processes the create pallet form
        /// </summary>
        /// <param name="model">The pallet data</param>
        /// <returns>Redirect to Index or back to form with errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PalletDto model)
        {
            try
            {
                // Check if user can create pallets
                if (!_userContext.CanEditPallets())
                {
                    SetErrorMessage("You do not have permission to create pallets.");
                    return RedirectToAction("Index");
                }

                if (ModelState.IsValid)
                {
                    // Parse enums
                    if (!Enum.TryParse(model.Division, out Division division))
                    {
                        ModelState.AddModelError("Division", "Invalid division.");
                        return View(model);
                    }

                    if (!Enum.TryParse(model.Platform, out Platform platform))
                    {
                        ModelState.AddModelError("Platform", "Invalid platform.");
                        return View(model);
                    }

                    if (!Enum.TryParse(model.UnitOfMeasure, out UnitOfMeasure unitOfMeasure))
                    {
                        ModelState.AddModelError("UnitOfMeasure", "Invalid unit of measure.");
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

                return View(model);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error creating pallet");
                ModelState.AddModelError("", "There was an error creating the pallet. Please try again.");
                return View(model);
            }
        }

        /// <summary>
        /// POST: Pallets/Close/5
        /// Closes a pallet
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <returns>Redirect to Details with result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Close(int id)
        {
            try
            {
                // Check if user can close pallets
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

                // Close the pallet
                var closedPallet = await _palletService.ClosePalletAsync(id);

                // Log the close
                _loggingService.LogAudit("Close", "Pallet", closedPallet.Id.ToString(),
                    $"Closed pallet {closedPallet.PalletNumber}");

                // Check if number changed (temporary to permanent)
                if (pallet.PalletNumber != closedPallet.PalletNumber)
                {
                    SetSuccessMessage($"Pallet closed successfully. Number changed from {pallet.PalletNumber} to {closedPallet.PalletNumber}.");
                }
                else
                {
                    SetSuccessMessage($"Pallet {closedPallet.PalletNumber} closed successfully.");
                }

                return RedirectToAction("Details", new { id });
            }
            catch (PalletClosedException pcEx)
            {
                _loggingService.LogWarning(pcEx.Message);
                SetErrorMessage(pcEx.Message);
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error closing pallet with ID {id}");
                SetErrorMessage("There was an error closing the pallet. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// GET: Pallets/Print/5
        /// Prints a pallet list
        /// </summary>
        /// <param name="id">The pallet ID</param>
        /// <returns>Redirect to Details with result or file download</returns>
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

                // Print the pallet list - this would normally display a print dialog or download the file
                await _printerService.PrintPalletListAsync(id);

                // Log the print
                _loggingService.LogAudit("Print", "Pallet", pallet.Id.ToString(),
                    $"Printed list for pallet {pallet.PalletNumber}");

                SetSuccessMessage($"Print job for pallet {pallet.PalletNumber} sent successfully.");
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error printing pallet list for ID {id}");
                SetErrorMessage("There was an error printing the pallet list. Please try again.");
                return RedirectToAction("Details", new { id });
            }
        }

        /// <summary>
        /// Gets the current division from session or user preferences
        /// </summary>
        /// <returns>The current division</returns>
        private Division GetCurrentDivision()
        {
            return _userContext.GetDivision();
        }

        /// <summary>
        /// Gets the current platform from session or user preferences
        /// </summary>
        /// <returns>The current platform</returns>
        private Platform GetCurrentPlatform()
        {
            return _userContext.GetPlatform();
        }
    }
}