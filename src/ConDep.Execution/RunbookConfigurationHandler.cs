using System.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Builders;
using ConDep.Dsl.Config;
using ConDep.Dsl.Sequence;

namespace ConDep.Execution
{
    internal class RunbookConfigurationHandler
    {
        private readonly IDiscoverRunbooks _runbookHandler;
        private readonly IResolveRunbookDependencies _runbookDependencyHandler;
        private readonly IDiscoverServers _serverHandler;
        private readonly ILoadBalance _loadBalancer;

        public RunbookConfigurationHandler(IDiscoverRunbooks runbookHandler, IResolveRunbookDependencies runbookDependencyHandler, IDiscoverServers serverHandler, ILoadBalance loadBalancer)
        {
            _runbookHandler = runbookHandler;
            _runbookDependencyHandler = runbookDependencyHandler;
            _serverHandler = serverHandler;
            _loadBalancer = loadBalancer;
        }

        public IManageExecutionSequence CreateExecutionSequence(ConDepSettings settings)
        {
            var artifact = _runbookHandler.GetRunbook(settings);
            _runbookDependencyHandler.PopulateWithDependencies(artifact, settings);

            var servers = _serverHandler.GetServers(artifact, settings);
            settings.Config.Servers = servers.ToList();

            var sequenceManager = new ExecutionSequenceManager(servers, _loadBalancer);

            if (artifact.Dependencies != null)
            {
                foreach (var dependency in artifact.Dependencies)
                {
                    ConfigureRunbook(dependency, sequenceManager, settings);
                }
            }

            ConfigureRunbook(artifact, sequenceManager, settings);
            return sequenceManager;
        }

        private void ConfigureRunbook(IProvideRunbook runbook, IManageExecutionSequence sequenceManager, ConDepSettings settings)
        {
            if (runbook is Runbook.Local)
            {
                var localSequence = sequenceManager.NewLocalSequence(runbook.GetType().Name);
                var localBuilder = new LocalOperationsBuilder(localSequence);
                ((Runbook.Local)runbook).Configure(localBuilder, settings);
            }
            else if (runbook is Runbook.Remote)
            {
                var remoteSequence = sequenceManager.NewRemoteSequence(runbook.GetType().Name);
                var remoteBuilder = new RemoteOperationsBuilder(remoteSequence);
                ((Runbook.Remote)runbook).Configure(remoteBuilder, settings);
            }
        }
    }
}