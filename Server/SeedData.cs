using System.Security.Claims;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.EntityFramework.Storage;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var service = new ServiceCollection();
            service.AddLogging();
            service.AddDbContext<ApplicationDbContext>(option => option.UseNpgsql(connectionString));

            service
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            service.AddOperationalDbContext(
                option =>
                    option.ConfigureDbContext = b => b.UseNpgsql(
                        connectionString, sql =>
                            sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName)
                    )
            );

            service.AddConfigurationDbContext(
                option =>
                    option.ConfigureDbContext = b => b.UseNpgsql(
                        connectionString, sql =>
                            sql.MigrationsAssembly(typeof(SeedData).Assembly.FullName)
                    )
            );
            var serviceProvider = service.BuildServiceProvider();
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context.Database.Migrate();

            EnsureSeedData(context);

            var ctx = scope.ServiceProvider.GetService<ApplicationDbContext>();
            ctx.Database.Migrate();
            EnsureUsers(scope);
        }

        // Finding User by the name
        private static void EnsureUsers(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var angella = userMgr.FindByNameAsync("angella").Result;
            if (angella == null)
            {
                angella = new IdentityUser()
                {
                    UserName = "angella",
                    Email = "angella.freeman@email.com",
                    EmailConfirmed = false
                };

                var result = userMgr.CreateAsync(angella, "Pass123$").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = userMgr.AddClaimsAsync(
                    angella, new Claim[]
                    {
                        new Claim(JwtClaimTypes.Name, "Angella Freeman"),
                        new Claim(JwtClaimTypes.GivenName, "Angella"),
                        new Claim(JwtClaimTypes.FamilyName, "Freeman"),
                    }
                ).Result;
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var apiScope in Config.ApiScopes)
                {
                    context.ApiScopes.Add(apiScope.ToEntity());
                }

                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var apiResource in Config.ApiResources)
                {
                    context.ApiResources.Add(apiResource.ToEntity());
                }

                context.SaveChanges();
            }
        }
    }
}