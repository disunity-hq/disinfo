using System.Reflection;

using EmbedDB.Entities;

using Microsoft.EntityFrameworkCore;


namespace EmbedDB.Data {

    public class EmbedDBContext : DbContext {

        public DbSet<EmbedEntity> Embeds { get; set; }
        public DbSet<FieldEntity> Fields { get; set; }

        public EmbedDBContext(DbContextOptions options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

    }
    
    public class EmbedDBContextDesignTimeFactory : DesignTimeDbContextFactoryBase<EmbedDBContext>
    {
        protected override EmbedDBContext CreateNewInstance(DbContextOptions<EmbedDBContext> options)
        {
            return new EmbedDBContext(options);
        }
    }


}