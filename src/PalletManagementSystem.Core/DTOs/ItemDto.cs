using System;

namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// Data transfer object for an item
    /// </summary>
    public class ItemDto
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
        /// Gets or sets the manufacturing order line
        /// </summary>
        public string ManufacturingOrderLine { get; set; }

        /// <summary>
        /// Gets or sets the service order
        /// </summary>
        public string ServiceOrder { get; set; }

        /// <summary>
        /// Gets or sets the service order line
        /// </summary>
        public string ServiceOrderLine { get; set; }

        /// <summary>
        /// Gets or sets the final order
        /// </summary>
        public string FinalOrder { get; set; }

        /// <summary>
        /// Gets or sets the final order line
        /// </summary>
        public string FinalOrderLine { get; set; }

        /// <summary>
        /// Gets or sets the client code
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// Gets or sets the client name
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets the reference
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the finish
        /// </summary>
        public string Finish { get; set; }

        /// <summary>
        /// Gets or sets the color
        /// </summary>
        public string Color { get; set; }

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
        /// Gets or sets the quality
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Gets or sets the batch
        /// </summary>
        public string Batch { get; set; }

        /// <summary>
        /// Gets or sets the date when this item was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the username of the creator
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the pallet this item belongs to
        /// </summary>
        public PalletDto Pallet { get; set; }
    }
}