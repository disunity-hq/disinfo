using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

using BindingAttributes;

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

        public ImmutableArray<string> CapturesFrom(Match match, int index = 1) {
            if (index >= match.Groups.Count) {
                return new ImmutableArray<string>();
            }

            return match.Groups[index]
                        .Captures
                        .Select(c => c.Value.Trim())
                        .ToImmutableArray();
        }

        private EmbedRef ParseRef(string input) {
            var (factStr, propStr, _) = input.Split('.', 2);
            var fact = _embedService.Query(factStr, _contextService.Guild);

            return new EmbedRef {
                EmbedEntry = fact,
                Slug = factStr,
                Property = propStr,
                Input = input
            };
        }

        public IEnumerable<EmbedRef> ParseCaptures(Match match, int index = 1) {
            var captures = CapturesFrom(match, index);
            return captures.Select(ParseRef).ToList();
        }

        public Dictionary<string, string> LoadJson(string index, string json) {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (data == null) {
                throw new JsonException("JSON data was blank or not a well-formed Object.");
            }

            data["Slug"] = index;
            data["Guild"] = _contextService.Guild;
            return data;
        }

        public Dictionary<string, string> LoadYaml(string input, string yaml) {
            var data = _serializer.Deserialize<Dictionary<string, string>>(yaml);

            if (data == null) {
                throw new YamlException("YAML data was blank or not a well-formed Object.");
            }

            data["Slug"] = input;
            data["Guild"] = _contextService.Guild;
            return data;
        }

    }

}