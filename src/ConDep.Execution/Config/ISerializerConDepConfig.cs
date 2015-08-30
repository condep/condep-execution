using System.IO;
using ConDep.Dsl.Config;

namespace ConDep.Execution.Config
{
    public interface ISerializerConDepConfig
    {
        string Serialize(ConDepEnvConfig config);
        ConDepEnvConfig DeSerialize(Stream stream);
        ConDepEnvConfig DeSerialize(string config);
    }
}