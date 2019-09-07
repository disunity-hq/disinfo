using System;
using System.IO;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace EmbedDB.Data {
    
    public abstract class DesignTimeDbContextFactoryBase<TContext> :
        IDesignTimeDbContextFactory<TContext> where TContext : DbContext {

        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

        public TContext CreateDbContext(string[] args) {
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            var config = builder.Build();
            var section = config.GetSection("Db");
            var filename = section.GetValue<string>("Filename");

            if (string.IsNullOrWhiteSpace(filename)) {
                throw new InvalidOperationException("Db__Filename was empty.");
            }

            return CreateOptions(filename);
        }

        private TContext CreateOptions(string filename) {

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException(
                    $"{nameof(filename)} is null or empty.",
                    nameof(filename));

            var connectionString = $"Data Source={filename}";

            var optionsBuilder =
                new DbContextOptionsBuilder<TContext>();

            Console.WriteLine($"DesignTimeDbContextFactory.Create(string): {connectionString}");

            optionsBuilder.UseSqlite(connectionString);

            DbContextOptions<TContext> options = optionsBuilder.Options;

            return CreateNewInstance(options);
        }

    }

}