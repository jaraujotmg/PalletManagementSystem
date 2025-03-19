using PalletManagementSystem.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PalletManagementSystem.Web.ViewModels.PalletViewModels
{
    /// <summary>
    /// View model for displaying pallet details
    /// </summary>
    public class PalletDetailsViewModel
    {
        /// <summary>
        /// The pallet details
        /// </summary>
        public PalletDto Pallet { get; set; }

        /// <summary>
        /// Activity log entries for the pallet (ordered by most recent first)
        /// </summary>
        public List<ActivityLogEntry> ActivityLog { get; set; }

        /// <summary>
        /// Gets a grouping of items by client
        /// </summary>
        public IEnumerable<ClientItemSummary> ItemsByClient =>
            Pallet?.Items
                ?.GroupBy(i => i.ClientName)
                .Select(g => new ClientItemSummary
                {
                    ClientName = g.Key,
                    ItemCount = g.Count(),
                    TotalQuantity = g.Sum(i => i.Quantity)
                }) ?? new List<ClientItemSummary>();

        /// <summary>
        /// Indicates if the pallet can be edited (not closed)
        /// </summary>
        public bool CanEdit => Pallet != null && !Pallet.IsClosed;

        /// <summary>
        /// Total weight of all items in the pallet
        /// </summary>
        public double TotalWeight => Pallet?.Items?.Sum(i => i.Weight) ?? 0;

        /// <summary>
        /// Creates a new instance of PalletDetailsViewModel
        /// </summary>
        public PalletDetailsViewModel()
        {
            ActivityLog = new List<ActivityLogEntry>();
        }

        /// <summary>
        /// Creates a view model from a PalletDto
        /// </summary>
        /// <param name="pallet">The pallet DTO</param>
        /// <returns>Populated view model</returns>
        public static PalletDetailsViewModel FromPalletDto(PalletDto pallet)
        {
            var viewModel = new PalletDetailsViewModel
            {
                Pallet = pallet,
                ActivityLog = new List<ActivityLogEntry>()
            };

            // Add pallet creation activity
            viewModel.ActivityLog.Add(new ActivityLogEntry
            {
                Title = "Pallet created",
                Subtitle = $"Pallet {pallet.PalletNumber} created",
                Timestamp = pallet.CreatedDate,
                Username = pallet.CreatedBy,
                Icon = "fa-pallet",
                BadgeClass = "badge-primary"
            });

            // Add pallet closed activity if applicable
            if (pallet.IsClosed && pallet.ClosedDate.HasValue)
            {
                viewModel.ActivityLog.Add(new ActivityLogEntry
                {
                    Title = "Pallet closed",
                    Subtitle = $"Pallet {pallet.PalletNumber} closed",
                    Timestamp = pallet.ClosedDate.Value,
                    Username = pallet.CreatedBy, // In a real app, we might track who closed it
                    Icon = "fa-lock",
                    BadgeClass = "badge-success"
                });
            }

            // Add item activities
            if (pallet.Items != null && pallet.Items.Any())
            {
                foreach (var item in pallet.Items.OrderByDescending(i => i.CreatedDate).Take(5))
                {
                    viewModel.ActivityLog.Add(new ActivityLogEntry
                    {
                        Title = "Item added",
                        Subtitle = $"Item #{item.ItemNumber} added to pallet",
                        Timestamp = item.CreatedDate,
                        Username = item.CreatedBy,
                        Icon = "fa-plus",
                        BadgeClass = "badge-primary"
                    });
                }
            }

            // Sort activity log by timestamp descending (most recent first)
            viewModel.ActivityLog = viewModel.ActivityLog
                .OrderByDescending(a => a.Timestamp)
                .ToList();

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

        /// <summary>
        /// Class for client item summary
        /// </summary>
        public class ClientItemSummary
        {
            /// <summary>
            /// Client name
            /// </summary>
            public string ClientName { get; set; }

            /// <summary>
            /// Count of items for this client
            /// </summary>
            public int ItemCount { get; set; }

            /// <summary>
            /// Total quantity of items for this client
            /// </summary>
            public double TotalQuantity { get; set; }
        }
    }
}