namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// Data transfer object for a search suggestion
    /// </summary>
    public class SearchSuggestionDto
    {
        /// <summary>
        /// Gets or sets the suggestion text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the type of suggestion (e.g., "Pallet", "Item", "Order")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the URL for the suggestion if selected
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the ID of the entity
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a "view all" suggestion
        /// </summary>
        public bool IsViewAll { get; set; }
    }
}