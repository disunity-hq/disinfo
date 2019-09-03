using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BindingAttributes;

using Disunity.Disinfo.Interfaces;
using Disunity.Disinfo.Models;

using Slugify;


namespace Disunity.Disinfo.Services {

    [AsScoped]
    public class EmbedService {

        private readonly DbService _dbService;
        private readonly ISlugHelper _slugHelper;

        public EmbedService(DbService dbService, ISlugHelper slugHelper) {
            _dbService = dbService;
            _slugHelper = slugHelper;
        }

        public Embed Lookup(string index) {
            index = _slugHelper.GenerateSlug(index);
            Embed embed = null;
            _dbService.WithTable<Embed>(t => { embed = t.FindById(index); });
            return embed;
        }

        public bool Forget(string index) {
            index = _slugHelper.GenerateSlug(index);
            var fact = Lookup(index);

            if (fact == null) {
                return false;
            }

            _dbService.WithTable<Embed>(t => { t.Delete(o => o.Id == index); });
            return true;
        }

        public bool Forget(ITable fact) {
            var index = _slugHelper.GenerateSlug(fact.Id);
            return Forget(index);
        }

        public Embed Learn(Embed embed) {
            embed.Id = _slugHelper.GenerateSlug(embed.Id);
            var oldFact = Lookup(embed.Id);

            if (oldFact != null) {
                _dbService.WithTable<Embed>(t => t.Update(embed));
            } else {
                _dbService.WithTable<Embed>(t => t.Insert(embed));
            }

            return embed;
        }

        public Embed Learn(string index, string description) {
            index = _slugHelper.GenerateSlug(index);
            var fact = new Embed {Id = index, Description = description};
            return Learn(fact);
        }

        public Embed Update(string index, string key, string value) {
            Console.WriteLine($"==== Updating: {index} / {key} / {value}");
            index = _slugHelper.GenerateSlug(index);

            if (value != null && value.ToLower() == "null") {
                value = null;
            }

            var fact = Lookup(index);

            if (fact == null) {
                fact = new Embed() {Id = index};
                _dbService.WithTable<Embed>(t => t.Insert(fact));
            }

            if (fact.Fields == null) {
                fact.Fields = new Dictionary<string, string>();
            }

            var lowerKey = key?.ToLower() ?? "description";

            if (lowerKey == "description") {
                fact.Description = value;
            } else if (lowerKey == "author") {
                fact.Author = value;
            } else if (lowerKey == "color") {
                fact.Color = value;
            } else if (lowerKey == "url") {
                fact.Url = value;
            } else if (lowerKey == "thumbnail") {
                fact.ThumbnailUrl = value;
            } else if (lowerKey == "image") {
                fact.ImageUrl = value;
            } else if (lowerKey == "locked") {
                fact.Locked = value?.ToLower() == "true";
            }else if (value != null) {
                fact.Fields[lowerKey] = value;
            } else if (fact.Fields.ContainsKey(key)) {
                fact.Fields.Remove(key);
            }
            
            _dbService.WithTable<Embed>(t => t.Update(fact));

            return fact;
        }

        public Embed Update(Dictionary<string, string> factData) {
            var index = _slugHelper.GenerateSlug(factData["Id"]);
            factData.Remove("Id");

            Embed embed = null;

            foreach (var (key, value) in factData) {
                embed = Update(index, key, value);
            }

            return embed;
        }

    }

}