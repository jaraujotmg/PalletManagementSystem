using System.Linq;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Infrastructure.Data
{
    /// <summary>
    /// Helper class to access the DbContext from repositories
    /// </summary>
    public static class DbContextAccessor
    {
        /// <summary>
        /// Gets the DbSet for an entity type from a repository
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="repository">The repository</param>
        /// <returns>The DbSet for the entity type</returns>
        public static DbSet<T> GetDbSet<T>(IRepository<T> repository) where T : class
        {
            // Get the context field using reflection
            var contextField = repository.GetType()
                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (contextField == null)
            {
                throw new System.InvalidOperationException(
                    $"Could not find _context field in repository of type {repository.GetType().Name}");
            }

            // Get the context instance
            var context = contextField.GetValue(repository) as DbContext;

            if (context == null)
            {
                throw new System.InvalidOperationException(
                    $"_context field in repository of type {repository.GetType().Name} is not a DbContext");
            }

            // Return the DbSet
            return context.Set<T>();
        }

        /// <summary>
        /// Gets the DbSet for an entity type from the unit of work
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="unitOfWork">The unit of work</param>
        /// <returns>The DbSet for the entity type</returns>
        public static DbSet<T> GetDbSet<T>(IUnitOfWork unitOfWork) where T : class
        {
            return GetDbSet(unitOfWork.Repository<T>());
        }

        /// <summary>
        /// Creates a queryable for an entity type from the unit of work
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="unitOfWork">The unit of work</param>
        /// <returns>A queryable for the entity type</returns>
        public static IQueryable<T> CreateQuery<T>(IUnitOfWork unitOfWork) where T : class
        {
            return GetDbSet<T>(unitOfWork).AsQueryable();
        }
    }
}