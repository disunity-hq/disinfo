using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Disunity.Disinfo.Interfaces;

using Slugify;


namespace Disunity.Disinfo.Services {

    public class FactService {

        private readonly DbService _dbService;
        private readonly SlugHelper _slugHelper;

        public FactService(DbService dbService, SlugHelper slugHelper) {
            _dbService = dbService;
            _slugHelper = slugHelper;
        }

        public Fact Lookup(string index) {
            index = _slugHelper.GenerateSlug(index);
            Fact fact = null;
            _dbService.WithTable<Fact>(t => { fact = t.FindById(index); });
            return fact;
        }

        public bool Forget(string index) {
            index = _slugHelper.GenerateSlug(index);
            var fact = Lookup(index);

            if (fact == null) {
                return false;
            }

            _dbService.WithTable<Fact>(t => { t.Delete(o => o.Id == index); });
            return true;
        }

        public bool Forget(ITable fact) {
            var index = _slugHelper.GenerateSlug(fact.Id);
            return Forget(index);
        }

        public Fact Learn(Fact fact) {
            fact.Id = _slugHelper.GenerateSlug(fact.Id);
            var oldFact = Lookup(fact.Id);

            if (oldFact != null) {
                _dbService.WithTable<Fact>(t => t.Update(fact));
            } else {
                _dbService.WithTable<Fact>(t => t.Insert(fact));
            }

            return fact;
        }

        public Fact Learn(string index, string description) {
            index = _slugHelper.GenerateSlug(index);
            var fact = new Fact {Id = index, Description = description};
            return Learn(fact);
        }

        public Fact Update(string index, string key, string value) {
            Console.WriteLine($"==== Updating: {index} / {key} / {value}");
            index = _slugHelper.GenerateSlug(index);

            if (value.ToLower() == "null") {
                value = null;
            }

            var fact = Lookup(index);

            if (fact == null) {
                fact = new Fact() {Id = index};
                _dbService.WithTable<Fact>(t => t.Insert(fact));
            }

            if (fact.Fields == null) {
                fact.Fields = new Dictionary<string, string>();
            }

            var lowerKey = key.ToLower();

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
            } else if (value != null) {
                fact.Fields[lowerKey] = value;
            } else if (fact.Fields.ContainsKey(key)) {
                fact.Fields.Remove(key);
            }

            _dbService.WithTable<Fact>(t => t.Update(fact));

            return fact;
        }

        public Fact Update(Dictionary<string, string> factData) {
            var index = _slugHelper.GenerateSlug(factData["Id"]);
            factData.Remove("Id");

            Fact fact = null;

            foreach (var (key, value) in factData) {
                fact = Update(index, key, value);
            }

            return fact;
        }

    }

}