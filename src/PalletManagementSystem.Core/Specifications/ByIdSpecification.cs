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
        {
            // Use reflection to get the Id property
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, "Id");
            var value = Expression.Constant(id);
            var equals = Expression.Equal(property, value);
            var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

            // Set the criteria
            Criteria = lambda;
        }
    }
}