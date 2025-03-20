namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// DTO for updating an item's editable properties
    /// </summary>
    public class UpdateItemDto
    {
        /// <summary>
        /// Gets or sets the weight
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        public decimal Width { get; set; }

        /// <summary>
        /// Gets or sets the quality
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        /// Gets or sets the batch
        /// </summary>
        public string Batch { get; set; }
    }
}