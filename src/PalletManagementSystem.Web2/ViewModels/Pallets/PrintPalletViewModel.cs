// src/PalletManagementSystem.Web/ViewModels/Pallets/PrintPalletViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Pallets
{
    public class PrintPalletViewModel : ViewModelBase
    {
        public int PalletId { get; set; }
        public PalletDetailDto Pallet { get; set; }

        [Required(ErrorMessage = "Printer is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; }

        public List<SelectListItem> AvailablePrinters { get; set; } = new List<SelectListItem>();

        [Display(Name = "Save as default printer")]
        public bool SaveAsDefault { get; set; } = false;

        [Display(Name = "Print preview")]
        public bool ShowPreview { get; set; } = true;
    }
}