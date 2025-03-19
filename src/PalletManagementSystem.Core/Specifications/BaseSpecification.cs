using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Core.Specifications
{
    /// <summary>
    /// Base class for specifications
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T>
    {
        /// <inheritdoc/>
        public Expression<Func<T, bool>> Criteria { get; private set; }

        /// <inheritdoc/>
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        /// <inheritdoc/>
        public List<string> IncludeStrings { get; } = new List<string>();

        /// <inheritdoc/>
        public Expression<Func<T, object>> OrderBy { get; private set; }

        /// <inheritdoc/>
        public Expression<Func<T, object>> OrderByDescending { get; private set; }

        /// <inheritdoc/>
        public bool IsPagingEnabled { get; private set; }

        /// <inheritdoc/>
        public int Skip { get; private set; }

        /// <inheritdoc/>
        public int Take { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSpecification{T}"/> class
        /// </summary>
        protected BaseSpecification()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSpecification{T}"/> class
        /// </summary>
        /// <param name="criteria">The criteria expression</param>
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        /// <summary>
        /// Adds an include expression to the specification
        /// </summary>
        /// <param name="includeExpression">The include expression</param>
        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Adds a string-based include path to the specification
        /// </summary>
        /// <param name="includeString">The include string</param>
        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        /// <summary>
        /// Applies paging to the specification
        /// </summary>
        /// <param name="skip">The number of entities to skip</param>
        /// <param name="take">The number of entities to take</param>
        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        /// <summary>
        /// Applies ascending ordering to the specification
        /// </summary>
        /// <param name="orderByExpression">The ordering expression</param>
        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        /// <summary>
        /// Applies descending ordering to the specification
        /// </summary>
        /// <param name="orderByDescendingExpression">The ordering expression</param>
        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }
    }
}