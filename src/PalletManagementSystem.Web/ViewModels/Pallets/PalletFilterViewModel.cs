// src/PalletManagementSystem.Web/ViewModels/Pallets/PalletFilterViewModel.cs
using System.Collections.Generic;
using System.Web.Mvc;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Web.ViewModels.Pallets
{
    public class PalletFilterViewModel
    {
        public string Keyword { get; set; }
        public Division? Division { get; set; }
        public Platform? Platform { get; set; }
        public bool? IsClosed { get; set; }
        public string ManufacturingOrder { get; set; }
        public string SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;

        public List<SelectListItem> DivisionOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PlatformOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "All Pallets", Value = "" },
            new SelectListItem { Text = "Open Pallets", Value = "false" },
            new SelectListItem { Text = "Closed Pallets", Value = "true" }
        };
        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "Creation Date", Value = "CreatedDate" },
            new SelectListItem { Text = "Pallet Number", Value = "PalletNumber" },
            new SelectListItem { Text = "Manufacturing Order", Value = "ManufacturingOrder" }
        };
    }
}