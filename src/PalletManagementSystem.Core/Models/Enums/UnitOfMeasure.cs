using System.ComponentModel;

namespace PalletManagementSystem.Core.Models.Enums
{
    /// <summary>
    /// Represents units of measure for pallets and items
    /// </summary>
    public enum UnitOfMeasure
    {
        /// <summary>
        /// Piece
        /// </summary>
        [Description("Piece")]
        PC,

        /// <summary>
        /// Kilogram
        /// </summary>
        [Description("Kilogram")]
        KG,

        /// <summary>
        /// Box
        /// </summary>
        [Description("Box")]
        BOX,

        /// <summary>
        /// Roll
        /// </summary>
        [Description("Roll")]
        ROLL
    }
}