using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.ViewModels.ItemViewModels
{
    /// <summary>
    /// View model for creating a new item on a pallet
    /// </summary>
    public class CreateItemViewModel
    {
        /// <summary>
        /// The ID of the pallet the item belongs to
        /// </summary>
        [Required]
        public int PalletId { get; set; }

        /// <summary>
        /// The pallet number (for display purposes)
        /// </summary>
        public string PalletNumber { get; set; }

        #region Order Information

        /// <summary>
        /// Manufacturing order associated with the item
        /// </summary>
        [Required(ErrorMessage = "Manufacturing Order is required")]
        [StringLength(50, ErrorMessage = "Manufacturing Order cannot exceed 50 characters")]
        [Display(Name = "Manufacturing Order")]
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Manufacturing order line
        /// </summary>
        [Required(ErrorMessage = "Manufacturing Order Line is required")]
        [StringLength(10, ErrorMessage = "Manufacturing Order Line cannot exceed 10 characters")]
        [Display(Name = "MO Line")]
        public string ManufacturingOrderLine { get; set; }

        /// <summary>
        /// Service order
        /// </summary>
        [StringLength(50, ErrorMessage = "Service Order cannot exceed 50 characters")]
        [Display(Name = "Service Order")]
        public string ServiceOrder { get; set; }

        /// <summary>
        /// Service order line
        /// </summary>
        [StringLength(10, ErrorMessage = "Service Order Line cannot exceed 10 characters")]
        [Display(Name = "SO Line")]
        public string ServiceOrderLine { get; set; }

        /// <summary>
        /// Final order
        /// </summary>
        [StringLength(50, ErrorMessage = "Final Order cannot exceed 50 characters")]
        [Display(Name = "Final Order")]
        public string FinalOrder { get; set; }

        /// <summary>
        /// Final order line
        /// </summary>
        [StringLength(10, ErrorMessage = "Final Order Line cannot exceed 10 characters")]
        [Display(Name = "FO Line")]
        public string FinalOrderLine { get; set; }

        #endregion

        #region Client Information

        /// <summary>
        /// Client code
        /// </summary>
        [Required(ErrorMessage = "Client Code is required")]
        [StringLength(20, ErrorMessage = "Client Code cannot exceed 20 characters")]
        [Display(Name = "Client Code")]
        public string ClientCode { get; set; }

        /// <summary>
        /// Client name
        /// </summary>
        [Required(ErrorMessage = "Client Name is required")]
        [StringLength(100, ErrorMessage = "Client Name cannot exceed 100 characters")]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; }

        #endregion

        #region Product Information

        /// <summary>
        /// Reference
        /// </summary>
        [Required(ErrorMessage = "Reference is required")]
        [StringLength(50, ErrorMessage = "Reference cannot exceed 50 characters")]
        [Display(Name = "Reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Finish
        /// </summary>
        [Required(ErrorMessage = "Finish is required")]
        [StringLength(50, ErrorMessage = "Finish cannot exceed 50 characters")]
        [Display(Name = "Finish")]
        public string Finish { get; set; }

        /// <summary>
        /// Color
        /// </summary>
        [Required(ErrorMessage = "Color is required")]
        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        [Display(Name = "Color")]
        public string Color { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.1, 10000, ErrorMessage = "Quantity must be a positive number")]
        [Display(Name = "Quantity")]
        public double Quantity { get; set; }

        /// <summary>
        /// Quantity unit
        /// </summary>
        [Required(ErrorMessage = "Quantity Unit is required")]
        [Display(Name = "Quantity Unit")]
        public string QuantityUnit { get; set; }

        #endregion

        #region Physical Properties

        /// <summary>
        /// Weight
        /// </summary>
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 10000, ErrorMessage = "Weight must be a positive number")]
        [Display(Name = "Weight")]
        public double Weight { get; set; }

        /// <summary>
        /// Weight unit
        /// </summary>
        [Required(ErrorMessage = "Weight Unit is required")]
        [Display(Name = "Weight Unit")]
        public string WeightUnit { get; set; } = "KG";

        /// <summary>
        /// Width
        /// </summary>
        [Required(ErrorMessage = "Width is required")]
        [Range(0.1, 10000, ErrorMessage = "Width must be a positive number")]
        [Display(Name = "Width")]
        public double Width { get; set; }

        /// <summary>
        /// Width unit
        /// </summary>
        [Required(ErrorMessage = "Width Unit is required")]
        [Display(Name = "Width Unit")]
        public string WidthUnit { get; set; } = "CM";

        /// <summary>
        /// Quality
        /// </summary>
        [Required(ErrorMessage = "Quality is required")]
        [Display(Name = "Quality")]
        public string Quality { get; set; }

        /// <summary>
        /// Batch
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
        /// Available quantity units
        /// </summary>
        public List<SelectListItem> QuantityUnitOptions { get; set; }

        /// <summary>
        /// Available quality options
        /// </summary>
        public List<SelectListItem> QualityOptions { get; set; }

        /// <summary>
        /// Common client options for autocomplete
        /// </summary>
        public List<ClientOption> CommonClients { get; set; }

        /// <summary>
        /// Creates a new instance of CreateItemViewModel
        /// </summary>
        public CreateItemViewModel()
        {
            InitializeDropdownOptions();
            InitializeCommonClients();
        }

        /// <summary>
        /// Creates a new instance with specified pallet information
        /// </summary>
        /// <param name="palletId">The pallet ID</param>
        /// <param name="palletNumber">The pallet number</param>
        /// <param name="touchModeEnabled">Whether touch mode is enabled</param>
        public CreateItemViewModel(int palletId, string palletNumber, bool touchModeEnabled = false)
        {
            PalletId = palletId;
            PalletNumber = palletNumber;
            TouchModeEnabled = touchModeEnabled;

            InitializeDropdownOptions();
            InitializeCommonClients();
        }

        /// <summary>
        /// Initializes dropdown options
        /// </summary>
        private void InitializeDropdownOptions()
        {
            // Initialize quantity unit options
            QuantityUnitOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "PC", Text = "PC (Piece)", Selected = QuantityUnit == "PC" },
                new SelectListItem { Value = "KG", Text = "KG (Kilogram)", Selected = QuantityUnit == "KG" },
                new SelectListItem { Value = "BOX", Text = "BOX", Selected = QuantityUnit == "BOX" },
                new SelectListItem { Value = "ROLL", Text = "ROLL", Selected = QuantityUnit == "ROLL" }
            };

            // Initialize quality options
            QualityOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Premium", Text = "Premium", Selected = Quality == "Premium" },
                new SelectListItem { Value = "Standard", Text = "Standard", Selected = Quality == "Standard" || string.IsNullOrEmpty(Quality) },
                new SelectListItem { Value = "Economy", Text = "Economy", Selected = Quality == "Economy" },
                new SelectListItem { Value = "Special", Text = "Special", Selected = Quality == "Special" }
            };

            // Set default values if not already set
            if (string.IsNullOrEmpty(QuantityUnit))
            {
                QuantityUnit = "PC";
            }

            if (string.IsNullOrEmpty(Quality))
            {
                Quality = "Standard";
            }
        }

        /// <summary>
        /// Initializes common clients for autocomplete
        /// </summary>
        private void InitializeCommonClients()
        {
            CommonClients = new List<ClientOption>
            {
                new ClientOption { ClientCode = "101567", ClientName = "Standard Manufacturing Inc." },
                new ClientOption { ClientCode = "280898", ClientName = "Special Client HB" },
                new ClientOption { ClientCode = "328711", ClientName = "Global Industries Ltd." }
            };
        }

        /// <summary>
        /// Class for client options in autocomplete
        /// </summary>
        public class ClientOption
        {
            /// <summary>
            /// Client code
            /// </summary>
            public string ClientCode { get; set; }

            /// <summary>
            /// Client name
            /// </summary>
            public string ClientName { get; set; }
        }
    }
}