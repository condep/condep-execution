using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    /// <summary>
    /// Resolves dependencies between ConDep artifacts
    /// </summary>
    public interface IResolveRunbookDependencies
    {
        /// <summary>
        /// Recursively detects and populates dependency tree of Artifact dependencies
        /// </summary>
        /// <param name="runbook"></param>
        /// <param name="settings"></param>
        void PopulateWithDependencies(IProvideRunbook runbook, ConDepSettings settings);
    }
}