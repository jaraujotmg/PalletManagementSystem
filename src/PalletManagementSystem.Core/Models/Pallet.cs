using System;
using System.Collections.Generic;
using System.Linq;
using PalletManagementSystem.Core.Exceptions;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;

namespace PalletManagementSystem.Core.Models
{
    /// <summary>
    /// Represents a pallet in the system
    /// </summary>
    public class Pallet
    {
        private readonly List<Item> _items = new List<Item>();

        /// <summary>
        /// Gets the pallet ID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the pallet number
        /// </summary>
        public PalletNumber PalletNumber { get; private set; }

        /// <summary>
        /// Gets the manufacturing order associated with this pallet
        /// </summary>
        public string ManufacturingOrder { get; private set; }

        /// <summary>
        /// Gets the division this pallet belongs to
        /// </summary>
        public Division Division { get; private set; }

        /// <summary>
        /// Gets the platform this pallet is assigned to
        /// </summary>
        public Platform Platform { get; private set; }

        /// <summary>
        /// Gets the unit of measure for this pallet
        /// </summary>
        public UnitOfMeasure UnitOfMeasure { get; private set; }

        /// <summary>
        /// Gets the total quantity on this pallet
        /// </summary>
        public decimal Quantity { get; private set; }

        /// <summary>
        /// Gets the number of items on this pallet
        /// </summary>
        public int ItemCount => _items.Count;

        /// <summary>
        /// Gets a value indicating whether this pallet is closed
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Gets the date and time when this pallet was created
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// Gets the date and time when this pallet was closed
        /// </summary>
        public DateTime? ClosedDate { get; private set; }

        /// <summary>
        /// Gets the username of the person who created this pallet
        /// </summary>
        public string CreatedBy { get; private set; }

        /// <summary>
        /// Gets a read-only collection of items on this pallet
        /// </summary>
        public IReadOnlyCollection<Item> Items => _items.AsReadOnly();

        /// <summary>
        /// Initializes a new instance of the <see cref="Pallet"/> class
        /// Private constructor for EF Core
        /// </summary>
        private Pallet() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pallet"/> class
        /// </summary>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="manufacturingOrder">The manufacturing order</param>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="unitOfMeasure">The unit of measure</param>
        /// <param name="createdBy">Username of creator</param>
        public Pallet(
            PalletNumber palletNumber,
            string manufacturingOrder,
            Division division,
            Platform platform,
            UnitOfMeasure unitOfMeasure,
            string createdBy)
        {
            ValidateConstructorParameters(
                palletNumber,
                manufacturingOrder,
                division,
                platform,
                createdBy);

            PalletNumber = palletNumber;
            ManufacturingOrder = manufacturingOrder;
            Division = division;
            Platform = platform;
            UnitOfMeasure = unitOfMeasure;
            CreatedBy = createdBy;
            CreatedDate = DateTime.Now;
            IsClosed = false;
            Quantity = 0;
        }

        private void ValidateConstructorParameters(
            PalletNumber palletNumber,
            string manufacturingOrder,
            Division division,
            Platform platform,
            string createdBy)
        {
            if (palletNumber == null)
            {
                throw new ArgumentNullException(nameof(palletNumber));
            }

            if (string.IsNullOrWhiteSpace(manufacturingOrder))
            {
                throw new ArgumentException("Manufacturing order cannot be null or empty", nameof(manufacturingOrder));
            }

            if (string.IsNullOrWhiteSpace(createdBy))
            {
                throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));
            }
        }

        /// <summary>
        /// Add an item to this pallet
        /// </summary>
        /// <param name="item">The item to add</param>
        public void AddItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (IsClosed)
            {
                throw new PalletClosedException("Cannot add items to a closed pallet");
            }

            // Only add the item if it's not already on this pallet
            if (!_items.Contains(item))
            {
                _items.Add(item);
                RecalculateQuantity();
            }
        }

        /// <summary>
        /// Remove an item from this pallet
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void RemoveItem(Item item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (IsClosed)
            {
                throw new PalletClosedException("Cannot remove items from a closed pallet");
            }

            _items.Remove(item);
            RecalculateQuantity();
        }

        /// <summary>
        /// Closes the pallet and assigns a permanent number if needed
        /// </summary>
        /// <param name="permanentNumber">The permanent pallet number to assign (if needed)</param>
        public void Close(PalletNumber permanentNumber = null)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("Pallet is already closed");
            }

            // If pallet has a temporary number and no permanent number was provided, throw exception
            if (PalletNumber.IsTemporary && permanentNumber == null)
            {
                throw new ArgumentNullException(nameof(permanentNumber),
                    "A permanent pallet number must be provided when closing a pallet with a temporary number");
            }

            // Assign permanent number if needed
            if (PalletNumber.IsTemporary)
            {
                PalletNumber = permanentNumber;
            }

            IsClosed = true;
            ClosedDate = DateTime.Now;
        }

        /// <summary>
        /// Recalculates the total quantity on this pallet
        /// </summary>
        private void RecalculateQuantity()
        {
            Quantity = _items.Sum(i => i.Quantity);
        }
    }
}