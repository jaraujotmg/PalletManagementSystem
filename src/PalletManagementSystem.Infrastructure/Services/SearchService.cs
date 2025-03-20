using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Interfaces.Repositories;
using PalletManagementSystem.Core.Interfaces.Services;
using PalletManagementSystem.Core.Mappers;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Infrastructure.Data;

namespace PalletManagementSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the search service with projection optimizations
    /// </summary>
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SearchService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class
        /// </summary>
        public SearchService(
            IUnitOfWork unitOfWork,
            ILogger<SearchService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string keyword, int maxResults = 0)
        {
            return await SearchAsync(keyword, maxResults, CancellationToken.None);
        }

        /// <summary>
        /// Performs a general search across all entities
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited)</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>Collection of search results</returns>
        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string keyword, int maxResults = 0, CancellationToken cancellationToken = default)
        {
            if (!await ValidateSearchKeywordAsync(keyword, cancellationToken))
            {
                return Enumerable.Empty<SearchResultDto>();
            }

            try
            {
                var results = new List<SearchResultDto>();

                // Search for pallets using projections
                var palletQuery = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p =>
                        EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                        p.ManufacturingOrder.Contains(keyword))
                    .Select(p => new SearchResultDto
                    {
                        Id = p.Id,
                        EntityType = "Pallet",
                        Identifier = EF.Property<string>(p, "_palletNumberValue"),
                        AdditionalInfo = $"MO: {p.ManufacturingOrder}",
                        ViewUrl = $"/Pallets/Details/{p.Id}"
                    });

                if (maxResults > 0)
                {
                    palletQuery = palletQuery.Take(maxResults / 2); // Take half of the max results for pallets
                }

                results.AddRange(await palletQuery.ToListAsync(cancellationToken));

                // Search for items using projections
                var remainingResults = maxResults > 0 ? maxResults - results.Count : 0;

                var itemQuery = DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i =>
                        i.ItemNumber.Contains(keyword) ||
                        i.ManufacturingOrder.Contains(keyword) ||
                        i.ServiceOrder.Contains(keyword) ||
                        i.FinalOrder.Contains(keyword) ||
                        i.ClientCode.Contains(keyword) ||
                        i.ClientName.Contains(keyword) ||
                        i.Reference.Contains(keyword) ||
                        i.Batch.Contains(keyword))
                    .Select(i => new SearchResultDto
                    {
                        Id = i.Id,
                        EntityType = "Item",
                        Identifier = i.ItemNumber,
                        AdditionalInfo = $"Client: {i.ClientName}",
                        ViewUrl = $"/Items/Details/{i.Id}"
                    });

                if (maxResults > 0 && remainingResults > 0)
                {
                    itemQuery = itemQuery.Take(remainingResults);
                }

                results.AddRange(await itemQuery.ToListAsync(cancellationToken));

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching for '{keyword}'");

                // Fallback to traditional approach
                return await FallbackSearchAsync(keyword, maxResults, cancellationToken);
            }
        }

        /// <summary>
        /// Fallback search method using traditional approach
        /// </summary>
        private async Task<IEnumerable<SearchResultDto>> FallbackSearchAsync(string keyword, int maxResults, CancellationToken cancellationToken)
        {
            var results = new List<SearchResultDto>();

            // Search for pallets
            var pallets = await _unitOfWork.PalletRepository.SearchAsync(keyword, cancellationToken);
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
            var items = await _unitOfWork.ItemRepository.SearchAsync(keyword, cancellationToken);
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

        /// <inheritdoc/>
        public async Task<IEnumerable<SearchSuggestionDto>> GetSearchSuggestionsAsync(string partialKeyword, int maxResults = 5)
        {
            return await GetSearchSuggestionsAsync(partialKeyword, maxResults, CancellationToken.None);
        }

        /// <summary>
        /// Gets search suggestions as the user types
        /// </summary>
        /// <param name="partialKeyword">The partial search keyword</param>
        /// <param name="maxResults">Maximum number of suggestions to return</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>Collection of search suggestions</returns>
        public async Task<IEnumerable<SearchSuggestionDto>> GetSearchSuggestionsAsync(
            string partialKeyword, int maxResults = 5, CancellationToken cancellationToken = default)
        {
            if (!await ValidateSearchKeywordAsync(partialKeyword, cancellationToken) || partialKeyword.Length < 2)
            {
                return Enumerable.Empty<SearchSuggestionDto>();
            }

            try
            {
                var suggestions = new List<SearchSuggestionDto>();

                // Search for pallets using projections
                var palletSuggestionsQuery = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p =>
                        EF.Property<string>(p, "_palletNumberValue").Contains(partialKeyword) ||
                        p.ManufacturingOrder.Contains(partialKeyword))
                    .Select(p => new SearchSuggestionDto
                    {
                        Text = EF.Property<string>(p, "_palletNumberValue"),
                        Type = "Pallet",
                        Url = $"/Pallets/Details/{p.Id}",
                        EntityId = p.Id,
                        IsViewAll = false
                    });

                int palletsToInclude = maxResults > 0 ? Math.Min(maxResults / 2, 5) : 5;
                suggestions.AddRange(await palletSuggestionsQuery.Take(palletsToInclude).ToListAsync(cancellationToken));

                // Search for items using projections
                var remainingSuggestions = maxResults > 0 ? maxResults - suggestions.Count : 5;

                if (remainingSuggestions > 0)
                {
                    var itemSuggestionsQuery = DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                        .Where(i =>
                            i.ItemNumber.Contains(partialKeyword) ||
                            i.ManufacturingOrder.Contains(partialKeyword) ||
                            i.ClientCode.Contains(partialKeyword) ||
                            i.ClientName.Contains(partialKeyword))
                        .Select(i => new SearchSuggestionDto
                        {
                            Text = i.ItemNumber,
                            Type = "Item",
                            Url = $"/Items/Details/{i.Id}",
                            EntityId = i.Id,
                            IsViewAll = false
                        });

                    suggestions.AddRange(await itemSuggestionsQuery.Take(remainingSuggestions).ToListAsync(cancellationToken));
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
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<PalletDto>();
            }

            try
            {
                // Build query with projection
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p =>
                        EF.Property<string>(p, "_palletNumberValue").Contains(keyword) ||
                        p.ManufacturingOrder.Contains(keyword))
                    .ProjectToDto();

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    query = query.Take(maxResults);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching pallets for '{keyword}'");

                // Fallback to traditional approach
                var pallets = await _unitOfWork.PalletRepository.SearchAsync(keyword);
                var results = PalletMapper.ToDtoList(pallets);

                // Apply max results limit if specified
                if (maxResults > 0 && results.Count() > maxResults)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword, int maxResults = 0)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ItemDto>();
            }

            try
            {
                // Build query with projection
                var query = DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i =>
                        i.ItemNumber.Contains(keyword) ||
                        i.ManufacturingOrder.Contains(keyword) ||
                        i.ServiceOrder.Contains(keyword) ||
                        i.FinalOrder.Contains(keyword) ||
                        i.ClientCode.Contains(keyword) ||
                        i.ClientName.Contains(keyword) ||
                        i.Reference.Contains(keyword) ||
                        i.Batch.Contains(keyword))
                    .ProjectToDto();

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    query = query.Take(maxResults);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching items for '{keyword}'");

                // Fallback to traditional approach
                var items = await _unitOfWork.ItemRepository.SearchAsync(keyword);
                var results = ItemMapper.ToDtoList(items);

                // Apply max results limit if specified
                if (maxResults > 0 && results.Count() > maxResults)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> SearchManufacturingOrdersAsync(string keyword, int maxResults = 0)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<string>();
            }

            try
            {
                // Build query with projection
                var query = DbContextAccessor.CreateQuery<Pallet>(_unitOfWork)
                    .Where(p => p.ManufacturingOrder.Contains(keyword))
                    .Select(p => p.ManufacturingOrder)
                    .Distinct();

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    query = query.Take(maxResults);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching manufacturing orders for '{keyword}'");

                // Fallback to traditional approach
                var pallets = await _unitOfWork.PalletRepository.SearchAsync(keyword);
                var results = pallets
                    .Select(p => p.ManufacturingOrder)
                    .Distinct();

                // Apply max results limit if specified
                if (maxResults > 0 && results.Count() > maxResults)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ClientDto>> SearchClientsAsync(string keyword, int maxResults = 0)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ClientDto>();
            }

            try
            {
                // Build query with projection
                var query = DbContextAccessor.CreateQuery<Item>(_unitOfWork)
                    .Where(i =>
                        i.ClientCode.Contains(keyword) ||
                        i.ClientName.Contains(keyword))
                    .GroupBy(i => new { i.ClientCode, i.ClientName })
                    .Select(g => new ClientDto
                    {
                        ClientCode = g.Key.ClientCode,
                        ClientName = g.Key.ClientName,
                        IsSpecial = g.Any(i => i.ClientCode == "280898" && i.ClientName == "Special Client HB"),
                        ItemCount = g.Count()
                    })
                    .OrderBy(c => c.ClientName);

                // Apply max results limit if specified
                if (maxResults > 0)
                {
                    query = query.Take(maxResults);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching clients for '{keyword}'");

                // Fallback to traditional approach
                var items = await _unitOfWork.ItemRepository.SearchAsync(keyword);
                var results = items
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
                if (maxResults > 0 && results.Count() > maxResults)
                {
                    results = results.Take(maxResults);
                }

                return results;
            }
        }

        /// <inheritdoc/>
        public Task<bool> ValidateSearchKeywordAsync(string keyword)
        {
            return ValidateSearchKeywordAsync(keyword, CancellationToken.None);
        }

        /// <summary>
        /// Validates a search keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="cancellationToken">A token to cancel the operation</param>
        /// <returns>True if the keyword is valid, false otherwise</returns>
        public async Task<bool> ValidateSearchKeywordAsync(string keyword, CancellationToken cancellationToken = default)
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