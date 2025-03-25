// src/PalletManagementSystem.Web/Models/PalletListViewModel.cs
using System.Collections.Generic;
using PalletManagementSystem.Core.DTOs;

namespace PalletManagementSystem.Web.Models
{
    public class PalletListViewModel : ViewModelBase
    {
        public PagedResultDto<PalletListDto> Pallets { get; set; }
        public string SearchKeyword { get; set; }
        public bool? IsClosed { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}