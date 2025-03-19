namespace PalletManagementSystem.Core.Specifications
{
    /// <summary>
    /// A specification for paging
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class PagedSpecification<T> : BaseSpecification<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedSpecification{T}"/> class
        /// </summary>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        public PagedSpecification(int pageNumber, int pageSize)
        {
            // Skip (pageNumber - 1) * pageSize items and take pageSize items
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}