// src/PalletManagementSystem.Web/ViewModels/Shared/DivisionPlatformSelectorViewModel.cs
using System.Collections.Generic;
using System.Web.Mvc;
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Web.ViewModels.Shared
{
    public class DivisionPlatformSelectorViewModel
    {
        public Division CurrentDivision { get; set; }
        public Platform CurrentPlatform { get; set; }

        public List<SelectListItem> Divisions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Platforms { get; set; } = new List<SelectListItem>();

        public string ReturnUrl { get; set; }
    }
}