namespace PalletManagementSystem.Core.DTOs
{
    /// <summary>
    /// Data transfer object for a client
    /// </summary>
    public class ClientDto
    {
        /// <summary>
        /// Gets or sets the client code
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// Gets or sets the client name
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a special client
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// Gets or sets the number of items for this client
        /// </summary>
        public int ItemCount { get; set; }
    }
}