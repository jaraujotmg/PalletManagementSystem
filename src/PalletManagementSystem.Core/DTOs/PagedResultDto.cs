using System.Collections.Generic;

namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// Data transfer object for a paged result
    /// </summary>
    /// <typeparam name="T">The type of items in the page</typeparam>
    public class PagedResultDto<T>
    {
        /// <summary>
        /// Gets or sets the items in the current page
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Gets or sets the total count of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of pages
        /// </summary>
        public int TotalPages => (PageSize <= 0) ? 0 : (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Gets a value indicating whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}