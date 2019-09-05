using Disunity.Disinfo.Models.Entities;

using LiteDB;


namespace Disunity.Disinfo.Extensions {

    public static class LiteCollectionEmbedExtensions {

        private static string Hash(string slug, string guild = "0") {
            return $"{slug}####{guild}";
        }

        /// <summary>
        /// Find an entry by unique slug/guild combination.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="slug"></param>
        /// <param name="guild"></param>
        /// <returns>Returns the found EmbedEntry or null</returns>
        public static EmbedEntry QueryEntry(this LiteCollection<EmbedEntry> db, string slug, string guild = "0") {
            var id = Hash(slug, guild);
            return db.FindById(id);
        }

        /// <summary>
        /// Remove an entry from the database by unique slug/guild combo.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="slug"></param>
        /// <param name="guild"></param>
        /// <returns></returns>
        public static int DeleteEntry(this LiteCollection<EmbedEntry> db, EmbedEntry entry) {
            var id = Hash(entry.Slug, entry.Guild);
            return db.Delete(o => o.Id == id);
        }

        /// <summary>
        /// Use the provided EmbedEntry to update an existing entry.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static bool UpdateEntry(this LiteCollection<EmbedEntry> db, EmbedEntry entry) {
            entry.Id = Hash(entry.Slug, entry.Guild);
            return db.Update(entry);
        }

        /// <summary>
        /// Insert the provided EmbedEntry into the database.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static BsonValue InsertEntry(this LiteCollection<EmbedEntry> db, EmbedEntry entry) {
            entry.Id = Hash(entry.Slug, entry.Guild);
            return db.Insert(entry);
        }

    }

}