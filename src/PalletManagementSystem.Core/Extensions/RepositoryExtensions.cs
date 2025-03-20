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
        /// Creates a queryable view of the repository
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="repository">The repository</param>
        /// <returns>A queryable of the entities</returns>
        public static IQueryable<T> AsQueryable<T>(this IRepository<T> repository) where T : class
        {
            var queryableRepo = repository as IQueryableRepository<T>;
            return queryableRepo?.Queryable ?? throw new System.NotSupportedException("Repository does not support queryable operations");
        }
    }
}