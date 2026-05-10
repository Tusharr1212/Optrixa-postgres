using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Optrixa.Domain.Interfaces;
using Optrixa.Infrastructure.Identity;
using Optrixa.Infrastructure.Persistence;
using Optrixa.Infrastructure.Persistence.Repositories;

namespace Optrixa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ── Database — PostgreSQL ──────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(                           // ← changed
                config.GetConnectionString("DefaultConnection")));

        // ── Identity ──────────────────────────────────────────
        services.AddIdentity<OptrixaUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ── JWT Settings ──────────────────────────────────────
        var tokenSettings = new TokenSettings();
        config.GetSection(nameof(TokenSettings)).Bind(tokenSettings);
        services.AddSingleton(tokenSettings);
        services.AddScoped<JwtService>();

        // ── Repositories ──────────────────────────────────────
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}