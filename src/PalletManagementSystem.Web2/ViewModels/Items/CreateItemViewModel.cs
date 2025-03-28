// src/PalletManagementSystem.Web/ViewModels/Items/CreateItemViewModel.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using PalletManagementSystem.Web2.ViewModels.Shared;

namespace PalletManagementSystem.Web2.ViewModels.Items
{
    public class CreateItemViewModel : ViewModelBase
    {
        public int PalletId { get; set; }
        public string PalletNumber { get; set; }

        [Required(ErrorMessage = "Manufacturing Order is required")]
        [Display(Name = "Manufacturing Order")]
        public string ManufacturingOrder { get; set; }

        [Display(Name = "Manufacturing Order Line")]
        public string ManufacturingOrderLine { get; set; }

        [Display(Name = "Service Order")]
        public string ServiceOrder { get; set; }

        [Display(Name = "Service Order Line")]
        public string ServiceOrderLine { get; set; }

        [Display(Name = "Final Order")]
        public string FinalOrder { get; set; }

        [Display(Name = "Final Order Line")]
        public string FinalOrderLine { get; set; }

        [Required(ErrorMessage = "Client Code is required")]
        [Display(Name = "Client Code")]
        public string ClientCode { get; set; }

        [Required(ErrorMessage = "Client Name is required")]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; }

        [Display(Name = "Reference")]
        public string Reference { get; set; }

        [Display(Name = "Finish")]
        public string Finish { get; set; }

        [Display(Name = "Color")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Quantity must be greater than zero")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Quantity Unit is required")]
        [Display(Name = "Quantity Unit")]
        public string QuantityUnit { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Weight must be greater than zero")]
        [Display(Name = "Weight")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Weight Unit is required")]
        [Display(Name = "Weight Unit")]
        public string WeightUnit { get; set; }

        [Required(ErrorMessage = "Width is required")]
        [Range(0.01, 9999.99, ErrorMessage = "Width must be greater than zero")]
        [Display(Name = "Width")]
        public decimal Width { get; set; }

        [Required(ErrorMessage = "Width Unit is required")]
        [Display(Name = "Width Unit")]
        public string WidthUnit { get; set; }

        [Required(ErrorMessage = "Quality is required")]
        [Display(Name = "Quality")]
        public string Quality { get; set; }

        [Required(ErrorMessage = "Batch is required")]
        [Display(Name = "Batch")]
        public string Batch { get; set; }

        // Dropdown options
        public List<SelectListItem> QuantityUnitOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> WeightUnitOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> WidthUnitOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> QualityOptions { get; set; } = new List<SelectListItem>();

        // Client search functionality
        public bool ShowClientSearch { get; set; } = false;
        public string ClientSearchKeyword { get; set; }

        // Form control options
        public bool EnableTouchMode { get; set; }

        // Return URL for cancel button
        public string ReturnUrl { get; set; }
    }
}