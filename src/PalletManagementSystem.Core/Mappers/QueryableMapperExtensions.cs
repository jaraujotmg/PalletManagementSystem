using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Core.Mappers
{
    /// <summary>
    /// Extensions for mapping IQuery results to DTOs
    /// </summary>
    public static class QueryableMapperExtensions
    {
        /// <summary>
        /// Projects a query to a list of DTOs
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TDto">The DTO type</typeparam>
        /// <param name="query">The query</param>
        /// <param name="selector">The selector expression</param>
        /// <returns>A list of mapped DTOs</returns>
        public static IReadOnlyList<TDto> ProjectToList<TEntity, TDto>(
            this IQuery<TEntity> query,
            Expression<Func<TEntity, TDto>> selector) where TEntity : class
        {
            // Apply the selector to entities after they're loaded
            // This is a simplified version that loads entities first then maps
            var entities = query.ToList();
            var selectorCompiled = selector.Compile();
            return entities.Select(selectorCompiled).ToList();
        }

        /// <summary>
        /// Projects a query to a DTO
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TDto">The DTO type</typeparam>
        /// <param name="query">The query</param>
        /// <param name="selector">The selector expression</param>
        /// <returns>The mapped DTO</returns>
        public static TDto ProjectToFirst<TEntity, TDto>(
            this IQuery<TEntity> query,
            Expression<Func<TEntity, TDto>> selector) where TEntity : class
        {
            // Apply the selector to the entity after it's loaded
            var entity = query.FirstOrDefault();
            if (entity == null)
                return default;

            var selectorCompiled = selector.Compile();
            return selectorCompiled(entity);
        }
    }
}