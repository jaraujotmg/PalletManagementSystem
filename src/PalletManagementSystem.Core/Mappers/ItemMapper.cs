using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.DTOs;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Core.Mappers
{
    /// <summary>
    /// Provides mapping methods and projections for Item entities
    /// </summary>
    public static class ItemMapper
    {
        /// <summary>
        /// Maps an Item entity to an ItemDto
        /// </summary>
        /// <param name="item">The item entity</param>
        /// <returns>The item DTO</returns>
        public static ItemDto ToDto(Item item)
        {
            if (item == null)
                return null;

            return new ItemDto
            {
                Id = item.Id,
                ItemNumber = item.ItemNumber,
                PalletId = item.PalletId,
                ManufacturingOrder = item.ManufacturingOrder,
                ManufacturingOrderLine = item.ManufacturingOrderLine,
                ServiceOrder = item.ServiceOrder,
                ServiceOrderLine = item.ServiceOrderLine,
                FinalOrder = item.FinalOrder,
                FinalOrderLine = item.FinalOrderLine,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                Reference = item.Reference,
                Finish = item.Finish,
                Color = item.Color,
                Quantity = item.Quantity,
                QuantityUnit = item.QuantityUnit,
                Weight = item.Weight,
                WeightUnit = item.WeightUnit,
                Width = item.Width,
                WidthUnit = item.WidthUnit,
                Quality = item.Quality,
                Batch = item.Batch,
                CreatedDate = item.CreatedDate,
                CreatedBy = item.CreatedBy
            };
        }

        /// <summary>
        /// Maps an Item entity to an ItemDto including its pallet
        /// </summary>
        /// <param name="item">The item entity</param>
        /// <returns>The item DTO with pallet</returns>
        public static ItemDto ToDtoWithPallet(Item item)
        {
            if (item == null)
                return null;

            var dto = ToDto(item);

            // Add pallet if exists
            if (item.Pallet != null)
            {
                dto.Pallet = PalletMapper.ToDto(item.Pallet);
            }

            return dto;
        }

        /// <summary>
        /// Maps a collection of Item entities to a collection of ItemDto objects
        /// </summary>
        /// <param name="items">The item entities</param>
        /// <returns>The item DTOs</returns>
        public static IEnumerable<ItemDto> ToDtoList(IEnumerable<Item> items)
        {
            return items?.Select(ToDto);
        }

        /// <summary>
        /// Maps a collection of Item entities to a collection of ItemDto objects including their pallets
        /// </summary>
        /// <param name="items">The item entities</param>
        /// <returns>The item DTOs with pallets</returns>
        public static IEnumerable<ItemDto> ToDtoWithPalletList(IEnumerable<Item> items)
        {
            return items?.Select(ToDtoWithPallet);
        }

        /// <summary>
        /// Creates a projection expression to map Item entities to ItemDto objects
        /// </summary>
        /// <returns>The projection expression</returns>
        public static Expression<Func<Item, ItemDto>> ProjectToDto()
        {
            return i => new ItemDto
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
                CreatedBy = i.CreatedBy
            };
        }

        /// <summary>
        /// Applies the projection to a queryable of Item entities
        /// </summary>
        /// <param name="query">The queryable to project</param>
        /// <returns>A queryable of ItemDto objects</returns>
        public static IQueryable<ItemDto> ProjectToDto(this IQueryable<Item> query)
        {
            return query.Select(ProjectToDto());
        }

        /// <summary>
        /// Creates a projection expression to map Item entities to ItemDto objects including pallets
        /// </summary>
        /// <returns>The projection expression</returns>
        public static Expression<Func<Item, ItemDto>> ProjectToDtoWithPallet()
        {
            return i => new ItemDto
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
                Pallet = i.Pallet == null ? null : new PalletDto
                {
                    Id = i.Pallet.Id,
                    PalletNumber = EF.Property<string>(i.Pallet, "_palletNumberValue"),
                    IsTemporary = EF.Property<bool>(i.Pallet, "_isTemporaryPalletNumber"),
                    ManufacturingOrder = i.Pallet.ManufacturingOrder,
                    Division = i.Pallet.Division.ToString(),
                    Platform = i.Pallet.Platform.ToString(),
                    UnitOfMeasure = i.Pallet.UnitOfMeasure.ToString(),
                    Quantity = i.Pallet.Quantity,
                    ItemCount = i.Pallet.ItemCount,
                    IsClosed = i.Pallet.IsClosed,
                    CreatedDate = i.Pallet.CreatedDate,
                    ClosedDate = i.Pallet.ClosedDate,
                    CreatedBy = i.Pallet.CreatedBy
                }
            };
        }

        /// <summary>
        /// Applies the projection to a queryable of Item entities to include pallets
        /// </summary>
        /// <param name="query">The queryable to project</param>
        /// <returns>A queryable of ItemDto objects with pallets</returns>
        public static IQueryable<ItemDto> ProjectToDtoWithPallet(this IQueryable<Item> query)
        {
            return query.Include(i => i.Pallet).Select(ProjectToDtoWithPallet());
        }

        /// <summary>
        /// Projects to a paged result of ItemDto objects
        /// </summary>
        /// <param name="query">The queryable to project</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includePallets">Whether to include pallets</param>
        /// <returns>The paged result</returns>
        public static async Task<PagedResultDto<ItemDto>> ProjectToPagedResultAsync(
            this IQueryable<Item> query,
            int pageNumber,
            int pageSize,
            bool includePallets = false)
        {
            var totalCount = await query.CountAsync();
            var skip = (pageNumber - 1) * pageSize;

            var projectedQuery = includePallets
                ? query.Skip(skip).Take(pageSize).ProjectToDtoWithPallet()
                : query.Skip(skip).Take(pageSize).ProjectToDto();

            var items = await projectedQuery.ToListAsync();

            return new PagedResultDto<ItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}