using System;
using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using var serviceScope = applicationBuilder.ApplicationServices.CreateScope();
            var platformDataClient = serviceScope.ServiceProvider.GetRequiredService<IPlatformDataClient>();
            var platforms = platformDataClient.ReturnAllPlatforms();

            var commandRepository = serviceScope.ServiceProvider.GetRequiredService<ICommandRepository>();
            SeedData(commandRepository, platforms);
        }

        private static void SeedData(ICommandRepository commandRepository, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding platforms data");
            foreach (var platform in platforms)
            {
                if (commandRepository.ExternalPlatformExists(platform.ExternalId))
                {
                    continue;
                }
                
                commandRepository.CreatePlatform(platform);
            }

            commandRepository.SaveChanges();
        }
    }
}