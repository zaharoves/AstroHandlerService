using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AstroHandlerService.Configurations;
using AstroTBotService.Db.Entities;
using AstroHandlerService.Db.Entities;

namespace AstroHandlerService.Db.Providers
{
    public class ApplicationContext : DbContext
    {
        private PostgresConfig _postgresConfig;

        public DbSet<Ephemeris> Ephemerises { get; }
        public DbSet<User> Users { get; }
        public DbSet<UserStage> UsersStages { get; }

        public ApplicationContext(IOptions<PostgresConfig> postgresConfig)
        {
            _postgresConfig = postgresConfig.Value;
            Ephemerises = Set<Ephemeris>();
            Users = Set<User>();
            UsersStages = Set<UserStage>();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_postgresConfig.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<User>(entity => 
                { 
                    entity.Property(e => e.BirthDate)
                        .HasColumnType("timestamp without time zone"); 
                })
                .Entity<Ephemeris>(entity => 
                { 
                    entity.Property(e => e.DateTime)
                        .HasColumnType("timestamp without time zone"); 
                });
        }
    }
}
