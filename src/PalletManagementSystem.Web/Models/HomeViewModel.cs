// src/PalletManagementSystem.Web/Models/HomeViewModel.cs
namespace PalletManagementSystem.Web.Models
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