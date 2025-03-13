namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// Data transfer object for a search result
    /// </summary>
    public class SearchResultDto
    {
        /// <summary>
        /// Gets or sets the ID of the entity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of entity (e.g., "Pallet", "Item")
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the primary identifier (e.g., pallet number, item number)
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the additional information (e.g., manufacturing order)
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Gets or sets the URL to view the entity
        /// </summary>
        public string ViewUrl { get; set; }
    }
}