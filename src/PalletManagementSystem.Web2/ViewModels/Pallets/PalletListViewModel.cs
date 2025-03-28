// src/PalletManagementSystem.Web/ViewModels/Pallets/PalletListViewModel.cs
using System.Collections.Generic;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Pallets
{
    public class PalletListViewModel : ViewModelBase
    {
        public PagedResultDto<PalletListDto> Pallets { get; set; }
        public string SearchKeyword { get; set; }
        public bool? IsClosed { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Division SelectedDivision { get; set; }
        public Platform SelectedPlatform { get; set; }
        public List<Platform> AvailablePlatforms { get; set; } = new List<Platform>();
        public bool CanCreatePallet { get; set; }
    }
}