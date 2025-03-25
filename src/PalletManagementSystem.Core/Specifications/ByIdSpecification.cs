using System;
using System.Linq.Expressions;

namespace PalletManagementSystem.Core.Specifications
{
    /// <summary>
    /// A specification for filtering entities by ID
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public class ByIdSpecification<T> : BaseSpecification<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByIdSpecification{T}"/> class
        /// </summary>
        /// <param name="id">The entity ID</param>
        public ByIdSpecification(int id)
            : base(CreateIdPredicate(id))
        {
        }

        /// <summary>
        /// Creates a predicate expression to filter by ID
        /// </summary>
        /// <param name="id">The ID to filter by</param>
        /// <returns>A predicate expression</returns>
        private static Expression<Func<T, bool>> CreateIdPredicate(int id)
        {
            // Use reflection to get the Id property
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "Id");
            var value = Expression.Constant(id);
            var equals = Expression.Equal(property, value);
            return Expression.Lambda<Func<T, bool>>(equals, parameter);
        }
    }
}