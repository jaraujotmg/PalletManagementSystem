using System;

namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// DTO for item list view (without pallet)
    /// </summary>
    public class ItemListDto
    {
        /// <summary>
        /// Gets or sets the item ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the item number
        /// </summary>
        public string ItemNumber { get; set; }

        /// <summary>
        /// Gets or sets the pallet ID
        /// </summary>
        public int PalletId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturing order
        /// </summary>
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Gets or sets the client code
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// Gets or sets the client name
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the quantity unit
        /// </summary>
        public string QuantityUnit { get; set; }

        /// <summary>
        /// Gets or sets the weight
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Gets or sets the weight unit
        /// </summary>
        public string WeightUnit { get; set; }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        public decimal Width { get; set; }

        /// <summary>
        /// Gets or sets the width unit
        /// </summary>
        public string WidthUnit { get; set; }

        /// <summary>
        /// Gets or sets the date when this item was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}