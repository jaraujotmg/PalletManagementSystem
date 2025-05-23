using System;
using PalletManagementSystem.Core.Exceptions;

namespace PalletManagementSystem.Core.Models
{
    /// <summary>
    /// Represents an item on a pallet
    /// </summary>
    public class Item
    {
        // Special client indicators - centralized here
        private const string SPECIAL_CLIENT_CODE = "280898";
        private const string SPECIAL_CLIENT_NAME = "Special Client HB";

        /// <summary>
        /// Gets the item ID
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Gets the unique item number
        /// </summary>
        public string ItemNumber { get; protected set; }

        /// <summary>
        /// Gets or sets the pallet ID this item belongs to
        /// </summary>
        public int PalletId { get; set; }

        /// <summary>
        /// Gets or sets the pallet this item belongs to
        /// </summary>
        public Pallet Pallet { get; set; }

        #region Order Information

        /// <summary>
        /// Gets the manufacturing order
        /// </summary>
        public string ManufacturingOrder { get; protected set; }

        /// <summary>
        /// Gets the manufacturing order line
        /// </summary>
        public string ManufacturingOrderLine { get; protected set; }

        /// <summary>
        /// Gets the service order
        /// </summary>
        public string ServiceOrder { get; protected set; }

        /// <summary>
        /// Gets the service order line
        /// </summary>
        public string ServiceOrderLine { get; protected set; }

        /// <summary>
        /// Gets the final order
        /// </summary>
        public string FinalOrder { get; protected set; }

        /// <summary>
        /// Gets the final order line
        /// </summary>
        public string FinalOrderLine { get; protected set; }

        #endregion

        #region Client Information

        /// <summary>
        /// Gets the client code
        /// </summary>
        public string ClientCode { get; protected set; }

        /// <summary>
        /// Gets the client name
        /// </summary>
        public string ClientName { get; protected set; }

        #endregion

        #region Product Information

        /// <summary>
        /// Gets the reference
        /// </summary>
        public string Reference { get; protected set; }

        /// <summary>
        /// Gets the finish
        /// </summary>
        public string Finish { get; protected set; }

        /// <summary>
        /// Gets the color
        /// </summary>
        public string Color { get; protected set; }

        /// <summary>
        /// Gets the quantity
        /// </summary>
        public decimal Quantity { get; protected set; }

        /// <summary>
        /// Gets the quantity unit
        /// </summary>
        public string QuantityUnit { get; protected set; }

        #endregion

        #region Physical Properties (Editable)

        /// <summary>
        /// Gets or sets the weight (editable)
        /// </summary>
        public decimal Weight { get; protected set; }

        /// <summary>
        /// Gets the weight unit
        /// </summary>
        public string WeightUnit { get; protected set; }

        /// <summary>
        /// Gets or sets the width (editable)
        /// </summary>
        public decimal Width { get; protected set; }

        /// <summary>
        /// Gets the width unit
        /// </summary>
        public string WidthUnit { get; protected set; }

        /// <summary>
        /// Gets or sets the quality (editable)
        /// </summary>
        public string Quality { get; protected set; }

        /// <summary>
        /// Gets or sets the batch (editable)
        /// </summary>
        public string Batch { get; protected set; }

        #endregion

        /// <summary>
        /// Gets the created date
        /// </summary>
        public DateTime CreatedDate { get; protected set; }

