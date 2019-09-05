using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

using BindingAttributes;

using Discord;

using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Models;

using Newtonsoft.Json;

using SharpYaml;
using SharpYaml.Serialization;


namespace Disunity.Disinfo.Services.Singleton {

    [AsScoped]
    public class LearnModuleParserService {

        private readonly ContextService _contextService;
        private readonly EmbedService _embedService;
        private readonly Serializer _serializer;

        public LearnModuleParserService(ContextService contextService,
                                        EmbedService embedService, 
                                        Serializer serializer) {
            _contextService = contextService;
            _embedService = embedService;
            _serializer = serializer;

        }

        public ImmutableArray<string> CapturesFromMatchGroup(Match match, int index = 1) {
            if (index >= match.Groups.Count) {
                return new ImmutableArray<string>();
            }

            return match.Groups[index]
                        .Captures
                        .Select(c => c.Value.Trim())
                        .ToImmutableArray();
        }

        public EmbedReference ParseReference(string input) {
            var (slug, property, _) = input.Split('.', 2);
            var entry = _embedService.Query(slug) ?? _embedService.Query(slug, _contextService.Guild);

            return new EmbedReference {
                EmbedEntry = entry,
                Slug = slug,
                Property = property,
                Input = input
            };
        }

        public IEnumerable<EmbedReference> ParseReferences(Match match, int index = 1) {
            var captures = CapturesFromMatchGroup(match, index);
            return captures.Select(ParseReference);
        }

        public Dictionary<EmbedReference, string> LoadJson(string input, string json) {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (data == null) {
                throw new JsonException("JSON data was blank or not a well-formed Object.");
            }

            return data.Select(kp => {
                var embedReference = ParseReference(input);
                var (property, value) = kp;
                embedReference.Property = property;
                return (embedReference, value);
            }).ToDictionary(tup => tup.Item1, tup => tup.Item2);

        }

        public Dictionary<EmbedReference, string> LoadYaml(string input, string yaml) {
            var data = _serializer.Deserialize<Dictionary<string, string>>(yaml);

            if (data == null) {
                throw new YamlException("YAML data was blank or not a well-formed Object.");
            }

            return data.Select(kp => {
                var embedReference = ParseReference(input);
                var (property, value) = kp;
                embedReference.Property = property;
                return (embedReference, value);
            }).ToDictionary(tup => tup.Item1, tup => tup.Item2);
        }

    }

}