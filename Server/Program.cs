using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server;
using Server.Data;

// Maybe: Add Config to database in first time run application
var seed = args.Contains("/seed");
if (seed)
{
    args = args.Except(new[] { "seed" }).ToArray();
}

var builder = WebApplication.CreateBuilder(args);

// Get name assembly
var assembly = typeof(Program).Assembly.GetName().Name;
// Add IdentityServer service
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (seed)
{
    SeedData.EnsureSeedData(defaultConnectionString);
}

// Add services to the container.
// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(
    options => { options.UseNpgsql(defaultConnectionString, opt => opt.MigrationsAssembly(assembly)); }
);

// Add AspNet Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    // Add IdentityServer with AspNetIdentity
    .AddAspNetIdentity<IdentityUser>()
    // Store configuration and operational data in PostgreSQL
    .AddConfigurationStore(
        options =>
        {
            options.ConfigureDbContext = b =>
            {
                b.UseNpgsql(defaultConnectionString, opt => opt.MigrationsAssembly(assembly));
            };
        }
    )
    .AddOperationalStore(
        options =>
        {
            options.ConfigureDbContext = b =>
            {
                b.UseNpgsql(defaultConnectionString, opt => opt.MigrationsAssembly(assembly));
            };
        }
    )
    .AddDeveloperSigningCredential();

builder.Services.AddRazorPages();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

app.Run();