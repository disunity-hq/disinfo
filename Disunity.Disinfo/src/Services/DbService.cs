
using System;

using LiteDB;

using Microsoft.Extensions.Configuration;


namespace Disunity.Disinfo.Services {

    public interface ITable {

        string Id { get; set; }

    }

    public class DbService {

        private readonly IConfigurationRoot _config;
        private string _filename;

        public DbService(IConfigurationRoot config) {
            _config = config;
            _filename = config["DbFilename"];
        }

        public void WithDb(Action<LiteDatabase> handler) {
            using (var db = new LiteDatabase(_filename)) {
                handler(db);
            }
        }

        public void WithTable<T>(string name, Action<LiteCollection<T>> handler) where T: ITable {
            WithDb(db => {
                var table = db.GetCollection<T>(name);
                handler(table);
                table.EnsureIndex(o => o.Id);
            });
        }

    }

}