using PalletManagementSystem.Core.DTOs;
using System;
using System.Collections.Generic;

namespace PalletManagementSystem.Web.ViewModels.ItemViewModels
{
    /// <summary>
    /// View model for displaying item details
    /// </summary>
    public class ItemDetailsViewModel
    {
        /// <summary>
        /// The item details
        /// </summary>
        public ItemDto Item { get; set; }

        /// <summary>
        /// The pallet the item belongs to
        /// </summary>
        public PalletDto Pallet { get; set; }

        /// <summary>
        /// Activity log entries for the item
        /// </summary>
        public List<ActivityLogEntry> ActivityLog { get; set; }

        /// <summary>
        /// Whether the item can be edited (only if pallet is not closed)
        /// </summary>
        public bool CanEdit => Pallet != null && !Pallet.IsClosed;

        /// <summary>
        /// Whether the item can be moved (only if pallet is not closed)
        /// </summary>
        public bool CanMove => Pallet != null && !Pallet.IsClosed;

        /// <summary>
        /// Creates a new instance of ItemDetailsViewModel
        /// </summary>
        public ItemDetailsViewModel()
        {
            ActivityLog = new List<ActivityLogEntry>();
        }

        /// <summary>
        /// Creates a view model from an ItemDto and its PalletDto
        /// </summary>
        /// <param name="item">The item DTO</param>
        /// <param name="pallet">The pallet DTO</param>
        /// <returns>Populated view model</returns>
        public static ItemDetailsViewModel FromItemAndPalletDto(ItemDto item, PalletDto pallet)
        {
            var viewModel = new ItemDetailsViewModel
            {
                Item = item,
                Pallet = pallet,
                ActivityLog = new List<ActivityLogEntry>()
            };

            // Add item creation activity
            viewModel.ActivityLog.Add(new ActivityLogEntry
            {
                Title = "Item created",
                Subtitle = $"Item #{item.ItemNumber} created on pallet {pallet.PalletNumber}",
                Timestamp = item.CreatedDate,
                Username = item.CreatedBy,
                Icon = "fa-plus",
                BadgeClass = "badge-primary"
            });

            // In a real application, we would add more activity log entries
            // from a database or service that tracks item history

            return viewModel;
        }

        /// <summary>
        /// Class for activity log entries
        /// </summary>
        public class ActivityLogEntry
        {
            /// <summary>
            /// Title of the activity
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Subtitle or description of the activity
            /// </summary>
            public string Subtitle { get; set; }

            /// <summary>
            /// When the activity occurred
            /// </summary>
            public DateTime Timestamp { get; set; }

            /// <summary>
            /// Username of who performed the activity
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// Font Awesome icon class for the activity
            /// </summary>
            public string Icon { get; set; }

            /// <summary>
            /// Bootstrap badge class for styling
            /// </summary>
            public string BadgeClass { get; set; }

            /// <summary>
            /// Formatted timestamp for display
            /// </summary>
            public string FormattedTimestamp => Timestamp.ToString("dd/MM/yyyy HH:mm");
        }
    }
}