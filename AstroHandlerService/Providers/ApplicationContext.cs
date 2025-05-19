using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstroHandlerService.Configurations;
using AstroHandlerService.Entities.Db;

namespace AstroHandlerService.Providers
{
    public class ApplicationContext : DbContext
    {
        private PostgresConfig _postgresConfig;

        private bool _isEphemerisSetInit;
        public DbSet<EphemerisDb> EphemerisSet { get; }

        //public DbSet<GoodModel> Goods => Set<GoodModel>();

        public ApplicationContext(IOptions<PostgresConfig> postgresConfig)
        {
            _postgresConfig = postgresConfig.Value;
            EphemerisSet = Set<EphemerisDb>();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_postgresConfig.ConnectionString);
        }
    }
}
