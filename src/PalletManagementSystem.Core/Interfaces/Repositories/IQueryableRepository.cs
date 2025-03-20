using System.Linq;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Interface for repositories that support direct queryable access
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IQueryableRepository<T> where T : class
    {
        /// <summary>
        /// Gets a queryable view of the repository
        /// </summary>
        IQueryable<T> Queryable { get; }
    }
}