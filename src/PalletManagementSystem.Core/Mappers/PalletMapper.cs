using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Mappers
{
    /// <summary>
    /// Provides mapping expressions and extension methods for Pallet entities
    /// </summary>
    public static class PalletMapper
    {
        /// <summary>
        /// Creates a projection expression to map Pallet entities to PalletDto objects
        /// </summary>
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
                ItemCount = p.Items.Count,
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy
            };
        }

        /// <summary>
        /// Creates a projection expression to map Pallet entities to PalletListDto objects
        /// </summary>
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
                ItemCount = p.Items.Count,
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy
            };
        }

        /// <summary>
        /// Creates a projection expression to map Pallet entities to PalletDetailDto objects
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
                ItemCount = p.Items.Count,
                IsClosed = p.IsClosed,
                CreatedDate = p.CreatedDate,
                ClosedDate = p.ClosedDate,
                CreatedBy = p.CreatedBy,
                Items = p.Items.Select(i => new ItemListDto
                {
                    Id = i.Id,
                    ItemNumber = i.ItemNumber,
                    PalletId = i.PalletId,
                    ManufacturingOrder = i.ManufacturingOrder,
                    ClientCode = i.ClientCode,
                    ClientName = i.ClientName,
                    Quantity = i.Quantity,
                    QuantityUnit = i.QuantityUnit,
                    Weight = i.Weight,
                    WeightUnit = i.WeightUnit,
                    Width = i.Width,
                    WidthUnit = i.WidthUnit,
                    CreatedDate = i.CreatedDate
                }).ToList()
            };
        }

        /// <summary>
        /// Applies the DTO projection to a queryable of Pallet entities
        /// </summary>
        public static IQueryable<PalletDto> ProjectToDto(this IQueryable<Pallet> query)
        {
            return query.Select(ProjectToDto());
        }

        /// <summary>
        /// Applies the list projection to a queryable of Pallet entities
        /// </summary>
        public static IQueryable<PalletListDto> ProjectToListDto(this IQueryable<Pallet> query)
        {
            return query.Select(ProjectToListDto());
        }

        /// <summary>
        /// Applies the detail projection to a queryable of Pallet entities
        /// </summary>
        public static IQueryable<PalletDetailDto> ProjectToDetailDto(this IQueryable<Pallet> query)
        {
            return query.Include(p => p.Items).Select(ProjectToDetailDto());
        }

        /// <summary>
        /// Converts a list of Pallet list DTOs to an enumerable of PalletDto
        /// </summary>
        public static IEnumerable<PalletDto> ToDto(this IReadOnlyList<PalletListDto> palletListDtos)
        {
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
            });
        }

        /// <summary>
        /// Converts a queryable of Pallet entities to an IEnumerable of PalletDto
        /// </summary>
        public static IEnumerable<PalletDto> ToDto(this IQueryable<Pallet> query)
        {
            return query.Select(ProjectToDto());
        }
    }
}