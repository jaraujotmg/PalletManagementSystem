// src/PalletManagementSystem.Web/ViewModels/Settings/UserPreferencesViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Web.ViewModels.Shared;

namespace PalletManagementSystem.Web.ViewModels.Settings
{
    public class UserPreferencesViewModel : ViewModelBase
    {
        // Division and Platform Settings
        [Required(ErrorMessage = "Division is required")]
        [Display(Name = "Division")]
        public Division PreferredDivision { get; set; }

        [Required(ErrorMessage = "Platform is required")]
        [Display(Name = "Platform")]
        public Platform PreferredPlatform { get; set; }

        // Display and Interface Settings
        [Required(ErrorMessage = "Items per page is required")]
        [Range(10, 100, ErrorMessage = "Items per page must be between 10 and 100")]
        [Display(Name = "Items per page")]
        public int ItemsPerPage { get; set; }

        [Required(ErrorMessage = "Default view is required")]
        [Display(Name = "Default view")]
        public string DefaultPalletView { get; set; }

        [Display(Name = "Enable touch mode by default")]
        public bool TouchModeEnabled { get; set; }

        [Display(Name = "Show on-screen keyboard")]
        public bool TouchKeyboardEnabled { get; set; }

        [Display(Name = "Use larger buttons and inputs")]
        public bool LargeButtonsEnabled { get; set; }

        [Required(ErrorMessage = "Button size is required")]
        [Display(Name = "Button size")]
        public string ButtonSize { get; set; }

        [Display(Name = "Show confirmation prompts")]
        public bool ShowConfirmationPrompts { get; set; }

        // Printer Settings
        [Required(ErrorMessage = "Pallet list printer is required")]
        [Display(Name = "Pallet List Printer")]
        public string DefaultPalletListPrinter { get; set; }

        [Required(ErrorMessage = "Item label printer is required")]
        [Display(Name = "Item Label Printer")]
        public string DefaultItemLabelPrinter { get; set; }

        [Display(Name = "Auto-print pallet list when closing")]
        public bool AutoPrintPalletList { get; set; }

        [Display(Name = "Use special printer for special clients")]
        public bool UseSpecialPrinterForSpecialClients { get; set; }

        // Session Settings
        [Required(ErrorMessage = "Session timeout is required")]
        [Range(15, 120, ErrorMessage = "Session timeout must be between 15 and 120 minutes")]
        [Display(Name = "Session timeout (minutes)")]
        public int SessionTimeoutMinutes { get; set; }

        [Display(Name = "Remember division and platform")]
        public bool RememberDivisionAndPlatform { get; set; }

        [Display(Name = "Auto-refresh pallet list")]
        public bool AutoRefreshPalletList { get; set; }

        [Required(ErrorMessage = "Auto-refresh interval is required")]
        [Range(30, 600, ErrorMessage = "Auto-refresh interval must be between 30 and 600 seconds")]
        [Display(Name = "Auto-refresh interval (seconds)")]
        public int AutoRefreshIntervalSeconds { get; set; }

        [Display(Name = "Show browser notifications")]
        public bool ShowBrowserNotifications { get; set; }

        // Dropdown options
        public List<SelectListItem> DivisionOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PlatformOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DefaultViewOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ButtonSizeOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> SessionTimeoutOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RefreshIntervalOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PalletListPrinterOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ItemLabelPrinterOptions { get; set; } = new List<SelectListItem>();

        // System Information
        public string ApplicationVersion { get; set; }
        public string DatabaseVersion { get; set; }
        public string LastUpdateDate { get; set; }
        public string ServerName { get; set; }
        public bool AllServicesOperational { get; set; }
    }
}