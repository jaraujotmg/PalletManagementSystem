// src/PalletManagementSystem.Web/ViewModels/Items/ItemDetailViewModel.cs
using System.ComponentModel.DataAnnotations;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.ViewModels.Items
{
    public class ItemDetailViewModel : ViewModelBase
    {
        public ItemDetailDto Item { get; set; }
        public bool CanEdit { get; set; }
        public bool CanMove { get; set; }
        public bool CanPrint { get; set; }

        // Grouped information sections for UI organization
        public bool HasManufacturingOrder => !string.IsNullOrEmpty(Item?.ManufacturingOrder);
        public bool HasServiceOrder => !string.IsNullOrEmpty(Item?.ServiceOrder);
        public bool HasFinalOrder => !string.IsNullOrEmpty(Item?.FinalOrder);

        // Determine if the item is on a closed pallet
        public bool IsOnClosedPallet => Item?.Pallet?.IsClosed ?? false;

        // Return URL for back button
        public string ReturnUrl { get; set; }
    }
}