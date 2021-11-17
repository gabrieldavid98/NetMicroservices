using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlatformService.Models;

namespace PlatformService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            SeedData(
                context: serviceScope.ServiceProvider.GetService<AppDbContext>(),
                isProd: isProd, 
                logger: serviceScope.ServiceProvider.GetService<ILogger<Startup>>()
            );
        }

        private static void SeedData(AppDbContext context, bool isProd, ILogger logger)
        {
            if (isProd)
            {
                logger.LogInformation("--> Attempting to apply migrations...");
                
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    logger.LogError($"--> Could not run migrations: {ex.Message}");
                }
            }
            
            if (context.Platforms.Any())
            {
                logger.LogInformation("--> We already have data");
                return;
            }

            logger.LogInformation("--> Seeding Data...");
            context.Platforms.AddRange(
                new Platform() {Name = "Dot Net", Publisher = "Microsoft", Cost = "Free"},
                new Platform() {Name = "SQL Server Express", Publisher = "Microsoft", Cost = "Free"},
                new Platform() {Name = "Kubernetes", Publisher = "Cloud Native Computing Foundation", Cost = "Free"}
            );

            context.SaveChanges();
        }
    }
}