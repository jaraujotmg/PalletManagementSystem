using PalletManagementSystem.Core.Models.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace PalletManagementSystem.Web.ViewModels.PalletViewModels
{
    /// <summary>
    /// View model for creating a new pallet
    /// </summary>
    public class CreatePalletViewModel
    {
        /// <summary>
        /// Manufacturing order associated with the pallet
        /// </summary>
        [Required(ErrorMessage = "Manufacturing Order is required")]
        [StringLength(50, ErrorMessage = "Manufacturing Order cannot exceed 50 characters")]
        [Display(Name = "Manufacturing Order")]
        public string ManufacturingOrder { get; set; }

        /// <summary>
        /// Division for the pallet
        /// </summary>
        [Required(ErrorMessage = "Division is required")]
        [Display(Name = "Division")]
        public string Division { get; set; }

        /// <summary>
        /// Platform for the pallet
        /// </summary>
        [Required(ErrorMessage = "Platform is required")]
        [Display(Name = "Platform")]
        public string Platform { get; set; }

        /// <summary>
        /// Unit of measure for items on the pallet
        /// </summary>
        [Required(ErrorMessage = "Unit of Measure is required")]
        [Display(Name = "Unit of Measure")]
        public string UnitOfMeasure { get; set; }

        /// <summary>
        /// Available divisions for the dropdown
        /// </summary>
        public List<SelectListItem> AvailableDivisions { get; set; }

        /// <summary>
        /// Available platforms for the selected division
        /// </summary>
        public List<SelectListItem> AvailablePlatforms { get; set; }

        /// <summary>
        /// Available units of measure for the dropdown
        /// </summary>
        public List<SelectListItem> AvailableUnitsOfMeasure { get; set; }

        /// <summary>
        /// Flag indicating if touch mode is enabled
        /// </summary>
        public bool TouchModeEnabled { get; set; }

        /// <summary>
        /// Creates a new instance of CreatePalletViewModel with default values
        /// </summary>
        public CreatePalletViewModel()
        {
            Division = Core.Models.Enums.Division.MA.ToString();
            Platform = Core.Models.Enums.Platform.TEC1.ToString();
            UnitOfMeasure = Core.Models.Enums.UnitOfMeasure.PC.ToString();
            TouchModeEnabled = false;

            // Initialize dropdown options
            InitializeDropdownOptions();
        }

        /// <summary>
        /// Creates a new instance with specified values
        /// </summary>
        /// <param name="division">The division</param>
        /// <param name="platform">The platform</param>
        /// <param name="touchModeEnabled">Whether touch mode is enabled</param>
        public CreatePalletViewModel(Division division, Platform platform, bool touchModeEnabled = false)
        {
            Division = division.ToString();
            Platform = platform.ToString();
            UnitOfMeasure = Core.Models.Enums.UnitOfMeasure.PC.ToString();
            TouchModeEnabled = touchModeEnabled;

            // Initialize dropdown options
            InitializeDropdownOptions();
        }

        /// <summary>
        /// Initialize dropdown options for divisions, platforms, and units of measure
        /// </summary>
        private void InitializeDropdownOptions()
        {
            // Initialize divisions
            AvailableDivisions = new List<SelectListItem>
            {
                new SelectListItem { Value = "MA", Text = "MA - Manufacturing", Selected = Division == "MA" },
                new SelectListItem { Value = "TC", Text = "TC - Technical Center", Selected = Division == "TC" }
            };

            // Initialize platforms based on the selected division
            AvailablePlatforms = GetPlatformsForDivision(Division);

            // Initialize units of measure
            AvailableUnitsOfMeasure = new List<SelectListItem>
            {
                new SelectListItem { Value = "PC", Text = "PC (Piece)", Selected = UnitOfMeasure == "PC" },
                new SelectListItem { Value = "KG", Text = "KG (Kilogram)", Selected = UnitOfMeasure == "KG" },
                new SelectListItem { Value = "BOX", Text = "BOX", Selected = UnitOfMeasure == "BOX" },
                new SelectListItem { Value = "ROLL", Text = "ROLL", Selected = UnitOfMeasure == "ROLL" }
            };
        }

        /// <summary>
        /// Gets the platforms for a specific division
        /// </summary>
        /// <param name="division">The division</param>
        /// <returns>List of select list items for platforms</returns>
        public static List<SelectListItem> GetPlatformsForDivision(string division)
        {
            List<SelectListItem> platforms = new List<SelectListItem>();

            switch (division)
            {
                case "MA":
                    platforms.Add(new SelectListItem { Value = "TEC1", Text = "TEC1" });
                    platforms.Add(new SelectListItem { Value = "TEC2", Text = "TEC2" });
                    platforms.Add(new SelectListItem { Value = "TEC4I", Text = "TEC4I" });
                    break;
                case "TC":
                    platforms.Add(new SelectListItem { Value = "TEC1", Text = "TEC1" });
                    platforms.Add(new SelectListItem { Value = "TEC3", Text = "TEC3" });
                    platforms.Add(new SelectListItem { Value = "TEC5", Text = "TEC5" });
                    break;
                default:
                    platforms.Add(new SelectListItem { Value = "TEC1", Text = "TEC1" });
                    break;
            }

            return platforms;
        }

        /// <summary>
        /// Updates the available platforms based on the selected division
        /// </summary>
        public void UpdatePlatformsForDivision()
        {
            AvailablePlatforms = GetPlatformsForDivision(Division);

            // Reset platform to a valid option if current selection is not valid for the division
            bool isValidPlatform = AvailablePlatforms.Any(p => p.Value == Platform);
            if (!isValidPlatform && AvailablePlatforms.Any())
            {
                Platform = AvailablePlatforms.First().Value;
            }
        }
    }
}