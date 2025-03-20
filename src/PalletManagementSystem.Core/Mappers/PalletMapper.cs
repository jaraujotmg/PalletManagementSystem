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
    /// Provides mapping methods and projections for Pallet entities
    /// </summary>
    public static class PalletMapper
    {
        /// <summary>
        /// Maps a Pallet entity to a PalletDto
        /// </summary>
        /// <param name="pallet">The pallet entity</param>
        /// <returns>The pallet DTO</returns>
        public static PalletDto ToDto(Pallet pallet)
        {
            if (pallet == null)
                return null;

            return new PalletDto
            {
                Id = pallet.Id,
                PalletNumber = pallet.PalletNumber.Value,
                IsTemporary = pallet.PalletNumber.IsTemporary,
                ManufacturingOrder = pallet.ManufacturingOrder,
                Division = pallet.Division.ToString(),
                Platform = pallet.Platform.ToString(),
                UnitOfMeasure = pallet.UnitOfMeasure.ToString(),
                Quantity = pallet.Quantity,
                ItemCount = pallet.ItemCount,
                IsClosed = pallet.IsClosed,
                CreatedDate = pallet.CreatedDate,
                ClosedDate = pallet.ClosedDate,
                CreatedBy = pallet.CreatedBy
            };
        }

        /// <summary>
        /// Maps a Pallet entity to a PalletDto including its items
        /// </summary>
        /// <param name="pallet">The pallet entity</param>
        /// <returns>The pallet DTO with items</returns>
        public static PalletDto ToDtoWithItems(Pallet pallet)
        {
            if (pallet == null)
                return null;

            var dto = ToDto(pallet);

            // Add items if they exist
            if (pallet.Items != null && pallet.Items.Any())
            {
                dto.Items = pallet.Items.Select(ItemMapper.ToDto).ToList();
            }

            return dto;
        }

        /// <summary>
        /// Maps a collection of Pallet entities to a collection of PalletDto objects
        /// </summary>
        /// <param name="pallets">The pallet entities</param>
        /// <returns>The pallet DTOs</returns>
        public static IEnumerable<PalletDto> ToDtoList(IEnumerable<Pallet> pallets)
        {
            return pallets?.Select(ToDto);
        }

        /// <summary>
        /// Maps a collection of Pallet entities to a collection of PalletDto objects including their items
        /// </summary>
        /// <param name="pallets">The pallet entities</param>
        /// <returns>The pallet DTOs with items</returns>
        public static IEnumerable<PalletDto> ToDtoWithItemsList(IEnumerable<Pallet> pallets)
        {
            return pallets?.Select(ToDtoWithItems);
        }

        /// <summary>
        /// Creates a projection expression to map Pallet entities to PalletDto objects
        /// </summary>
        /// <returns>The projection expression</returns>
        public static Expression<Func<Pallet, PalletDto>> ProjectToDto()
        {
            return p => new PalletDto
            {
                Id = p.Id,
                // Note: Accessing value objects directly doesn't work well with EF Core projections
                // We need to access the shadow properties instead
                PalletNumber = EF.Property<string>(p, "_palletNumberValue"),
                IsTemporary = EF.Property<bool>(p, "_isTemporaryPalletNumber"),
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
        /// Applies the projection to a queryable of Pallet entities
        /// </summary>
        /// <param name="query">The queryable to project</param>
        /// <returns>A queryable of PalletDto objects</returns>
        public static IQueryable<PalletDto> ProjectToDto(this IQueryable<Pallet> query)
        {
            return query.Select(ProjectToDto());
        }

        /// <summary>
        /// Projects a Pallet entity to a PalletDto with its items
        /// </summary>
        /// <returns>The projection expression</returns>
        public static Expression<Func<Pallet, PalletDto>> ProjectToDtoWithItems()
        {
            return p => new PalletDto
            {
                Id = p.Id,
                PalletNumber = EF.Property<string>(p, "_palletNumberValue"),
                IsTemporary = EF.Property<bool>(p, "_isTemporaryPalletNumber"),
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
                Items = p.Items.Select(i => new ItemDto
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
                }).ToList()
            };
        }

        /// <summary>
        /// Applies the projection to a queryable of Pallet entities to include items
        /// </summary>
        /// <param name="query">The queryable to project</param>
        /// <returns>A queryable of PalletDto objects with items</returns>
        public static IQueryable<PalletDto> ProjectToDtoWithItems(this IQueryable<Pallet> query)
        {
            return query.Include(p => p.Items).Select(ProjectToDtoWithItems());
        }

        /// <summary>
        /// Projects to a paged result of PalletDto objects
        /// </summary>
        /// <param name="query">The queryable to project</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeItems">Whether to include items</param>
        /// <returns>The paged result</returns>
        public static async Task<PagedResultDto<PalletDto>> ProjectToPagedResultAsync(
            this IQueryable<Pallet> query,
            int pageNumber,
            int pageSize,
            bool includeItems = false)
        {
            var totalCount = await query.CountAsync();
            var skip = (pageNumber - 1) * pageSize;

            var projectedQuery = includeItems
                ? query.Skip(skip).Take(pageSize).ProjectToDtoWithItems()
                : query.Skip(skip).Take(pageSize).ProjectToDto();

            var items = await projectedQuery.ToListAsync();

            return new PagedResultDto<PalletDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}