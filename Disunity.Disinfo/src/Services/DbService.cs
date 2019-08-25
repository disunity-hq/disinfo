
using System;

using BindingAttributes;

using Disunity.Disinfo.Interfaces;

using LiteDB;

using Microsoft.Extensions.Configuration;


namespace Disunity.Disinfo.Services {
    

    [AsSingleton]
    public class DbService {

        private readonly IConfigurationRoot _config;
        private readonly string _filename;

        public DbService(IConfigurationRoot config) {
            _config = config;
            _filename = config["DbFilename"];
        }

        public void WithDb(Action<LiteDatabase> handler) {
            using (var db = new LiteDatabase(_filename)) {
                handler(db);
            }
        }

        public void WithTable<T>(Action<LiteCollection<T>> handler) where T: ITable {
            WithDb(db => {
                var name = typeof(T).Name;
                var table = db.GetCollection<T>(name);
                handler(table);
                table.EnsureIndex(o => o.Id);
            });
        }

    }

}