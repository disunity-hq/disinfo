using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using BindingAttributes;

using Disunity.Disinfo.Interfaces;
using Disunity.Disinfo.Options;

using LiteDB;

using Microsoft.Extensions.Options;


namespace Disunity.Disinfo.Services {

    [AsScoped]
    public class DbService {

        private readonly DbServiceOptions _options;
        public ContextService Context { get; }
        private readonly string _filename;

        public DbService(IOptions<DbServiceOptions> options, ContextService contextService) {
            _options = options.Value;
            Context = contextService;
        }

        public string Filename {
            get {
                var rootPath = _options.Path;
                var guildId = Context.Context.Guild.Id.ToString();
                var filename = Path.Join(rootPath, guildId);
                return filename;
            }
        }

        public void WithDb(Action<LiteDatabase> handler) {
            using (var db = new LiteDatabase(Filename)) {
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