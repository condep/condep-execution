using System.IO;
using Newtonsoft.Json;

namespace ConDep.Execution.Config
{
    public class JsonSerializer<T> : ISerializeConfig<T>
    {
        private JsonSerializerSettings _jsonSettings;
        private readonly IHandleConfigCrypto _crypto;

        public JsonSerializer(IHandleConfigCrypto crypto)
        {
            _crypto = crypto;
        }

        public string Serialize(T config)
        {
            var json = JsonConvert.SerializeObject(config, JsonSettings);
            var encryptedJson = _crypto.Encrypt(json);
            return encryptedJson;
        }

        public T DeSerialize(Stream stream)
        {
            T config;
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

        public T DeSerialize(string config)
        {
            if (_crypto.IsEncrypted(config))
            {
                config = _crypto.Decrypt(config);
            }
            return JsonConvert.DeserializeObject<T>(config, JsonSettings);
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