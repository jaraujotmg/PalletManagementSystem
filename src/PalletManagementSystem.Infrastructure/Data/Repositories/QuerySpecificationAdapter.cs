using System.Collections.Generic;
using System.Linq;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Adapter to apply specifications to IQuery objects
    /// </summary>
    public static class QuerySpecificationAdapter
    {
        /// <summary>
        /// Applies a specification to a query
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">The query</param>
        /// <param name="spec">The specification</param>
        /// <returns>A query with the specification applied</returns>
        public static IQuery<T> ApplySpecification<T>(this IQuery<T> query, ISpecification<T> spec) where T : class
        {
            // Apply criteria
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            // Apply includes
            foreach (var include in spec.Includes)
            {
                query = query.Include(include);
            }

            foreach (var includeString in spec.IncludeStrings)
            {
                query = query.Include(includeString);
            }

            // Note: For paging, ordering, etc. we rely on the underlying implementation
            // These would need to be added to the IQuery interface if needed

            return query;
        }
    }
}