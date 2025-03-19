using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PalletManagementSystem.Web.ViewModels.PalletViewModels
{
    /// <summary>
    /// View model for displaying a list of pallets with filtering and pagination
    /// </summary>
    public class PalletListViewModel
    {
        /// <summary>
        /// The list of pallets to display
        /// </summary>
        public IEnumerable<PalletDto> Pallets { get; set; }

        /// <summary>
        /// Current page number (for pagination)
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Total number of pages (for pagination)
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Items per page
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Current filter status (all, open, closed)
        /// </summary>
        public string Status { get; set; } = "all";

        /// <summary>
        /// Current search term (if any)
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Current division filter
        /// </summary>
        public Division Division { get; set; }

        /// <summary>
        /// Current platform filter
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        /// Indicates if there are previous pages
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Indicates if there are next pages
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Gets count of open pallets
        /// </summary>
        public int OpenPalletsCount => Pallets?.Count(p => !p.IsClosed) ?? 0;

        /// <summary>
        /// Gets count of closed pallets
        /// </summary>
        public int ClosedPalletsCount => Pallets?.Count(p => p.IsClosed) ?? 0;

        /// <summary>
        /// Gets count of total pallets
        /// </summary>
        public int TotalPalletsCount => Pallets?.Count() ?? 0;

        /// <summary>
        /// Creates a new instance of PalletListViewModel
        /// </summary>
        public PalletListViewModel()
        {
            Pallets = new List<PalletDto>();
        }

        /// <summary>
        /// Creates the view model from a collection of PalletDto objects
        /// </summary>
        /// <param name="pallets">Collection of PalletDto objects</param>
        /// <param name="currentPage">Current page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="status">Status filter</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="division">Division filter</param>
        /// <param name="platform">Platform filter</param>
        /// <returns>Populated view model</returns>
        public static PalletListViewModel FromPalletDtos(
            IEnumerable<PalletDto> pallets,
            int currentPage,
            int pageSize,
            string status = "all",
            string searchTerm = null,
            Division division = Division.MA,
            Platform platform = Platform.TEC1)
        {
            var filteredPallets = pallets;

            // Apply status filter if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "open")
                {
                    filteredPallets = filteredPallets.Where(p => !p.IsClosed);
                }
                else if (status == "closed")
                {
                    filteredPallets = filteredPallets.Where(p => p.IsClosed);
                }
            }

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchLower = searchTerm.ToLower();
                filteredPallets = filteredPallets.Where(p =>
                    p.PalletNumber.ToLower().Contains(searchLower) ||
                    p.ManufacturingOrder.ToLower().Contains(searchLower));
            }

            // Calculate total number of pages
            int totalItems = filteredPallets.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Ensure current page is valid
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages > 0 ? totalPages : 1));

            // Apply pagination
            var paginatedPallets = filteredPallets
                .OrderByDescending(p => p.CreatedDate)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            return new PalletListViewModel
            {
                Pallets = paginatedPallets,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                PageSize = pageSize,
                Status = status,
                SearchTerm = searchTerm,
                Division = division,
                Platform = platform
            };
        }
    }
}