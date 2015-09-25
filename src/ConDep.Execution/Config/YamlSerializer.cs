using System.IO;
using YamlDotNet.Serialization;

namespace ConDep.Execution.Config
{
    public class YamlSerializer<T> : ISerializeConfig<T>
    {
        private readonly IHandleConfigCrypto _crypto;

        public YamlSerializer(IHandleConfigCrypto crypto)
        {
            _crypto = crypto;
        }

        public string Serialize(T config)
        {
            using (var stringWriter = new StringWriter())
            {
                var serializer = new Serializer();
                serializer.Serialize(stringWriter, config);
                return stringWriter.ToString();
            }
        }

        public T DeSerialize(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var deserialize = new Deserializer(ignoreUnmatched: true);
                deserialize.RegisterTagMapping("tag:yaml.org,2002:encrypt", typeof(string));
                return deserialize.Deserialize<T>(reader);
            }
        }

        public T DeSerialize(string config)
        {
            using (var stringReader = new StringReader(config))
            {
                var deserialize = new Deserializer(ignoreUnmatched: true);
                deserialize.RegisterTagMapping("tag:yaml.org,2002:encrypt", typeof(string));
                return deserialize.Deserialize<T>(stringReader);
            }
        }
    }

    public class YamlEncrypt
    {
        private readonly string _secret;

        public YamlEncrypt(string secret)
        {
            _secret = secret;
        }

        public override string ToString()
        {
            return _secret;
        }
    }
}