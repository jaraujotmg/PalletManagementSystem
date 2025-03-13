using System.Collections.Generic;
using System.Threading.Tasks;
using PalletManagementSystem.Core.DTOs;

namespace PalletManagementSystem.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for search operations
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Searches for pallets and items by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of search result DTOs</returns>
        Task<IEnumerable<SearchResultDto>> SearchAsync(string keyword);

        /// <summary>
        /// Gets search suggestions as the user types
        /// </summary>
        /// <param name="partialKeyword">The partial keyword</param>
        /// <param name="maxResults">The maximum number of results to return</param>
        /// <returns>A collection of search suggestion DTOs</returns>
        Task<IEnumerable<SearchSuggestionDto>> GetSearchSuggestionsAsync(string partialKeyword, int maxResults = 5);

        /// <summary>
        /// Searches for pallets by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of pallet DTOs</returns>
        Task<IEnumerable<PalletDto>> SearchPalletsAsync(string keyword);

        /// <summary>
        /// Searches for items by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of item DTOs</returns>
        Task<IEnumerable<ItemDto>> SearchItemsAsync(string keyword);

        /// <summary>
        /// Searches for manufacturing orders by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of manufacturing order strings</returns>
        Task<IEnumerable<string>> SearchManufacturingOrdersAsync(string keyword);

        /// <summary>
        /// Searches for clients by keyword
        /// </summary>
        /// <param name="keyword">The search keyword</param>
        /// <returns>A collection of client DTOs</returns>
        Task<IEnumerable<ClientDto>> SearchClientsAsync(string keyword);
    }
}