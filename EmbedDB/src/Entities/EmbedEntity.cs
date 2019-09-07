using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace EmbedDB.Entities {

    public class EmbedEntity {


        public int Id { get; set; }

        // Avoid modifying the following directly.
        // Used as a database column only.
        [Required] public long __GuildId { get; set; }

        [Required] public string __Slug { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        [DataType(DataType.Url)] public string Url { get; set; }

        [DataType(DataType.ImageUrl)] public string Image { get; set; }

        [DataType(DataType.ImageUrl)] public string Thumbnail { get; set; }

        [DataType(DataType.Url)] public string AuthorUrl { get; set; }

        [DataType(DataType.ImageUrl)] public string AuthorIcon { get; set; }

        public ICollection<FieldEntity> Fields { get; set; }

        // Access/modify this variable instead.
        // Tell EF not to map this field to a Db table
        [NotMapped]
        public ulong GuildId {
            get {
                unchecked {
                    return (ulong) __GuildId;
                }
            }

            set {
                unchecked {
                    __GuildId = (long) value;
                }
            }
        }

        // Access/modify this variable instead.
        // Tell EF not to map this field to a Db table
        [NotMapped]
        public string Slug {
            get => __Slug;

            set {
                var slugifier = new Slugify.SlugHelper();
                __Slug = slugifier.GenerateSlug(value);
            }
        }

    }

    internal class EmbedEntityConfiguration : IEntityTypeConfiguration<EmbedEntity> {

        public void Configure(EntityTypeBuilder<EmbedEntity> builder) {

            builder.HasKey(e => new {e.__GuildId, e.__Slug});
            builder.HasKey(e => e.Id);

        }

    }

}