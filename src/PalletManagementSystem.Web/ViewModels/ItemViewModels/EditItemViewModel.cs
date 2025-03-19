using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.ViewModels.ItemViewModels
{
    /// <summary>
    /// View model for editing an item's physical properties
    /// </summary>
    public class EditItemViewModel
    {
        /// <summary>
        /// The ID of the item
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The item number
        /// </summary>
        [Display(Name = "Item Number")]
        public string ItemNumber { get; set; }

        /// <summary>
        /// The ID of the pallet the item belongs to
        /// </summary>
        [Required]
        public int PalletId { get; set; }

        /// <summary>
        /// The pallet number
        /// </summary>
        [Display(Name = "Pallet Number")]
        public string PalletNumber { get; set; }

        #region Non-editable fields (for display only)

        /// <summary>
        /// Manufacturing order associated with the item
        /// </summary>
        [Display(Name = "Manufacturing Order")]
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Manufacturing order line
        /// </summary>
        [Display(Name = "MO Line")]
        public string ManufacturingOrderLine { get; set; }

        /// <summary>
        /// Client code
        /// </summary>
        [Display(Name = "Client Code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Client name
        /// </summary>
        [Display(Name = "Client Name")]
        public string ClientName { get; set; }

        /// <summary>
        /// Reference
        /// </summary>
        [Display(Name = "Reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Finish
        /// </summary>
        [Display(Name = "Finish")]
        public string Finish { get; set; }

        /// <summary>
        /// Color
        /// </summary>
        [Display(Name = "Color")]
        public string Color { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [Display(Name = "Quantity")]
        public double Quantity { get; set; }

        /// <summary>
        /// Quantity unit
        /// </summary>
        [Display(Name = "Quantity Unit")]
        public string QuantityUnit { get; set; }

        #endregion

        #region Editable fields

        /// <summary>
        /// Weight - Editable
        /// </summary>
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 10000, ErrorMessage = "Weight must be a positive number")]
        [Display(Name = "Weight")]
        public double Weight { get; set; }

        /// <summary>
        /// Weight unit - Display only
        /// </summary>
        [Display(Name = "Weight Unit")]
        public string WeightUnit { get; set; } = "KG";

        /// <summary>
        /// Width - Editable
        /// </summary>
        [Required(ErrorMessage = "Width is required")]
        [Range(0.1, 10000, ErrorMessage = "Width must be a positive number")]
        [Display(Name = "Width")]
        public double Width { get; set; }

        /// <summary>
        /// Width unit - Display only
        /// </summary>
        [Display(Name = "Width Unit")]
        public string WidthUnit { get; set; } = "CM";

        /// <summary>
        /// Quality - Editable
        /// </summary>
        [Required(ErrorMessage = "Quality is required")]
        [Display(Name = "Quality")]
        public string Quality { get; set; }

        /// <summary>
        /// Batch - Editable
        /// </summary>
        [Required(ErrorMessage = "Batch is required")]
        [StringLength(20, ErrorMessage = "Batch cannot exceed 20 characters")]
        [Display(Name = "Batch")]
        public string Batch { get; set; }

        #endregion

        /// <summary>
        /// Whether touch mode is enabled
        /// </summary>
        public bool TouchModeEnabled { get; set; }

        /// <summary>
        /// Available quality options for dropdown
        /// </summary>
        public List<SelectListItem> QualityOptions { get; set; }

        /// <summary>
        /// Creates a new instance of EditItemViewModel
        /// </summary>
        public EditItemViewModel()
        {
            TouchModeEnabled = false;
            InitializeQualityOptions();
        }

        /// <summary>
        /// Initializes the quality options dropdown
        /// </summary>
        private void InitializeQualityOptions()
        {
            QualityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Premium", Text = "Premium" },
                new SelectListItem { Value = "Standard", Text = "Standard" },
                new SelectListItem { Value = "Economy", Text = "Economy" },
                new SelectListItem { Value = "Special", Text = "Special" }
            };
        }

        /// <summary>
        /// Creates a view model from a Core.DTOs.ItemDto object
        /// </summary>
        /// <param name="item">The item DTO</param>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="touchModeEnabled">Whether touch mode is enabled</param>
        /// <returns>Populated view model</returns>
        public static EditItemViewModel FromItemDto(Core.DTOs.ItemDto item, string palletNumber, bool touchModeEnabled = false)
        {
            var viewModel = new EditItemViewModel
            {
                Id = item.Id,
                ItemNumber = item.ItemNumber,
                PalletId = item.PalletId,
                PalletNumber = palletNumber,
                ManufacturingOrder = item.ManufacturingOrder,
                ManufacturingOrderLine = item.ManufacturingOrderLine,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                Reference = item.Reference,
                Finish = item.Finish,
                Color = item.Color,
                Quantity = item.Quantity,
                QuantityUnit = item.QuantityUnit,
                Weight = item.Weight,
                WeightUnit = item.WeightUnit,
                Width = item.Width,
                WidthUnit = item.WidthUnit,
                Quality = item.Quality,
                Batch = item.Batch,
                TouchModeEnabled = touchModeEnabled
            };

            viewModel.InitializeQualityOptions();

            // Set the selected quality option
            if (!string.IsNullOrEmpty(item.Quality))
            {
                var selectedOption = viewModel.QualityOptions.Find(q => q.Value == item.Quality);
                if (selectedOption != null)
                {
                    selectedOption.Selected = true;
                }
            }

            return viewModel;
        }
    }
}