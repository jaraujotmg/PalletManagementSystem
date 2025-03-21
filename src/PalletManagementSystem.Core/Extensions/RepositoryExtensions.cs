using System.Linq;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Core.Extensions
{
    /// <summary>
    /// Extension methods for repositories
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Creates a query from the repository
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="repository">The repository</param>
        /// <returns>A query for the entities</returns>
        public static IQuery<T> AsQuery<T>(this IRepository<T> repository) where T : class
        {
            return repository.GetQuery();
        }

        /// <summary>
        /// Creates a queryable view of the repository (for backward compatibility)
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="repository">The repository</param>
        /// <returns>A queryable of the entities</returns>
        public static IQueryable<T> AsQueryable<T>(this IRepository<T> repository) where T : class
        {
            return repository.GetQueryable();
        }
    }
}