        /// <summary>
        /// Gets the created by username
        /// </summary>
        public string CreatedBy { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class
        /// Private constructor for EF Core
        /// </summary>
        private Item() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class
        /// </summary>
        /// <param name="itemNumber">The item number</param>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="manufacturingOrderLine">The manufacturing order line</param>
        /// <param name="serviceOrder">The service order</param>
        /// <param name="serviceOrderLine">The service order line</param>
        /// <param name="finalOrder">The final order</param>
        /// <param name="finalOrderLine">The final order line</param>
        /// <param name="clientCode">The client code</param>
        /// <param name="clientName">The client name</param>
        /// <param name="reference">The reference</param>
        /// <param name="finish">The finish</param>
        /// <param name="color">The color</param>
        /// <param name="quantity">The quantity</param>
        /// <param name="quantityUnit">The quantity unit</param>
        /// <param name="weight">The weight</param>
        /// <param name="weightUnit">The weight unit</param>
        /// <param name="width">The width</param>
        /// <param name="widthUnit">The width unit</param>
        /// <param name="quality">The quality</param>
        /// <param name="batch">The batch</param>
        /// <param name="createdBy">The username of the creator</param>
        public Item(
            string itemNumber,
            string manufacturingOrder,
            string manufacturingOrderLine,
            string serviceOrder,
            string serviceOrderLine,
            string finalOrder,
            string finalOrderLine,
            string clientCode,
            string clientName,
            string reference,
            string finish,
            string color,
            decimal quantity,
            string quantityUnit,
            decimal weight,
            string weightUnit,
            decimal width,
            string widthUnit,
            string quality,
            string batch,
            string createdBy)
        {
            ValidateConstructorParameters(
                itemNumber,
                manufacturingOrder,
                quantity,
                weight,
                width,
                createdBy);

            ItemNumber = itemNumber;
            ManufacturingOrder = manufacturingOrder;
            ManufacturingOrderLine = manufacturingOrderLine;
            ServiceOrder = serviceOrder;
            ServiceOrderLine = serviceOrderLine;
            FinalOrder = finalOrder;
            FinalOrderLine = finalOrderLine;
            ClientCode = clientCode;
            ClientName = clientName;
            Reference = reference;
            Finish = finish;
            Color = color;
            Quantity = quantity;
            QuantityUnit = quantityUnit;
            Weight = weight;
            WeightUnit = weightUnit;
            Width = width;
            WidthUnit = widthUnit;
            Quality = quality;
            Batch = batch;
            CreatedDate = DateTime.Now;
            CreatedBy = createdBy;
        }

        private void ValidateConstructorParameters(
            string itemNumber,
            string manufacturingOrder,
            decimal quantity,
            decimal weight,
            decimal width,
            string createdBy)
        {
            if (string.IsNullOrWhiteSpace(itemNumber))
            {
                throw new ArgumentException("Item number cannot be null or empty", nameof(itemNumber));
            }

            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            if (string.IsNullOrWhiteSpace(createdBy))
            {
                throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            }

            if (weight < 0)
            {
                throw new ArgumentException("Weight cannot be negative", nameof(weight));
            }

            if (width < 0)
            {
                throw new ArgumentException("Width cannot be negative", nameof(width));
            }
        }

        /// <summary>
        /// Updates the editable properties of the item
        /// </summary>
        /// <param name="weight">The new weight</param>
        /// <param name="width">The new width</param>
        /// <param name="quality">The new quality</param>
        /// <param name="batch">The new batch</param>
        public void Update(decimal weight, decimal width, string quality, string batch)
        {
            EnsurePalletNotClosed();
            ValidateUpdateParameters(weight, width, batch);

            Weight = weight;
            Width = width;
            Quality = quality;
            Batch = batch;
        }

        /// <summary>
        /// Assigns this item to a pallet
        /// </summary>
        /// <param name="pallet">The pallet to assign to</param>
        public void AssignToPallet(Pallet pallet)
        {
            if (pallet == null)
            {
                throw new ArgumentNullException(nameof(pallet));
            }

            if (pallet.IsClosed)
            {
                throw new PalletClosedException("Cannot assign items to a closed pallet");
            }

            if (Pallet != null && Pallet.IsClosed)
            {
                throw new PalletClosedException("Cannot reassign items from a closed pallet");
            }

            // Set the pallet and update the ID
            Pallet = pallet;
            PalletId = pallet.Id;

            // Let the pallet know about this item
            pallet.AddItem(this);
        }

        /// <summary>
        /// Removes this item from its current pallet
        /// </summary>
        public void RemoveFromPallet()
        {
            if (Pallet == null)
            {
                return;
            }

            if (Pallet.IsClosed)
            {
                throw new PalletClosedException("Cannot remove items from a closed pallet");
            }

            // Let the pallet know this item is being removed
            Pallet.RemoveItem(this);

            // Clear the pallet references
            Pallet = null;
            PalletId = 0;
        }

        /// <summary>
        /// Moves this item from its current pallet to another pallet
        /// </summary>
        /// <param name="targetPallet">The target pallet</param>
        public void MoveToPallet(Pallet targetPallet)
        {
            if (targetPallet == null)
            {
                throw new ArgumentNullException(nameof(targetPallet));
            }

            // Remove from current pallet if any
            RemoveFromPallet();

            // Assign to the target pallet
            AssignToPallet(targetPallet);
        }

        /// <summary>
        /// Checks if the item belongs to a special client
        /// </summary>
        /// <returns>True if the client is special, false otherwise</returns>
        public bool IsSpecialClient()
        {
            return ClientCode == SPECIAL_CLIENT_CODE && ClientName == SPECIAL_CLIENT_NAME;
        }

        private void EnsurePalletNotClosed()
        {
            if (Pallet != null && Pallet.IsClosed)
            {
                throw new PalletClosedException("Cannot update items on a closed pallet");
            }
        }

        private void ValidateUpdateParameters(decimal weight, decimal width, string batch)
        {
            if (weight < 0)
            {
                throw new ItemValidationException("Weight cannot be negative");
            }

            if (width < 0)
            {
                throw new ItemValidationException("Width cannot be negative");
            }

            if (string.IsNullOrWhiteSpace(batch))
            {
                throw new ItemValidationException("Batch cannot be empty");
            }
        }
    }
}