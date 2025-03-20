using System;

namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// DTO for pallet list view (without items)
    /// </summary>
    public class PalletListDto
    {
        /// <summary>
        /// Gets or sets the pallet ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the pallet number
        /// </summary>
        public string PalletNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this pallet has a temporary number
        /// </summary>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Gets or sets the manufacturing order
        /// </summary>
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Gets or sets the division
        /// </summary>
        public string Division { get; set; }

        /// <summary>
        /// Gets or sets the platform
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure
        /// </summary>
        public string UnitOfMeasure { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the number of items on this pallet
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this pallet is closed
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Gets or sets the date when this pallet was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the date when this pallet was closed
        /// </summary>
        public DateTime? ClosedDate { get; set; }

        /// <summary>
        /// Gets or sets the username of the creator
        /// </summary>
        public string CreatedBy { get; set; }
    }
}