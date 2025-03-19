using PalletManagementSystem.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service for searching entities in the system
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Performs a general search across all entities
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited)</param>
        /// <returns>Collection of search results</returns>
        Task<IEnumerable<SearchResultDto>> SearchAsync(string keyword, int maxResults = 0);

        /// <summary>
        /// Gets search suggestions as the user types
        /// </summary>
        /// <param name="partialKeyword">The partial search keyword</param>
        /// <param name="maxResults">Maximum number of suggestions to return</param>
        /// <returns>Collection of search suggestions</returns>
        Task<IEnumerable<SearchSuggestionDto>> GetSearchSuggestionsAsync(string partialKeyword, int maxResults = 5);

        /// <summary>
        /// Searches for pallets matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited)</param>
        /// <returns>Collection of pallets matching the search</returns>
        Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword, int maxResults = 0);

        /// <summary>
        /// Searches for items matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited)</param>
        /// <returns>Collection of items matching the search</returns>
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword, int maxResults = 0);

        /// <summary>
        /// Searches for manufacturing orders matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited)</param>
        /// <returns>Collection of manufacturing orders matching the search</returns>
        Task<IEnumerable<string>> SearchManufacturingOrdersAsync(string keyword, int maxResults = 0);

        /// <summary>
        /// Searches for clients matching the keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <param name="maxResults">Maximum number of results to return (0 for unlimited)</param>
        /// <returns>Collection of clients matching the search</returns>
        Task<IEnumerable<ClientDto>> SearchClientsAsync(string keyword, int maxResults = 0);

        /// <summary>
        /// Validates a search keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>True if the keyword is valid, false otherwise</returns>
        Task<bool> ValidateSearchKeywordAsync(string keyword);
    }
}