using System.IO;

namespace ConDep.Execution.Config
{
    public interface ISerializeArtifactManifest
    {
        string Serialize(ArtifactManifest config);
        ArtifactManifest DeSerialize(Stream stream);
        ArtifactManifest DeSerialize(string config);
    }
}