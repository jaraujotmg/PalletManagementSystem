using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace PalletManagementSystem.Core.Extensions
{
    /// <summary>
    /// Extension methods for IQueryable to standardize navigation property loading
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Applies a collection of string-based include paths to the query
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">The query to extend</param>
        /// <param name="includes">The collection of include paths</param>
        /// <returns>The query with includes applied</returns>
        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, IEnumerable<string> includes)
            where T : class
        {
            if (includes == null)
                return query;

            foreach (var include in includes)
            {
                if (!string.IsNullOrWhiteSpace(include))
                {
                    query = query.Include(include);
                }
            }

            return query;
        }

        /// <summary>
        /// Applies a collection of expression-based includes to the query
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">The query to extend</param>
        /// <param name="includes">The collection of include expressions</param>
        /// <returns>The query with includes applied</returns>
        public static IQueryable<T> IncludeMultiple<T>(this IQueryable<T> query, IEnumerable<Expression<Func<T, object>>> includes)
            where T : class
        {
            if (includes == null)
                return query;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }
    }
}