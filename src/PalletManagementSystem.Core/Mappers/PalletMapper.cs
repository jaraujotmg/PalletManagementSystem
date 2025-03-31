// src/PalletManagementSystem.Core/Mappers/PalletMapper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore; // Keep for Include/ThenInclude usage
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Mappers
{
    /// <summary>
    /// Provides mapping expressions and extension methods for Pallet entities
    /// </summary>
    public static class PalletMapper
    {
        // --- ProjectToDto remains the same ---
        public static Expression<Func<Pallet, PalletDto>> ProjectToDto()
        {
            return p => new PalletDto
            {
                Id = p.Id,
                PalletNumber = p.PalletNumber.Value,
                IsTemporary = p.PalletNumber.IsTemporary,
                ManufacturingOrder = p.ManufacturingOrder,
                Division = p.Division.ToString(),
                Platform = p.Platform.ToString(),
                UnitOfMeasure = p.UnitOfMeasure.ToString(),
                Quantity = p.Quantity,
                // Performance Note: Be mindful if using p.Items.Count here without Include
                ItemCount = p.Items.Count, // Requires Items to be loaded or counted efficiently by EF
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy
            };
        }

        // --- ProjectToListDto remains the same ---
        public static Expression<Func<Pallet, PalletListDto>> ProjectToListDto()
        {
            return p => new PalletListDto
            {
                Id = p.Id,
                PalletNumber = p.PalletNumber.Value,
                IsTemporary = p.PalletNumber.IsTemporary,
                ManufacturingOrder = p.ManufacturingOrder,
                Division = p.Division.ToString(),
                Platform = p.Platform.ToString(),
                UnitOfMeasure = p.UnitOfMeasure.ToString(),
                Quantity = p.Quantity,
                // Performance Note: Be mindful if using p.Items.Count here without Include
                ItemCount = p.Items.Count, // Requires Items to be loaded or counted efficiently by EF
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy
            };
        }

        // --- MODIFIED ProjectToDetailDto ---
        /// <summary>
        /// Creates a projection expression to map Pallet entities to PalletDetailDto objects,
        /// including detailed item information.
        /// </summary>
        public static Expression<Func<Pallet, PalletDetailDto>> ProjectToDetailDto()
        {
            return p => new PalletDetailDto
            {
                Id = p.Id,
                PalletNumber = p.PalletNumber.Value,
                IsTemporary = p.PalletNumber.IsTemporary,
                ManufacturingOrder = p.ManufacturingOrder,
                Division = p.Division.ToString(),
                Platform = p.Platform.ToString(),
                UnitOfMeasure = p.UnitOfMeasure.ToString(),
                Quantity = p.Quantity,
                ItemCount = p.Items.Count, // Okay here as Items are explicitly included
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy,
                // --- Project Items to ItemDetailDto ---
                Items = p.Items.Select(i => new ItemDetailDto
                {
                    // Map ALL fields from Item entity 'i' to ItemDetailDto
                    Id = i.Id,
                    ItemNumber = i.ItemNumber,
                    PalletId = i.PalletId,
                    ManufacturingOrder = i.ManufacturingOrder,
                    ManufacturingOrderLine = i.ManufacturingOrderLine,
                    ServiceOrder = i.ServiceOrder,
                    ServiceOrderLine = i.ServiceOrderLine,
                    FinalOrder = i.FinalOrder,
                    FinalOrderLine = i.FinalOrderLine,
                    ClientCode = i.ClientCode,
                    ClientName = i.ClientName,
                    Reference = i.Reference, // Include Reference
                    Finish = i.Finish,
                    Color = i.Color,
                    Quantity = i.Quantity,
                    QuantityUnit = i.QuantityUnit,
                    Weight = i.Weight,
                    WeightUnit = i.WeightUnit,
                    Width = i.Width,
                    WidthUnit = i.WidthUnit,
                    Quality = i.Quality,
                    Batch = i.Batch,
                    CreatedDate = i.CreatedDate,
                    CreatedBy = i.CreatedBy, // Include CreatedBy
                    // Pallet info within ItemDetailDto: provide basic info
                    // Avoid circular reference or deep nesting
                    Pallet = new PalletInfo { Id = p.Id, PalletNumber = p.PalletNumber.Value, IsClosed = p.IsClosed }
                }).ToList() // Materialize the list of ItemDetailDto
                // --------------------------------------
            };
        }
        // --- END MODIFIED ProjectToDetailDto ---


        // --- Extension methods ---

        /// <summary>
        /// Applies the DTO projection to a queryable of Pallet entities.
        /// </summary>
        public static IQueryable<PalletDto> ProjectToDto(this IQueryable<Pallet> query)
        {
            // Include Items if ItemCount calculation relies on it loading here
            // return query.Include(p => p.Items).Select(ProjectToDto());
            return query.Select(ProjectToDto()); // Assuming ItemCount can be calculated efficiently
        }

        /// <summary>
        /// Applies the list projection to a queryable of Pallet entities.
        /// </summary>
        public static IQueryable<PalletListDto> ProjectToListDto(this IQueryable<Pallet> query)
        {
            // Include Items if ItemCount calculation relies on it loading here
            // return query.Include(p => p.Items).Select(ProjectToListDto());
            return query.Select(ProjectToListDto()); // Assuming ItemCount can be calculated efficiently
        }

        /// <summary>
        /// Applies the detail projection (including detailed items) to a queryable of Pallet entities.
        /// Ensures Items navigation property is loaded before projecting.
        /// </summary>
        public static IQueryable<PalletDetailDto> ProjectToDetailDto(this IQueryable<Pallet> query)
        {
            // *** ENSURE Include(p => p.Items) is called here ***
            // This loads the related Item entities so the Select projection can access them.
            return query.Include(p => p.Items).Select(ProjectToDetailDto());
        }


        // --- ToDto extension methods (remain the same conceptually) ---
        public static IEnumerable<PalletDto> ToDto(this IReadOnlyList<PalletListDto> palletListDtos)
        {
            // Implement the mapping from PalletListDto to PalletDto if needed
            return palletListDtos.Select(p => new PalletDto
            {
                Id = p.Id,
                PalletNumber = p.PalletNumber,
                IsTemporary = p.IsTemporary,
                ManufacturingOrder = p.ManufacturingOrder,
                Division = p.Division,
                Platform = p.Platform,
                UnitOfMeasure = p.UnitOfMeasure,
                Quantity = p.Quantity,
                ItemCount = p.ItemCount,
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy
                // PalletDto does not have Items collection
            });
        }
        public static IEnumerable<PalletDto> ToDto(this IQueryable<Pallet> query)
        {
            return query.ProjectToDto().ToList(); // Materialize the query
        }
    }
}