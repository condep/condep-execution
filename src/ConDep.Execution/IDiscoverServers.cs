using System.Collections.Generic;
using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    public interface IDiscoverServers
    {
        IEnumerable<ServerConfig> GetServers(IProvideRunbook runbook, ConDepSettings settings);
    }
}