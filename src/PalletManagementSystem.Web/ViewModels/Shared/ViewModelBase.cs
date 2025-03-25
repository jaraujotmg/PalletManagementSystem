// src/PalletManagementSystem.Web/Models/ViewModelBase.cs

// src/PalletManagementSystem.Web/Models/ViewModelBase.cs

// src/PalletManagementSystem.Web/Models/ViewModelBase.cs

// src/PalletManagementSystem.Web/Models/ViewModelBase.cs
using PalletManagementSystem.Core.Models.Enums;

namespace PalletManagementSystem.Web.ViewModels.Shared
{
    public class ViewModelBase
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public bool CanEdit { get; set; }
        public Division CurrentDivision { get; set; }
        public Platform CurrentPlatform { get; set; }
        public bool TouchModeEnabled { get; set; }
    }
}