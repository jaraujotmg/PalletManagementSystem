using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PalletManagementSystem.Core.Interfaces.Repositories
{
    /// <summary>
    /// Enhanced specification pattern for creating reusable, composable queries
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Gets the criteria expression to filter entities
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Gets the list of include expressions for eager loading
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Gets the list of string-based include paths for eager loading
        /// </summary>
        List<string> IncludeStrings { get; }

        /// <summary>
        /// Gets the ordering expression for the query
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// Gets the descending ordering expression for the query
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }

        /// <summary>
        /// Gets a value indicating whether pagination is enabled
        /// </summary>
        bool IsPagingEnabled { get; }

        /// <summary>
        /// Gets the number of entities to skip
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Gets the number of entities to take
        /// </summary>
        int Take { get; }
    }
}