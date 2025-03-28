// src/PalletManagementSystem.Web2/Models/HomeViewModel.cs
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Home
{
    public class HomeViewModel : ViewModelBase
    {
        public string LastLoginDate { get; set; }
        public string ApplicationVersion { get; set; }
        public string DatabaseVersion { get; set; }
        public string LastUpdateDate { get; set; }
        public string ServerName { get; set; }
        public bool AllServicesOperational { get; set; }
    }
}