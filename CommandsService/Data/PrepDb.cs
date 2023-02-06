using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder builder){
            using(var serviceScope = builder.ApplicationServices.CreateScope())
            {
                var grpcClient= serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                var platforms = grpcClient.ReturnAllPlatforms();

                SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms){
            System.Console.WriteLine("--> Seeding new platforms...");

            foreach (var platform in platforms)
            {
                if(!repo.ExternalPlatformExist(platform.ExternalId)){
                    repo.CreatePlatform(platform);
                }
            }
            repo.SaveChanges();
        }
    }
}