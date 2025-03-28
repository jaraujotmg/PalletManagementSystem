// src/PalletManagementSystem.Web/ViewModels/Pallets/SearchResultsViewModel.cs
using System.Collections.Generic;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Pallets
{
    public class SearchResultsViewModel : ViewModelBase
    {
        public string Keyword { get; set; }

        public List<PalletListDto> PalletResults { get; set; } = new List<PalletListDto>();
        public List<ItemListDto> ItemResults { get; set; } = new List<ItemListDto>();
        public List<string> ManufacturingOrderResults { get; set; } = new List<string>();
        public List<ClientDto> ClientResults { get; set; } = new List<ClientDto>();

        public int TotalResults =>
            PalletResults.Count +
            ItemResults.Count +
            ManufacturingOrderResults.Count +
            ClientResults.Count;
    }
}