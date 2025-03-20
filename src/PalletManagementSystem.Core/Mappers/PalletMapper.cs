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
    /// Provides mapping expressions for Pallet entities
    /// </summary>
    public static class PalletMapper
    {
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
    }
}