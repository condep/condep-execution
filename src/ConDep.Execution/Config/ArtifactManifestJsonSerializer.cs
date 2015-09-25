using System.IO;
using Newtonsoft.Json;

namespace ConDep.Execution.Config
{
    public class ArtifactManifestJsonSerializer : ISerializeArtifactManifest
    {
        private JsonSerializerSettings _jsonSettings;

        public string Serialize(ArtifactManifest manifest)
        {
            var json = JsonConvert.SerializeObject(manifest, JsonSettings);
            return json;
        }

        public ArtifactManifest DeSerialize(Stream stream)
        {
            ArtifactManifest config;
            using (var memStream = GetMemoryStreamWithCorrectEncoding(stream))
            {
                using (var reader = new StreamReader(memStream))
                {
                    var json = reader.ReadToEnd();
                    config = DeSerialize(json);
                }
            }
            return config;
        }

        public ArtifactManifest DeSerialize(string config)
        {
            return JsonConvert.DeserializeObject<ArtifactManifest>(config, JsonSettings);
        }

        private JsonSerializerSettings JsonSettings
        {
            get
            {
                return _jsonSettings ?? (_jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                });
            }
        }

        private static MemoryStream GetMemoryStreamWithCorrectEncoding(Stream stream)
        {
            using (var r = new StreamReader(stream, true))
            {
                var encoding = r.CurrentEncoding;
                return new MemoryStream(encoding.GetBytes(r.ReadToEnd()));
            }
        }

    }
}