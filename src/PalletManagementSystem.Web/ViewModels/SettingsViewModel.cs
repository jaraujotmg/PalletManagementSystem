using PalletManagementSystem.Core.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.ViewModels
{
    /// <summary>
    /// View model for user settings
    /// </summary>
    public class SettingsViewModel
    {
        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; }

        #region Division and Platform Settings

        /// <summary>
        /// Preferred division
        /// </summary>
        [Required(ErrorMessage = "Division is required")]
        [Display(Name = "Preferred Division")]
        public string PreferredDivision { get; set; }

        /// <summary>
        /// Preferred platform
        /// </summary>
        [Required(ErrorMessage = "Platform is required")]
        [Display(Name = "Preferred Platform")]
        public string PreferredPlatform { get; set; }

        /// <summary>
        /// Remember division and platform preference
        /// </summary>
        [Display(Name = "Remember my division and platform preference")]
        public bool RememberDivisionPlatform { get; set; } = true;

        /// <summary>
        /// Available divisions for the dropdown
        /// </summary>
        public List<SelectListItem> AvailableDivisions { get; set; }

        /// <summary>
        /// Available platforms for the selected division
        /// </summary>
        public List<SelectListItem> AvailablePlatforms { get; set; }

        #endregion

        #region Display Settings

        /// <summary>
        /// Items per page for lists
        /// </summary>
        [Required]
        [Range(10, 100, ErrorMessage = "Items per page must be between 10 and 100")]
        [Display(Name = "Items per page")]
        public int ItemsPerPage { get; set; } = 20;

        /// <summary>
        /// Default view for pallet list (all, open, closed)
        /// </summary>
        [Required]
        [Display(Name = "Default pallet view")]
        public string DefaultView { get; set; } = "all";

        /// <summary>
        /// Show confirmation prompts before major actions
        /// </summary>
        [Display(Name = "Show confirmation prompts before major actions")]
        public bool ShowConfirmationPrompts { get; set; } = true;

        /// <summary>
        /// Auto-refresh pallet list
        /// </summary>
        [Display(Name = "Auto-refresh pallet list")]
        public bool AutoRefreshPalletList { get; set; }

        /// <summary>
        /// Auto-refresh interval in seconds
        /// </summary>
        [Range(30, 600, ErrorMessage = "Refresh interval must be between 30 and 600 seconds")]
        [Display(Name = "Auto-refresh interval (seconds)")]
        public int RefreshInterval { get; set; } = 60;

        /// <summary>
        /// Available items per page options
        /// </summary>
        public List<SelectListItem> ItemsPerPageOptions { get; set; }

        /// <summary>
        /// Available default view options
        /// </summary>
        public List<SelectListItem> DefaultViewOptions { get; set; }

        /// <summary>
        /// Available refresh interval options
        /// </summary>
        public List<SelectListItem> RefreshIntervalOptions { get; set; }

        #endregion

        #region Touch Mode Settings

        /// <summary>
        /// Enable touch mode by default
        /// </summary>
        [Display(Name = "Enable touch mode by default")]
        public bool TouchModeEnabled { get; set; }

        /// <summary>
        /// Show on-screen keyboard in touch mode
        /// </summary>
        [Display(Name = "Show on-screen keyboard in touch mode")]
        public bool ShowTouchKeyboard { get; set; } = true;

        /// <summary>
        /// Use larger buttons and inputs in touch mode
        /// </summary>
        [Display(Name = "Use larger buttons and inputs")]
        public bool UseLargeButtons { get; set; } = true;

        /// <summary>
        /// Button size in touch mode
        /// </summary>
        [Display(Name = "Button size")]
        public string ButtonSize { get; set; } = "large";

        /// <summary>
        /// Available button size options
        /// </summary>
        public List<SelectListItem> ButtonSizeOptions { get; set; }

        #endregion

        #region Printer Settings

        /// <summary>
        /// Default pallet list printer
        /// </summary>
        [Required(ErrorMessage = "Default pallet list printer is required")]
        [Display(Name = "Default pallet list printer")]
        public string DefaultPalletListPrinter { get; set; }

        /// <summary>
        /// Default item label printer
        /// </summary>
        [Required(ErrorMessage = "Default item label printer is required")]
        [Display(Name = "Default item label printer")]
        public string DefaultItemLabelPrinter { get; set; }

        /// <summary>
        /// Automatically print pallet list when closing a pallet
        /// </summary>
        [Display(Name = "Automatically print pallet list when closing a pallet")]
        public bool AutoPrintPalletList { get; set; } = true;

        /// <summary>
        /// Use special printer settings for special client
        /// </summary>
        [Display(Name = "Use special printer settings for Special Client HB")]
        public bool UseSpecialClientSettings { get; set; }

        /// <summary>
        /// Available pallet list printers
        /// </summary>
        public List<SelectListItem> PalletListPrinters { get; set; }

        /// <summary>
        /// Available item label printers
        /// </summary>
        public List<SelectListItem> ItemLabelPrinters { get; set; }

        #endregion

        #region Session Settings

        /// <summary>
        /// Session timeout in minutes
        /// </summary>
        [Required]
        [Range(15, 120, ErrorMessage = "Session timeout must be between 15 and 120 minutes")]
        [Display(Name = "Session timeout (minutes)")]
        public int SessionTimeout { get; set; } = 30;

        /// <summary>
        /// Show browser notifications
        /// </summary>
        [Display(Name = "Show browser notifications")]
        public bool ShowNotifications { get; set; } = true;

        /// <summary>
        /// Session timeout options
        /// </summary>
        public List<SelectListItem> SessionTimeoutOptions { get; set; }

        #endregion

        /// <summary>
        /// Creates a new instance of SettingsViewModel
        /// </summary>
        public SettingsViewModel()
        {
            InitializeSelectListOptions();
        }

        /// <summary>
        /// Creates a new instance of SettingsViewModel with specified values
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="preferredDivision">The preferred division</param>
        /// <param name="preferredPlatform">The preferred platform</param>
        public SettingsViewModel(string username, Division preferredDivision, Platform preferredPlatform)
        {
            Username = username;
            PreferredDivision = preferredDivision.ToString();
            PreferredPlatform = preferredPlatform.ToString();

            InitializeSelectListOptions();
        }

        /// <summary>
        /// Initializes all select list options for dropdowns
        /// </summary>
        private void InitializeSelectListOptions()
        {
            // Division options
            AvailableDivisions = new List<SelectListItem>
            {
                new SelectListItem { Value = "MA", Text = "MA - Manufacturing", Selected = PreferredDivision == "MA" },
                new SelectListItem { Value = "TC", Text = "TC - Technical Center", Selected = PreferredDivision == "TC" }
            };

            // Platform options based on selected division
            AvailablePlatforms = GetPlatformsForDivision(PreferredDivision);

            // Items per page options
            ItemsPerPageOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "10", Text = "10", Selected = ItemsPerPage == 10 },
                new SelectListItem { Value = "20", Text = "20", Selected = ItemsPerPage == 20 },
                new SelectListItem { Value = "50", Text = "50", Selected = ItemsPerPage == 50 },
                new SelectListItem { Value = "100", Text = "100", Selected = ItemsPerPage == 100 }
            };

            // Default view options
            DefaultViewOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "All pallets", Selected = DefaultView == "all" },
                new SelectListItem { Value = "open", Text = "Open pallets only", Selected = DefaultView == "open" },
                new SelectListItem { Value = "closed", Text = "Closed pallets only", Selected = DefaultView == "closed" }
            };

            // Auto-refresh interval options
            RefreshIntervalOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "30", Text = "30 seconds", Selected = RefreshInterval == 30 },
                new SelectListItem { Value = "60", Text = "1 minute", Selected = RefreshInterval == 60 },
                new SelectListItem { Value = "300", Text = "5 minutes", Selected = RefreshInterval == 300 },
                new SelectListItem { Value = "600", Text = "10 minutes", Selected = RefreshInterval == 600 }
            };

            // Button size options
            ButtonSizeOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "normal", Text = "Normal", Selected = ButtonSize == "normal" },
                new SelectListItem { Value = "large", Text = "Large", Selected = ButtonSize == "large" },
                new SelectListItem { Value = "extra-large", Text = "Extra Large", Selected = ButtonSize == "extra-large" }
            };

            // Pallet list printers
            PalletListPrinters = new List<SelectListItem>
            {
                new SelectListItem { Value = "HP LaserJet 4200 - Office", Text = "HP LaserJet 4200 - Office", Selected = DefaultPalletListPrinter == "HP LaserJet 4200 - Office" },
                new SelectListItem { Value = "Xerox WorkCentre - Production", Text = "Xerox WorkCentre - Production", Selected = DefaultPalletListPrinter == "Xerox WorkCentre - Production" }
            };

            // Item label printers
            ItemLabelPrinters = new List<SelectListItem>
            {
                new SelectListItem { Value = "Zebra ZT410 - Warehouse", Text = "Zebra ZT410 - Warehouse", Selected = DefaultItemLabelPrinter == "Zebra ZT410 - Warehouse" },
                new SelectListItem { Value = "Zebra ZT230 - Shipping", Text = "Zebra ZT230 - Shipping", Selected = DefaultItemLabelPrinter == "Zebra ZT230 - Shipping" }
            };

            // Session timeout options
            SessionTimeoutOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "15", Text = "15 minutes", Selected = SessionTimeout == 15 },
                new SelectListItem { Value = "30", Text = "30 minutes", Selected = SessionTimeout == 30 },
                new SelectListItem { Value = "60", Text = "1 hour", Selected = SessionTimeout == 60 },
                new SelectListItem { Value = "120", Text = "2 hours", Selected = SessionTimeout == 120 }
            };
        }

        /// <summary>
        /// Gets the platforms for a specific division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>List of select list items for platforms</returns>
        public static List<SelectListItem> GetPlatformsForDivision(string division)
        {
            List<SelectListItem> platforms = new List<SelectListItem>();

            switch (division)
            {
                case "MA":
                    platforms.Add(new SelectListItem { Value = "TEC1", Text = "TEC1" });
                    platforms.Add(new SelectListItem { Value = "TEC2", Text = "TEC2" });
                    platforms.Add(new SelectListItem { Value = "TEC4I", Text = "TEC4I" });
                    break;
                case "TC":
                    platforms.Add(new SelectListItem { Value = "TEC1", Text = "TEC1" });
                    platforms.Add(new SelectListItem { Value = "TEC3", Text = "TEC3" });
                    platforms.Add(new SelectListItem { Value = "TEC5", Text = "TEC5" });
                    break;
                default:
                    platforms.Add(new SelectListItem { Value = "TEC1", Text = "TEC1" });
                    break;
            }

            return platforms;
        }

        /// <summary>
        /// Updates the available platforms based on the selected division
        /// </summary>
        public void UpdatePlatformsForDivision()
        {
            AvailablePlatforms = GetPlatformsForDivision(PreferredDivision);

            // Reset platform to a valid option if current selection is not valid for the division
            bool isValidPlatform = AvailablePlatforms.Any(p => p.Value == PreferredPlatform);
            if (!isValidPlatform && AvailablePlatforms.Any())
            {
                PreferredPlatform = AvailablePlatforms.First().Value;
            }
        }
    }
}