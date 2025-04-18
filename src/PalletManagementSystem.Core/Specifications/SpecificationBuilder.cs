﻿using System;
using PalletManagementSystem.Core.Interfaces.Repositories;

namespace PalletManagementSystem.Core.Specifications
{
    /// <summary>
    /// Utility class for creating specifications
    /// </summary>
    public static class SpecificationBuilder
    {
        /// <summary>
        /// Creates a specification for an entity by ID
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="id">The entity ID</param>
        /// <returns>The specification</returns>
        public static ISpecification<T> ById<T>(int id) where T : class
        {
            return new ByIdSpecification<T>(id);
        }

        /// <summary>
        /// Creates a specification for paging
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="pageNumber">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>The specification</returns>
        public static ISpecification<T> Paged<T>(int pageNumber, int pageSize) where T : class
        {
            // Create a specification that applies paging
            return new PagedSpecification<T>(pageNumber, pageSize);
        }
    }
}