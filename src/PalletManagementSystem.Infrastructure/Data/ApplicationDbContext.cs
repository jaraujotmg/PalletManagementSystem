using Microsoft.EntityFrameworkCore;
using PalletManagementSystem.Core.Models;
using PalletManagementSystem.Infrastructure.Data.Configurations;

namespace PalletManagementSystem.Infrastructure.Data
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the pallets DbSet
        /// </summary>
        public DbSet<Pallet> Pallets { get; set; }

        /// <summary>
        /// Gets or sets the items DbSet
        /// </summary>
        public DbSet<Item> Items { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class
        /// </summary>
        /// <param name="options">The options to be used by the context</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfiguration(new PalletConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());

            // Add any additional configurations here
        }
    }
}