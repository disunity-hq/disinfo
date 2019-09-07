using System;
using System.Reflection;

using EmbedDB.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace EmbedDB.Data {

    public class EmbedDBContext<T> : DbContext where T : EmbedDBContext<T> {

        public DbSet<EmbedEntity> Embeds { get; set; }
        public DbSet<FieldEntity> Fields { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (optionsBuilder.IsConfigured) return;
            optionsBuilder.UseSqlite("Data Source=disinfo.sqlite");
        }

    }

}