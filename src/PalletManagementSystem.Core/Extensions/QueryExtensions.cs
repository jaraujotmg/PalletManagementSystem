using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Core.Extensions
{
    /// <summary>
    /// Extension methods for IQuery to simplify usage
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Includes multiple related entities in the query
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">The query</param>
        /// <param name="includes">The include paths</param>
        /// <returns>The query with includes applied</returns>
        public static IQuery<T> IncludeMultiple<T>(this IQuery<T> query, IEnumerable<string> includes) where T : class
        {
            if (includes == null)
                return query;

            IQuery<T> result = query;
            foreach (var include in includes)
            {
                if (!string.IsNullOrWhiteSpace(include))
                {
                    result = result.Include(include);
                }
            }

            return result;
        }

        /// <summary>
        /// Includes multiple related entities in the query using expressions
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">The query</param>
        /// <param name="includes">The include expressions</param>
        /// <returns>The query with includes applied</returns>
        public static IQuery<T> IncludeMultiple<T>(this IQuery<T> query, IEnumerable<Expression<Func<T, object>>> includes) where T : class
        {
            if (includes == null)
                return query;

            IQuery<T> result = query;
            foreach (var include in includes)
            {
                result = result.Include(include);
            }

            return result;
        }
    }
}