using System.ComponentModel;

namespace PalletManagementSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents the different divisions in the company
    /// </summary>
    public enum Division
    {
        /// <summary>
        /// Manufacturing Division
        /// </summary>
        [Description("Manufacturing")]
        MA,

        /// <summary>
        /// Technical Center Division
        /// </summary>
        [Description("Technical Center")]
        TC
    }
}