using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace EmbedDB.Entities {

    public class FieldEntity {

        public string Key { get; set; }
        public string Value { get; set; }
        public bool Inline { get; set; } = false;


        public int EntityId { get; set; }
        public EmbedEntity Entity { get; set; }

    }

    internal class FieldEntityConfiguration : IEntityTypeConfiguration<FieldEntity> {

        public void Configure(EntityTypeBuilder<FieldEntity> builder) {

            builder.HasKey(e => new {e.EntityId, e.Key});

        }

    }

}