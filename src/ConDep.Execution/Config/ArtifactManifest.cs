using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using ConDep.Execution.Relay;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConDep.Execution.Config
{
    [DataContract]
    public class ArtifactManifest
    {
        private readonly IEnumerable<ApplicationArtifact> _apps = new List<ApplicationArtifact>();

        [DataMember]
        public ApplicationArtifact ConPack { get; set; }

        [DataMember]
        public IEnumerable<ApplicationArtifact> Apps { get { return _apps; } }
    }

    public abstract class JsonConverter<T> : JsonConverter where T : ApplicationArtifact
    {
        protected abstract T Create(Type objectType, JObject jsonObject);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var type = value.GetType();
            var name = ConvertClassNameTo(type.Name);

            writer.WriteStartObject();
            writer.WritePropertyName(name);
            writer.WriteStartObject();

            foreach (var prop in type.GetProperties())
            {
                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, prop.GetValue(value, BindingFlags.GetProperty, null, null, CultureInfo.CurrentCulture));
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        private string ConvertClassNameTo(string className)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            var converted = r.Replace(className, "_").ToLower();
            return converted.Replace("_artifact", "");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(objectType, jsonObject);
            serializer.Populate(jsonObject.PropertyValues().First().CreateReader(), target);
            return target;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ApplicationArtifact).IsAssignableFrom(objectType);
        }
    }

    public class ApplicationArtifactConverter : JsonConverter<ApplicationArtifact>
    {
        protected override ApplicationArtifact Create(Type objectType, JObject jsonObject)
        {
            if (jsonObject["http_basic"] != null)
            {
                return new HttpBasicArtifact();
            }

            if (jsonObject["rest_bearer"] != null)
            {
                return new RestBearerArtifact();
            }

            return null;
        }
    }
}