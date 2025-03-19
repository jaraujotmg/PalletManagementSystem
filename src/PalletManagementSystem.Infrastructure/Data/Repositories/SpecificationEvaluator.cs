using System.Linq;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Evaluates a specification and applies it to a query
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class SpecificationEvaluator<T> where T : class
    {
        /// <summary>
        /// Gets a query with the specification applied
        /// </summary>
        /// <param name="inputQuery">The input query</param>
        /// <param name="specification">The specification</param>
        /// <returns>The query with the specification applied</returns>
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> specification)
        {
            var query = inputQuery;

            // Apply criteria if provided
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Include all expression-based includes
            query = specification.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            // Include all string-based includes
            query = specification.IncludeStrings.Aggregate(query,
                (current, include) => current.Include(include));

            // Apply ordering if expressions are provided
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Apply paging if enabled
            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip).Take(specification.Take);
            }

            return query;
        }
    }
}