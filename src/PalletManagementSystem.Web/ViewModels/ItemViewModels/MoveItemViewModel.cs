using PalletManagementSystem.Core.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PalletManagementSystem.Web.ViewModels.ItemViewModels
{
    /// <summary>
    /// View model for moving an item to another pallet
    /// </summary>
    public class MoveItemViewModel
    {
        /// <summary>
        /// The ID of the item to move
        /// </summary>
        [Required]
        public int ItemId { get; set; }

        /// <summary>
        /// The item number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// The ID of the source pallet
        /// </summary>
        public int SourcePalletId { get; set; }

        /// <summary>
        /// The source pallet number
        /// </summary>
        public string SourcePalletNumber { get; set; }

        /// <summary>
        /// The ID of the target pallet
        /// </summary>
        [Required(ErrorMessage = "You must select a target pallet")]
        [Display(Name = "Target Pallet")]
        public int TargetPalletId { get; set; }

        /// <summary>
        /// The list of available target pallets
        /// </summary>
        public List<PalletListItem> AvailablePallets { get; set; }

        /// <summary>
        /// Item reference information
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Item client information
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Item manufacturing order information
        /// </summary>
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Item quantity information
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Item quantity unit
        /// </summary>
        public string QuantityUnit { get; set; }

        /// <summary>
        /// Item weight information
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Item weight unit
        /// </summary>
        public string WeightUnit { get; set; }

        /// <summary>
        /// Whether to create a new pallet
        /// </summary>
        public bool CreateNewPallet { get; set; }

        /// <summary>
        /// New pallet manufacturing order (only used if CreateNewPallet is true)
        /// </summary>
        [Display(Name = "Manufacturing Order")]
        public string NewPalletManufacturingOrder { get; set; }

        /// <summary>
        /// Creates a new instance of MoveItemViewModel
        /// </summary>
        public MoveItemViewModel()
        {
            AvailablePallets = new List<PalletListItem>();
        }

        /// <summary>
        /// Creates a view model from an ItemDto and a list of available PalletDto objects
        /// </summary>
        /// <param name="item">The item to move</param>
        /// <param name="sourcePallet">The source pallet</param>
        /// <param name="availablePallets">List of available target pallets</param>
        /// <returns>Populated view model</returns>
        public static MoveItemViewModel FromItemAndPallets(
            ItemDto item,
            PalletDto sourcePallet,
            IEnumerable<PalletDto> availablePallets)
        {
            var viewModel = new MoveItemViewModel
            {
                ItemId = item.Id,
                ItemNumber = item.ItemNumber,
                SourcePalletId = sourcePallet.Id,
                SourcePalletNumber = sourcePallet.PalletNumber,
                Reference = item.Reference,
                ClientName = item.ClientName,
                ManufacturingOrder = item.ManufacturingOrder,
                Quantity = item.Quantity,
                QuantityUnit = item.QuantityUnit,
                Weight = item.Weight,
                WeightUnit = item.WeightUnit
            };

            // Convert available pallets to list items
            viewModel.AvailablePallets = availablePallets
                .Where(p => p.Id != sourcePallet.Id && !p.IsClosed)
                .Select(p => new PalletListItem
                {
                    Id = p.Id,
                    PalletNumber = p.PalletNumber,
                    ManufacturingOrder = p.ManufacturingOrder,
                    ItemCount = p.ItemCount,
                    Quantity = p.Quantity,
                    CreatedDate = p.CreatedDate.ToString("dd/MM/yyyy")
                })
                .ToList();

            return viewModel;
        }

        /// <summary>
        /// Class representing a list item for available pallets
        /// </summary>
        public class PalletListItem
        {
            /// <summary>
            /// The ID of the pallet
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The pallet number
            /// </summary>
            public string PalletNumber { get; set; }

            /// <summary>
            /// Manufacturing order associated with the pallet
            /// </summary>
            public string ManufacturingOrder { get; set; }

            /// <summary>
            /// Number of items on the pallet
            /// </summary>
            public int ItemCount { get; set; }

            /// <summary>
            /// Total quantity on the pallet
            /// </summary>
            public double Quantity { get; set; }

            /// <summary>
            /// Date when the pallet was created
            /// </summary>
            public string CreatedDate { get; set; }

            /// <summary>
            /// Display text for the pallet in dropdowns
            /// </summary>
            public string DisplayText => $"{PalletNumber} (MO: {ManufacturingOrder}, Items: {ItemCount})";
        }
    }
}