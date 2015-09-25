using System.IO;

namespace ConDep.Execution.Config
{
    public interface ISerializeConfig<T>
    {
        string Serialize(T config);
        T DeSerialize(Stream stream);
        T DeSerialize(string config);
    }
}