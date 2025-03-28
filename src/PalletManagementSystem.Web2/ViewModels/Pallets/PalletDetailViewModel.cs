// src/PalletManagementSystem.Web/ViewModels/Pallets/PalletDetailViewModel.cs
using System;
using System.Collections.Generic;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Pallets
{
    public class PalletDetailViewModel : ViewModelBase
    {
        public PalletDetailDto Pallet { get; set; }
        public bool CanClose { get; set; }
        public bool CanEdit { get; set; }
        public bool CanPrint { get; set; }
        public Dictionary<string, int> ClientSummary { get; set; } = new Dictionary<string, int>();
        public List<ActivityLogItem> ActivityLogs { get; set; } = new List<ActivityLogItem>();
    }

    public class ActivityLogItem
    {
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; }
        public string BadgeClass { get; set; }
        public string IconClass { get; set; }
    }
}