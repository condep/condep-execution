using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    /// <summary>
    /// Resolves dependencies between ConDep artifacts
    /// </summary>
    public interface IResolveArtifactDependencies
    {
        /// <summary>
        /// Recursively detects and populates dependency tree of Artifact dependencies
        /// </summary>
        /// <param name="artifact"></param>
        /// <param name="settings"></param>
        void PopulateWithDependencies(IProvideArtifact artifact, ConDepSettings settings);
    }
}