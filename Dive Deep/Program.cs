using Dive_Deep.Data;
using Dive_Deep.Persistence;
using Dive_Deep.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found in appsettings.json.");
        builder.Services.AddDbContext<Dive_DeepContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<Dive_DeepContext>();

        builder.Services.AddScoped<IBookingRepository, BookingRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();

        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICartService, CartService>();

        var app = builder.Build();

        // Seeding is an explicit command and runs only after the developer applies migrations.
        if (args.Contains("--seed-catalog", StringComparer.OrdinalIgnoreCase))
        {
            await using var scope = app.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<Dive_DeepContext>();
            await SeedData.SeedCatalogAsync(db);
            return;
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapRazorPages();

        await app.RunAsync();
    }
}