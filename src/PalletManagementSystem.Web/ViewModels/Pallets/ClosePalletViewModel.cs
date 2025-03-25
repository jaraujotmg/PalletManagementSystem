// src/PalletManagementSystem.Web/ViewModels/Pallets/ClosePalletViewModel.cs
using PalletManagementSystem.Web.ViewModels.Shared;
using System.ComponentModel.DataAnnotations;

namespace PalletManagementSystem.Web.ViewModels.Pallets
{
    public class ClosePalletViewModel : ViewModelBase
    {
        public int PalletId { get; set; }
        public string PalletNumber { get; set; }
        public string ManufacturingOrder { get; set; }
        public int ItemCount { get; set; }

        [Display(Name = "Print Pallet List")]
        public bool PrintPalletList { get; set; } = true;

        [Display(Name = "Confirmation")]
        [Required(ErrorMessage = "Please type 'CLOSE' to confirm")]
        public string ConfirmationText { get; set; }

        public bool IsValid()
        {
            return ConfirmationText?.ToUpper() == "CLOSE";
        }
    }
}