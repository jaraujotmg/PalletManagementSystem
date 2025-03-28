// src/PalletManagementSystem.Web/Controllers/SearchController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Infrastructure.Identity;
using PalletManagementSystem.Web2.ViewModels.Pallets;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.Controllers
{
    [Authorize]
    public class SearchController : BaseController
    {
        private readonly ISearchService _searchService;
        private readonly IPalletService _palletService;
        private readonly IItemService _itemService;
        private readonly IUserPreferenceService _userPreferenceService;

        public SearchController(
            IUserContext userContext,
            ISearchService searchService,
            IPalletService palletService,
            IItemService itemService,
            IUserPreferenceService userPreferenceService)
            : base(userContext)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            _palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
        }

        // GET: Search/Results
        public async Task<ActionResult> Results(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return RedirectToAction("Index", "Home");
                }

                var division = UserContext.GetDivision();
                var platform = UserContext.GetPlatform();

                // Search for pallets, items, manufacturing orders, and clients
                var palletResults = await _palletService.SearchPalletsAsync(keyword);

                // Filter pallets by division and platform
                palletResults = palletResults
                    .Where(p => p.Division == division.ToString() && p.Platform == platform.ToString())
                    .ToList();

                var itemResults = await _itemService.SearchItemsAsync(keyword);

                // Search for manufacturing orders and clients
                var searchResults = await _searchService.SearchAsync(keyword);
                var manufacturingOrderResults = await _searchService.SearchManufacturingOrdersAsync(keyword);
                var clientResults = await _searchService.SearchClientsAsync(keyword);

                var viewModel = new SearchResultsViewModel
                {
                    Keyword = keyword,
                    PalletResults = palletResults.ToList(),
                    ItemResults = itemResults.ToList(),
                    ManufacturingOrderResults = manufacturingOrderResults.ToList(),
                    ClientResults = clientResults.ToList(),

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
                ModelState.AddModelError("", $"Error performing search: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Search/Suggestions
        [HttpGet]
        public async Task<ActionResult> Suggestions(string keyword, int maxResults = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                {
                    return Json(new { success = false, message = "Search term must be at least 2 characters" }, JsonRequestBehavior.AllowGet);
                }

                var suggestions = await _searchService.GetSearchSuggestionsAsync(keyword, maxResults);

                // Create URLs for suggestions
                var suggestionViewModels = suggestions.Select(s => new SearchSuggestionDto
                {
                    Text = s.Text,
                    Type = s.Type,
                    Url = GetUrlForSuggestion(s),
                    EntityId = s.EntityId,
                    IsViewAll = s.IsViewAll
                }).ToList();

                // Add "View All" suggestion if needed
                if (suggestionViewModels.Count > 0 && !suggestionViewModels.Any(s => s.IsViewAll))
                {
                    suggestionViewModels.Add(new SearchSuggestionDto
                    {
                        Text = $"View all results for \"{keyword}\"",
                        Type = "ViewAll",
                        Url = Url.Action("Results", "Search", new { keyword }),
                        IsViewAll = true
                    });
                }

                var viewModel = new SearchSuggestionViewModel
                {
                    Suggestions = suggestionViewModels,
                    Keyword = keyword,
                    ActionUrl = Url.Action("Results", "Search")
                };

                return PartialView("_SearchSuggestions", viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // AJAX POST: Search/QuickSearch
        [HttpPost]
        public async Task<JsonResult> QuickSearch(string keyword, int maxResults = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                {
                    return Json(new { success = false, message = "Search term must be at least 2 characters" });
                }

                var suggestions = await _searchService.GetSearchSuggestionsAsync(keyword, maxResults);

                // Create URLs for suggestions
                var suggestionViewModels = suggestions.Select(s => new
                {
                    text = s.Text,
                    type = s.Type,
                    url = GetUrlForSuggestion(s),
                    entityId = s.EntityId
                }).ToList();

                return Json(new { success = true, results = suggestionViewModels });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to generate URLs for search suggestions
        private string GetUrlForSuggestion(SearchSuggestionDto suggestion)
        {
            if (!string.IsNullOrEmpty(suggestion.Url))
            {
                return suggestion.Url;
            }

            if (suggestion.Type == "Pallet" && suggestion.EntityId.HasValue)
            {
                return Url.Action("Details", "Pallets", new { id = suggestion.EntityId.Value });
            }
            else if (suggestion.Type == "Item" && suggestion.EntityId.HasValue)
            {
                return Url.Action("Details", "Items", new { id = suggestion.EntityId.Value });
            }
            else if (suggestion.Type == "Order")
            {
                return Url.Action("Index", "Pallets", new { manufacturingOrder = suggestion.Text });
            }
            else if (suggestion.Type == "Client")
            {
                string clientCode = suggestion.Text;

                // Extract client code if in format "CODE - NAME"
                int dashIndex = suggestion.Text.IndexOf(" - ");
                if (dashIndex > 0)
                {
                    clientCode = suggestion.Text.Substring(0, dashIndex);
                }

                return Url.Action("Index", "Items", new { clientCode });
            }
            else
            {
                return Url.Action("Results", "Search", new { keyword = suggestion.Text });
            }
        }
    }
}