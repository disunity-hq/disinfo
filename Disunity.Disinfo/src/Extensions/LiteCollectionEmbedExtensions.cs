using Disunity.Disinfo.Models.Entities;

using LiteDB;


namespace Disunity.Disinfo.Extensions {

    public static class LiteCollectionEmbedExtensions {

        private static string Hash(string slug, string guild = "0") {
            return $"{slug}####{guild}";
        }

        public static EmbedEntry FindById(this LiteCollection<EmbedEntry> db, string slug, string guild = "0") {
            var id = Hash(slug, guild);
            return db.FindById(id);
        }

        public static int Delete(this LiteCollection<EmbedEntry> db, string slug, string guild = "0") {
            var id = Hash(slug, guild);
            return db.Delete(o => o.Id == id);
        }

        public static bool UpdateEntry(this LiteCollection<EmbedEntry> db, EmbedEntry entry) {
            entry.Id = Hash(entry.Slug, entry.Guild);
            return db.Update(entry);
        }

        public static BsonValue InsertEntry(this LiteCollection<EmbedEntry> db, EmbedEntry entry) {
            entry.Id = Hash(entry.Slug, entry.Guild);
            return db.Insert(entry);
        }

    }

}