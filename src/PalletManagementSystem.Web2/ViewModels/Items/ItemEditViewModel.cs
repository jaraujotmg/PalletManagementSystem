// src/PalletManagementSystem.Web/ViewModels/Items/ItemEditViewModel.cs
using System.ComponentModel.DataAnnotations;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Items
{
    public class ItemEditViewModel : ViewModelBase
    {
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public string PalletNumber { get; set; }
        public int PalletId { get; set; }
        public bool IsPalletClosed { get; set; }

        // Read-only display information
        public string ManufacturingOrder { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string Reference { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityUnit { get; set; }

        // Editable fields
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Weight must be greater than zero")]
        [Display(Name = "Weight")]
        public decimal Weight { get; set; }

        [Display(Name = "Weight Unit")]
        public string WeightUnit { get; set; }

        [Required(ErrorMessage = "Width is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Width must be greater than zero")]
        [Display(Name = "Width")]
        public decimal Width { get; set; }

        [Display(Name = "Width Unit")]
        public string WidthUnit { get; set; }

        [Required(ErrorMessage = "Quality is required")]
        [Display(Name = "Quality")]
        public string Quality { get; set; }

        [Required(ErrorMessage = "Batch is required")]
        [Display(Name = "Batch")]
        public string Batch { get; set; }

        // Form control options
        public bool EnableTouchMode { get; set; }

        // Return URL for cancel button
        public string ReturnUrl { get; set; }

        // Populate editable fields from an item DTO
        public void PopulateFromDto(ItemDetailDto itemDto)
        {
            if (itemDto == null) return;

            ItemId = itemDto.Id;
            ItemNumber = itemDto.ItemNumber;
            PalletNumber = itemDto.Pallet?.PalletNumber;
            PalletId = itemDto.PalletId;
            IsPalletClosed = itemDto.Pallet?.IsClosed ?? false;

            ManufacturingOrder = itemDto.ManufacturingOrder;
            ClientCode = itemDto.ClientCode;
            ClientName = itemDto.ClientName;
            Reference = itemDto.Reference;
            Quantity = itemDto.Quantity;
            QuantityUnit = itemDto.QuantityUnit;

            Weight = itemDto.Weight;
            WeightUnit = itemDto.WeightUnit;
            Width = itemDto.Width;
            WidthUnit = itemDto.WidthUnit;
            Quality = itemDto.Quality;
            Batch = itemDto.Batch;
        }
    }
}