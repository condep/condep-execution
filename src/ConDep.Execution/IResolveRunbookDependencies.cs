using System.Collections.Generic;
using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    /// <summary>
    /// Resolves dependencies between ConDep artifacts
    /// </summary>
    public interface IResolveRunbookDependencies
    {
        List<Runbook> GetDependeciesForRunbook(Runbook runbook, ConDepSettings settings);
    }
}