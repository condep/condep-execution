using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    public interface IDiscoverArtifacts
    {
        IProvideArtifact GetArtifact(ConDepSettings settings);
    }
}