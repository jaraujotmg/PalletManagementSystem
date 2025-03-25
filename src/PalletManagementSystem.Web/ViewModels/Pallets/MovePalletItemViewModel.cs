// src/PalletManagementSystem.Web/ViewModels/Pallets/MovePalletItemViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.ViewModels.Pallets
{
    public class MovePalletItemViewModel : ViewModelBase
    {
        public int ItemId { get; set; }
        public string ItemNumber { get; set; }
        public ItemDetailDto Item { get; set; }

        [Required(ErrorMessage = "Target pallet is required")]
        [Display(Name = "Target Pallet")]
        public int TargetPalletId { get; set; }

        public int SourcePalletId { get; set; }
        public string SourcePalletNumber { get; set; }

        public List<PalletListDto> AvailablePallets { get; set; } = new List<PalletListDto>();
        public string SearchKeyword { get; set; }

        public bool CreateNewPallet { get; set; }

        [Display(Name = "Manufacturing Order")]
        public string NewPalletManufacturingOrder { get; set; }

        [Display(Name = "Unit of Measure")]
        public string NewPalletUnitOfMeasure { get; set; }

        [Display(Name = "Move immediately")]
        public bool MoveToNewPallet { get; set; } = true;
    }
}