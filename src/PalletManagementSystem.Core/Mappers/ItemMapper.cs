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
    /// Provides mapping expressions for Item entities
    /// </summary>
    public static class ItemMapper
    {
        /// <summary>
        /// Creates a projection expression to map Item entities to ItemListDto objects
        /// </summary>
        public static Expression<Func<Item, ItemListDto>> ProjectToListDto()
        {
            return i => new ItemListDto
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
            };
        }

        /// <summary>
        /// Creates a projection expression to map Item entities to ItemDetailDto objects
        /// </summary>
        public static Expression<Func<Item, ItemDetailDto>> ProjectToDetailDto()
        {
            return i => new ItemDetailDto
            {
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
                Reference = i.Reference,
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
                CreatedBy = i.CreatedBy,
                Pallet = i.Pallet == null ? null : new PalletInfo
                {
                    Id = i.Pallet.Id,
                    PalletNumber = i.Pallet.PalletNumber.Value,
                    IsClosed = i.Pallet.IsClosed
                }
            };
        }

        /// <summary>
        /// Applies the list projection to a queryable of Item entities
        /// </summary>
        public static IQueryable<ItemListDto> ProjectToListDto(this IQueryable<Item> query)
        {
            return query.Select(ProjectToListDto());
        }

        /// <summary>
        /// Applies the detail projection to a queryable of Item entities
        /// </summary>
        public static IQueryable<ItemDetailDto> ProjectToDetailDto(this IQueryable<Item> query)
        {
            return query.Include(i => i.Pallet).Select(ProjectToDetailDto());
        }
    }
}