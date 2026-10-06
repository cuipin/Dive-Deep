using Dive_Deep.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep.Data;

public class Dive_DeepContext : IdentityDbContext<ApplicationUser>
{
    public Dive_DeepContext(DbContextOptions<Dive_DeepContext> options) : base(options)
    {
    }

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<EquipmentUnit> EquipmentUnits => Set<EquipmentUnit>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<BookingAllocation> BookingAllocations => Set<BookingAllocation>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(category => category.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Brand).HasMaxLength(120).IsRequired();
            entity.Property(product => product.Model).HasMaxLength(160).IsRequired();
            entity.Property(product => product.ImageFileName).HasMaxLength(260);
            entity.HasIndex(product => new { product.ProductCategoryId, product.Brand, product.Model }).IsUnique();
            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.Property(variant => variant.OptionLabel).HasMaxLength(160).IsRequired();
            entity.Property(variant => variant.Size).HasMaxLength(40);
            entity.Property(variant => variant.Gender).HasMaxLength(40);
            entity.Property(variant => variant.EquipmentType).HasMaxLength(60);
            entity.Property(variant => variant.ThicknessMm).HasPrecision(8, 2);
            entity.Property(variant => variant.VolumeLiters).HasPrecision(8, 2);
            entity.Property(variant => variant.FirstStage).HasMaxLength(100);
            entity.Property(variant => variant.SecondStage).HasMaxLength(100);
            entity.Property(variant => variant.Octopus).HasMaxLength(100);
            entity.Property(variant => variant.DailyRate).HasPrecision(10, 2);
            entity.HasIndex(variant => new { variant.ProductId, variant.OptionLabel }).IsUnique();
            entity.HasOne(variant => variant.Product)
                .WithMany(product => product.Variants)
                .HasForeignKey(variant => variant.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EquipmentUnit>(entity =>
        {
            entity.Property(unit => unit.AssetTag).HasMaxLength(64).IsRequired();
            entity.Property(unit => unit.Status).HasConversion<string>().HasMaxLength(24);
            entity.HasIndex(unit => unit.AssetTag).IsUnique();
            entity.HasIndex(unit => new { unit.ProductVariantId, unit.Status });
            entity.HasOne(unit => unit.ProductVariant)
                .WithMany(variant => variant.EquipmentUnits)
                .HasForeignKey(unit => unit.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(booking => booking.UserId).HasMaxLength(450).IsRequired();
            entity.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(booking => booking.CreatedAtUtc).HasColumnType("datetimeoffset");
            entity.Property(booking => booking.RowVersion).IsRowVersion().IsConcurrencyToken();
            entity.HasIndex(booking => new { booking.UserId, booking.CreatedAtUtc });
            entity.HasOne(booking => booking.User)
                .WithMany(user => user.Bookings)
                .HasForeignKey(booking => booking.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingItem>(entity =>
        {
            entity.Property(item => item.StartTime).HasColumnType("datetimeoffset");
            entity.Property(item => item.EndTime).HasColumnType("datetimeoffset");
            entity.Property(item => item.DailyRateAtBooking).HasPrecision(10, 2);
            entity.ToTable(table => table.HasCheckConstraint("CK_BookingItems_Quantity_Positive", "[Quantity] > 0"));
            entity.ToTable(table => table.HasCheckConstraint("CK_BookingItems_TimeRange", "[EndTime] > [StartTime]"));
            entity.HasIndex(item => new { item.StartTime, item.EndTime });
            entity.HasOne(item => item.Booking)
                .WithMany(booking => booking.Items)
                .HasForeignKey(item => item.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.ProductVariant)
                .WithMany(variant => variant.BookingItems)
                .HasForeignKey(item => item.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookingAllocation>(entity =>
        {
            entity.HasIndex(allocation => allocation.EquipmentUnitId);
            entity.HasIndex(allocation => new { allocation.BookingItemId, allocation.EquipmentUnitId }).IsUnique();
            entity.HasOne(allocation => allocation.BookingItem)
                .WithMany(item => item.Allocations)
                .HasForeignKey(allocation => allocation.BookingItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(allocation => allocation.EquipmentUnit)
                .WithMany(unit => unit.Allocations)
                .HasForeignKey(allocation => allocation.EquipmentUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.Property(cart => cart.UserId).HasMaxLength(450).IsRequired();
            entity.HasIndex(cart => cart.UserId).IsUnique();
            entity.HasOne(cart => cart.User)
                .WithOne(user => user.Cart)
                .HasForeignKey<Cart>(cart => cart.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.Property(item => item.StartTime).HasColumnType("datetimeoffset");
            entity.Property(item => item.EndTime).HasColumnType("datetimeoffset");
            entity.ToTable(table => table.HasCheckConstraint("CK_CartItems_Quantity_Positive", "[Quantity] > 0"));
            entity.ToTable(table => table.HasCheckConstraint("CK_CartItems_TimeRange", "[EndTime] > [StartTime]"));
            entity.HasOne(item => item.Cart)
                .WithMany(cart => cart.Items)
                .HasForeignKey(item => item.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.ProductVariant)
                .WithMany(variant => variant.CartItems)
                .HasForeignKey(item => item.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
