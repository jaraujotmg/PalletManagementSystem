using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="palletRepository">The pallet repository</param>
        /// <param name="itemRepository">The item repository</param>
        /// <param name="logger">The logger</param>
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
        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
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
            if (string.IsNullOrWhiteSpace(partialKeyword) || partialKeyword.Length < 2)
            {
                return Enumerable.Empty<SearchSuggestionDto>();
            }

            try
            {
                var suggestions = new List<SearchSuggestionDto>();

                // Search for pallets
                var pallets = (await _palletRepository.SearchAsync(partialKeyword))
                    .Take(maxResults);

                foreach (var pallet in pallets)
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
                var items = (await _itemRepository.SearchAsync(partialKeyword))
                    .Take(maxResults);

                foreach (var item in items)
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

                // Add "View All" suggestion if there are results
                if (suggestions.Any())
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

                return suggestions.Take(maxResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting search suggestions for '{partialKeyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<PalletDto>();
            }

            try
            {
                var pallets = await _palletRepository.SearchAsync(keyword);
                return pallets.Select(p => new PalletDto
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ItemDto>();
            }

            try
            {
                var items = await _itemRepository.SearchAsync(keyword);
                return items.Select(i => new ItemDto
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> SearchManufacturingOrdersAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<string>();
            }

            try
            {
                var pallets = await _palletRepository.SearchAsync(keyword);
                return pallets
                    .Select(p => p.ManufacturingOrder)
                    .Distinct();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching manufacturing orders for '{keyword}'");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClientDto>> SearchClientsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
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

                return clients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching clients for '{keyword}'");
                throw;
            }
        }
    }
}