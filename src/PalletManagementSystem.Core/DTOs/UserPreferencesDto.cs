namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// Data transfer object for user preferences
    /// </summary>
    public class UserPreferencesDto
    {
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the preferred division
        /// </summary>
        public string PreferredDivision { get; set; }

        /// <summary>
        /// Gets or sets the preferred platform
        /// </summary>
        public string PreferredPlatform { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether touch mode is enabled
        /// </summary>
        public bool TouchModeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the default pallet view
        /// </summary>
        public string DefaultPalletView { get; set; }

        /// <summary>
        /// Gets or sets the default pallet list printer
        /// </summary>
        public string DefaultPalletListPrinter { get; set; }

        /// <summary>
        /// Gets or sets the default item label printer
        /// </summary>
        public string DefaultItemLabelPrinter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show confirmation prompts
        /// </summary>
        public bool ShowConfirmationPrompts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-print pallet lists when closing
        /// </summary>
        public bool AutoPrintPalletList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use special printer for special clients
        /// </summary>
        public bool UseSpecialPrinterForSpecialClients { get; set; }

        /// <summary>
        /// Gets or sets the session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remember division and platform
        /// </summary>
        public bool RememberDivisionAndPlatform { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto-refresh the pallet list
        /// </summary>
        public bool AutoRefreshPalletList { get; set; }

        /// <summary>
        /// Gets or sets the auto-refresh interval in seconds
        /// </summary>
        public int AutoRefreshIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show browser notifications
        /// </summary>
        public bool ShowBrowserNotifications { get; set; }
    }
}