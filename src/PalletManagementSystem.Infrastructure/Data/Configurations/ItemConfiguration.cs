using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PalletManagementSystem.Core.Models;

namespace PalletManagementSystem.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Entity Framework configuration for the Item entity
    /// </summary>
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");

            builder.HasKey(i => i.Id);

            // Configure properties
            builder.Property(i => i.Id)
                .ValueGeneratedOnAdd();

            builder.Property(i => i.ItemNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(i => i.PalletId)
                .IsRequired();

            builder.Property(i => i.ManufacturingOrder)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.ManufacturingOrderLine)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(i => i.ServiceOrder)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(i => i.ServiceOrderLine)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(i => i.FinalOrder)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(i => i.FinalOrderLine)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(i => i.ClientCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(i => i.ClientName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(i => i.Reference)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(i => i.Finish)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(i => i.Color)
                .HasMaxLength(50)
                .IsRequired(false);

            builder.Property(i => i.Quantity)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(i => i.QuantityUnit)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(i => i.Weight)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(i => i.WeightUnit)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(i => i.Width)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(i => i.WidthUnit)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(i => i.Quality)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.Batch)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.CreatedDate)
                .IsRequired();

            builder.Property(i => i.CreatedBy)
                .HasMaxLength(50)
                .IsRequired();

            // Configure navigation property
            builder.HasOne(i => i.Pallet)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique index for ItemNumber
            builder.HasIndex(i => i.ItemNumber)
                .IsUnique();
        }
    }
}