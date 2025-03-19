using System.ComponentModel.DataAnnotations;

namespace PalletManagementSystem.Web.ViewModels.PalletViewModels
{
    /// <summary>
    /// View model for closing a pallet
    /// </summary>
    public class ClosePalletViewModel
    {
        /// <summary>
        /// The ID of the pallet to close
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The pallet number
        /// </summary>
        public string PalletNumber { get; set; }

        /// <summary>
        /// Whether the pallet is temporary (needs a permanent number)
        /// </summary>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Manufacturing order associated with the pallet
        /// </summary>
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Number of items on the pallet
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Total quantity on the pallet
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Unit of measure for the pallet
        /// </summary>
        public string UnitOfMeasure { get; set; }

        /// <summary>
        /// Whether to automatically print the pallet list after closing
        /// </summary>
        public bool AutoPrint { get; set; } = true;

        /// <summary>
        /// Additional notes for closing (optional)
        /// </summary>
        [StringLength(200, ErrorMessage = "Notes cannot exceed 200 characters")]
        public string Notes { get; set; }

        /// <summary>
        /// Confirmation checkbox to ensure the user understands the implications
        /// </summary>
        [Display(Name = "I understand that this action cannot be undone")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm that you understand this action cannot be undone")]
        public bool ConfirmUnderstanding { get; set; }

        /// <summary>
        /// Whether the pallet has the minimum required items to be closed
        /// </summary>
        public bool HasMinimumItems => ItemCount > 0;

        /// <summary>
        /// Whether closing the pallet is allowed
        /// </summary>
        public bool CanBeClosed => HasMinimumItems && !string.IsNullOrEmpty(ManufacturingOrder);

        /// <summary>
        /// Gets a formatted warning message based on pallet state
        /// </summary>
        public string WarningMessage
        {
            get
            {
                if (!HasMinimumItems)
                {
                    return "This pallet has no items. Are you sure you want to close it?";
                }

                return IsTemporary
                    ? "Once closed, a permanent pallet number will be assigned and items cannot be edited or moved anymore."
                    : "Once closed, items on this pallet cannot be edited or moved anymore.";
            }
        }

        /// <summary>
        /// Creates a new ClosePalletViewModel with default values
        /// </summary>
        public ClosePalletViewModel()
        {
            AutoPrint = true;
        }

        /// <summary>
        /// Creates a new ClosePalletViewModel with specified values
        /// </summary>
        /// <param name="id">Pallet ID</param>
        /// <param name="palletNumber">Pallet number</param>
        /// <param name="isTemporary">Whether the pallet is temporary</param>
        /// <param name="manufacturingOrder">Manufacturing order</param>
        /// <param name="itemCount">Item count</param>
        /// <param name="quantity">Total quantity</param>
        /// <param name="unitOfMeasure">Unit of measure</param>
        public ClosePalletViewModel(
            int id,
            string palletNumber,
            bool isTemporary,
            string manufacturingOrder,
            int itemCount,
            double quantity,
            string unitOfMeasure)
        {
            Id = id;
            PalletNumber = palletNumber;
            IsTemporary = isTemporary;
            ManufacturingOrder = manufacturingOrder;
            ItemCount = itemCount;
            Quantity = quantity;
            UnitOfMeasure = unitOfMeasure;
            AutoPrint = true;
        }
    }
}