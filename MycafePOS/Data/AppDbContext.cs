using Microsoft.EntityFrameworkCore;
using MycafePOS.Entities;

namespace MycafePOS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Users> Users => Set<Users>();

    public DbSet<Menu> Menu => Set<Menu>();

    public DbSet<MenuType> MenuType => Set<MenuType>();

    public DbSet<Ingredients> Ingredients => Set<Ingredients>();

    public DbSet<IngredientsType> IngredientsType => Set<IngredientsType>();

    public DbSet<IngredientStock> IngredientStock => Set<IngredientStock>();

    public DbSet<MenuRecipe> MenuRecipe => Set<MenuRecipe>();

    public DbSet<Orders> Orders => Set<Orders>();

    public DbSet<OrderItems> OrderItems => Set<OrderItems>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredients>()
            .Property(i => i.IngredientsUnitType)
            .HasConversion<string>();

        modelBuilder.Entity<MenuRecipe>()
            .HasIndex(mr => new { mr.MenuId, mr.IngredientsId })
            .IsUnique();

        modelBuilder.Entity<OrderItems>()
           .HasOne(x => x.Order)
           .WithMany(x => x.OrderItems)
           .HasForeignKey(x => x.OrderId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}