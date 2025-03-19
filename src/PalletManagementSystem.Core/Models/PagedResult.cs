using System;
using System.Collections.Generic;

namespace PalletManagementSystem.Core.Models
{
    /// <summary>
    /// Represents a page of results from a repository query
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets the items in the current page
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// Gets the total count of items across all pages
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets the current page number (1-based)
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Gets the page size
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Gets a value indicating whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}"/> class
        /// </summary>
        /// <param name="items">The items in the current page</param>
        /// <param name="totalCount">The total count of items across all pages</param>
        /// <param name="pageNumber">The current page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}