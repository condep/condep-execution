using System.IO;

namespace ConDep.Execution.Config
{
    public class ArtifactManifestConfigParser
    {
        private readonly ISerializeConfig<ArtifactManifest> _configSerializer;

        public ArtifactManifestConfigParser(ISerializeConfig<ArtifactManifest> configSerializer)
        {
            _configSerializer = configSerializer;
        }

        public ArtifactManifest GetTypedConfig(string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                return GetTypedConfig(fileStream);
            }
        }

        public ArtifactManifest GetTypedConfig(Stream stream)
        {
            ArtifactManifest manifest = _configSerializer.DeSerialize(stream);

            return manifest;
        }
    }
}