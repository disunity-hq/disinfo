using System;

using BindingAttributes;

using Disunity.Disinfo.Interfaces;
using Disunity.Disinfo.Options;

using LiteDB;

using Microsoft.Extensions.Options;


namespace Disunity.Disinfo.Services.Singleton {

    [AsSingleton]
    public class DbService {

        private readonly DbServiceOptions _options;

        public DbService(IOptions<DbServiceOptions> options) {
            _options = options.Value;
        }

        public void WithDb(Action<LiteDatabase> handler) {
            using (var db = new LiteDatabase(_options.Path)) {
                handler(db);
            }
        }

        public void WithTable<T>(Action<LiteCollection<T>> handler) where T : ITable {
            WithDb(db => {
                var name = typeof(T).Name;
                var table = db.GetCollection<T>(name);
                handler(table);
                table.EnsureIndex(o => o.Id);
            });
        }

    }

}