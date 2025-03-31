using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Core.Models.Enums;
using PalletManagementSystem.Core.Models.ValueObjects;
using PalletManagementSystem.Infrastructure.Data.ValueConverters;
using System;

namespace PalletManagementSystem.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the Pallet entity
    /// </summary>
    public class PalletConfiguration : IEntityTypeConfiguration<Pallet>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Pallet> builder)
        {
            builder.ToTable("Pallets");

            builder.HasKey(p => p.Id);

            // Configure properties
            builder.Property(p => p.Id)
                .ValueGeneratedOnAdd();

            // Configure PalletNumber value object using a value converter
            builder.Property(p => p.PalletNumber)
                .HasConversion(new PalletNumberConverter())
                .HasColumnName("PalletNumber")
                .HasMaxLength(50)
                .IsRequired();

            // Configure other scalar properties
            builder.Property(p => p.ManufacturingOrder)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Division)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.Platform)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.UnitOfMeasure)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.Quantity)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.IsClosed)
                .IsRequired();

            builder.Property(p => p.CreatedDate)
                .IsRequired();

            builder.Property(p => p.ClosedDate)
                .IsRequired(false);

            builder.Property(p => p.CreatedBy)
                .HasMaxLength(50)
                .IsRequired();

            
            builder.HasMany(p => p.Items) // Navigation property on Pallet
                                    .WithOne(i => i.Pallet) // Inverse navigation property on Item
                                    .HasForeignKey(i => i.PalletId) // Foreign key property on Item
                                    .OnDelete(DeleteBehavior.Cascade); // Cascade delete configuration

            // Find the navigation metadata using the public property name
            var itemsNavigation = builder.Metadata.FindNavigation(nameof(Pallet.Items));
            // Set the access mode to Field, telling EF to use the backing field (_items)
            itemsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

            // --- Optional: Ignore calculated properties ---
            builder.Ignore(p => p.ItemCount);



        }
    }
}