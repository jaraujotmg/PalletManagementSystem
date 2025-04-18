﻿using System.ComponentModel;

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

    /// <summary>
    /// Represents platforms within divisions
    /// </summary>
    public enum Platform
    {
        /// <summary>
        /// Technology platform 1 (available in both divisions)
        /// </summary>
        [Description("TEC1")]
        TEC1,

        /// <summary>
        /// Technology platform 2 (Manufacturing division only)
        /// </summary>
        [Description("TEC2")]
        TEC2,

        /// <summary>
        /// Technology platform 3 (Technical Center division only)
        /// </summary>
        [Description("TEC3")]
        TEC3,

        /// <summary>
        /// Technology platform 4I (Manufacturing division only)
        /// </summary>
        [Description("TEC4I")]
        TEC4I,

        /// <summary>
        /// Technology platform 5 (Technical Center division only)
        /// </summary>
        [Description("TEC5")]
        TEC5
    }

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