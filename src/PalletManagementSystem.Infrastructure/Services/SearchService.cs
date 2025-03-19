using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the search service
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly IPalletRepository _palletRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<SearchService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class
        /// </summary>
        public SearchService(
            IPalletRepository palletRepository,
            IItemRepository itemRepository,
            ILogger<SearchService> logger)
        {
            _palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
            _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string keyword, int maxResults = 0)
        {
            if (!await ValidateSearchKeywordAsync(keyword))
            {
                return Enumerable.Empty<SearchResultDto>();
            }

            try
            {
                var results = new List<SearchResultDto>();

                // Search for pallets
                var pallets = await _palletRepository.SearchAsync(keyword);
                foreach (var pallet in pallets)
                {
                    results.Add(new SearchResultDto
                    {
                        Id = pallet.Id,
                        EntityType = "Pallet",
                        Identifier = pallet.PalletNumber.Value,
                        AdditionalInfo = $"MO: {pallet.ManufacturingOrder}",
                        ViewUrl = $"/Pallets/Details/{pallet.Id}"
                    });
                }

                // Search for items
                var items = await _itemRepository.SearchAsync(keyword);
                foreach (var item in items)
                {
                    results.Add(new SearchResultDto
                    {
                        Id = item.Id,
                        EntityType = "Item",
                        Identifier = item.ItemNumber,
                        AdditionalInfo = $"Client: {item.ClientName}",
                        ViewUrl = $"/Items/Details/{item.Id}"
                    });
                }

                // Apply max results limit if specified
                if (maxResults > 0 && results.Count > maxResults)
                {
                    results = results.Take(maxResults).ToList();
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchSuggestionDto>> GetSearchSuggestionsAsync(string partialKeyword, int maxResults = 5)
        {
            if (!await ValidateSearchKeywordAsync(partialKeyword) || partialKeyword.Length < 2)
            {
                return Enumerable.Empty<SearchSuggestionDto>();
            }

            try
            {
                var suggestions = new List<SearchSuggestionDto>();

                // Search for pallets
                var pallets = (await _palletRepository.SearchAsync(partialKeyword));
                int palletsToInclude = maxResults > 0 ? Math.Min(pallets.Count(), maxResults / 2) : pallets.Count();

                foreach (var pallet in pallets.Take(palletsToInclude))
                {
                    suggestions.Add(new SearchSuggestionDto
                    {
                        Text = pallet.PalletNumber.Value,
                        Type = "Pallet",
                        Url = $"/Pallets/Details/{pallet.Id}",
                        EntityId = pallet.Id,
                        IsViewAll = false
                    });
                }

                // Search for items
                var items = (await _itemRepository.SearchAsync(partialKeyword));
                int itemsToInclude = maxResults > 0 ? Math.Min(items.Count(), maxResults - suggestions.Count) : items.Count();

                foreach (var item in items.Take(itemsToInclude))
                {
                    suggestions.Add(new SearchSuggestionDto
                    {
                        Text = item.ItemNumber,
                        Type = "Item",
                        Url = $"/Items/Details/{item.Id}",
                        EntityId = item.Id,
                        IsViewAll = false
                    });
                }

                // Add "View All" suggestion if there are results and space
                if (suggestions.Any() && (maxResults <= 0 || suggestions.Count < maxResults))
                {
                    suggestions.Add(new SearchSuggestionDto
                    {
                        Text = $"View all results for '{partialKeyword}'",
                        Type = "ViewAll",
                        Url = $"/Search?q={Uri.EscapeDataString(partialKeyword)}",
                        EntityId = null,
                        IsViewAll = true
                    });
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting search suggestions for '{partialKeyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword, int maxResults = 0)
        {
            if (!await ValidateSearchKeywordAsync(keyword))
            {
                return Enumerable.Empty<PalletDto>();
            }

            try
            {
                var pallets = await _palletRepository.SearchAsync(keyword);
                var results = pallets.Select(p => new PalletDto
                {
                    Id = p.Id,
                    PalletNumber = p.PalletNumber.Value,
                    IsTemporary = p.PalletNumber.IsTemporary,
                    ManufacturingOrder = p.ManufacturingOrder,
                    Division = p.Division.ToString(),
                    Platform = p.Platform.ToString(),
                    UnitOfMeasure = p.UnitOfMeasure.ToString(),
                    Quantity = p.Quantity,
                    ItemCount = p.ItemCount,
                    IsClosed = p.IsClosed,
                    CreatedDate = p.CreatedDate,
                    ClosedDate = p.ClosedDate,
                    CreatedBy = p.CreatedBy
                });

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword, int maxResults = 0)
        {
            if (!await ValidateSearchKeywordAsync(keyword))
            {
                return Enumerable.Empty<ItemDto>();
            }

            try
            {
                var items = await _itemRepository.SearchAsync(keyword);
                var results = items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    ItemNumber = i.ItemNumber,
                    PalletId = i.PalletId,
                    ManufacturingOrder = i.ManufacturingOrder,
                    ManufacturingOrderLine = i.ManufacturingOrderLine,
                    ServiceOrder = i.ServiceOrder,
                    ServiceOrderLine = i.ServiceOrderLine,
                    FinalOrder = i.FinalOrder,
                    FinalOrderLine = i.FinalOrderLine,
                    ClientCode = i.ClientCode,
                    ClientName = i.ClientName,
                    Reference = i.Reference,
                    Finish = i.Finish,
                    Color = i.Color,
                    Quantity = i.Quantity,
                    QuantityUnit = i.QuantityUnit,
                    Weight = i.Weight,
                    WeightUnit = i.WeightUnit,
                    Width = i.Width,
                    WidthUnit = i.WidthUnit,
                    Quality = i.Quality,
                    Batch = i.Batch,
                    CreatedDate = i.CreatedDate,
                    CreatedBy = i.CreatedBy
                });

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> SearchManufacturingOrdersAsync(string keyword, int maxResults = 0)
        {
            if (!await ValidateSearchKeywordAsync(keyword))
            {
                return Enumerable.Empty<string>();
            }

            try
            {
                var pallets = await _palletRepository.SearchAsync(keyword);
                var results = pallets
                    .Select(p => p.ManufacturingOrder)
                    .Distinct();

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching manufacturing orders for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClientDto>> SearchClientsAsync(string keyword, int maxResults = 0)
        {
            if (!await ValidateSearchKeywordAsync(keyword))
            {
                return Enumerable.Empty<ClientDto>();
            }

            try
            {
                var items = await _itemRepository.SearchAsync(keyword);
                var clients = items
                    .GroupBy(i => new { i.ClientCode, i.ClientName })
                    .Select(g => new ClientDto
                    {
                        ClientCode = g.Key.ClientCode,
                        ClientName = g.Key.ClientName,
                        IsSpecial = g.Any(i => i.IsSpecialClient()),
                        ItemCount = g.Count()
                    })
                    .OrderBy(c => c.ClientName);

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    clients = clients.Take(maxResults);
                }

                return clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching clients for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateSearchKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return false;
            }

            keyword = keyword.Trim();

            // Ensure keyword is at least 2 characters long
            if (keyword.Length < 2)
            {
                return false;
            }

            // Check for special characters that might cause SQL injection
            // This is basic validation - real implementation should be more comprehensive
            if (keyword.Contains(";") || keyword.Contains("--") || keyword.Contains("/*") ||
                keyword.Contains("*/") || keyword.Contains("'"))
            {
                return false;
            }

            // For more complex validation, this could be expanded
            // For now, we'll return success asynchronously
            return await Task.FromResult(true);
        }
    }
}