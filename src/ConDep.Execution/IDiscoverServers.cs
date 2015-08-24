using System.Collections.Generic;
using ConDep.Dsl.Config;

namespace ConDep.Dsl.Execution
{
    public interface IDiscoverServers
    {
        IEnumerable<IServerConfig> GetServers(IProvideArtifact application, ConDepSettings settings);
    }
}