// src/PalletManagementSystem.Web/ViewModels/Pallets/CreatePalletViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.ViewModels.Pallets
{
    public class CreatePalletViewModel : ViewModelBase
    {
        [Required(ErrorMessage = "Manufacturing Order is required")]
        [Display(Name = "Manufacturing Order")]
        public string ManufacturingOrder { get; set; }

        [Required(ErrorMessage = "Division is required")]
        [Display(Name = "Division")]
        public Division Division { get; set; }

        [Required(ErrorMessage = "Platform is required")]
        [Display(Name = "Platform")]
        public Platform Platform { get; set; }

        [Required(ErrorMessage = "Unit of Measure is required")]
        [Display(Name = "Unit of Measure")]
        public UnitOfMeasure UnitOfMeasure { get; set; }

        public List<SelectListItem> DivisionOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PlatformOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> UnitOfMeasureOptions { get; set; } = new List<SelectListItem>();
    }
}