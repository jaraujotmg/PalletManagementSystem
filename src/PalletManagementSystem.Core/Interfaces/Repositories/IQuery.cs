using System;
using System.Linq.Expressions;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Persistence-ignorant queryable interface to remove direct dependency on EF Core's IQueryable
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IQuery<T> where T : class
    {
        /// <summary>
        /// Applies a filter to the query
        /// </summary>
        IQuery<T> Where(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Includes a related entity
        /// </summary>
        IQuery<T> Include(Expression<Func<T, object>> path);

        /// <summary>
        /// Includes a related entity using a string path
        /// </summary>
        IQuery<T> Include(string path);

        /// <summary>
        /// Executes the query and returns results
        /// </summary>
        System.Collections.Generic.IReadOnlyList<T> ToList();

        /// <summary>
        /// Executes the query and returns the first result
        /// </summary>
        T FirstOrDefault();
    }
}