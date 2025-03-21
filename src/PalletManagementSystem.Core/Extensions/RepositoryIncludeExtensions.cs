using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Extensions
{
    /// <summary>
    /// Extension methods for standardized entity include patterns
    /// </summary>
    public static class RepositoryIncludeExtensions
    {
        /// <summary>
        /// Includes the Items collection on a Pallet query
        /// </summary>
        /// <param name="query">The query to extend</param>
        /// <returns>The query with Items included</returns>
        public static IQueryable<Pallet> IncludeItems(this IQueryable<Pallet> query)
        {
            return query.Include(p => p.Items);
        }

        /// <summary>
        /// Includes the Pallet navigation property on an Item query
        /// </summary>
        /// <param name="query">The query to extend</param>
        /// <returns>The query with Pallet included</returns>
        public static IQueryable<Item> IncludePallet(this IQueryable<Item> query)
        {
            return query.Include(i => i.Pallet);
        }

        /// <summary>
        /// Includes the Pallet navigation property and the Pallet's Items collection on an Item query
        /// </summary>
        /// <param name="query">The query to extend</param>
        /// <returns>The query with Pallet and Pallet's Items included</returns>
        public static IQueryable<Item> IncludePalletWithItems(this IQueryable<Item> query)
        {
            return query.Include(i => i.Pallet)
                        .ThenInclude(p => p.Items);
        }
    }
}