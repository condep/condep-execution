using System.Collections.Generic;
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
        private readonly ILoadBalance _loadBalancer;

        public RunbookConfigurationHandler(IDiscoverRunbooks runbookHandler, IResolveRunbookDependencies runbookDependencyHandler, ILoadBalance loadBalancer)
        {
            _runbookHandler = runbookHandler;
            _runbookDependencyHandler = runbookDependencyHandler;
            _loadBalancer = loadBalancer;
        }

        public IEnumerable<Runbook> GetRunbooksToExecute(ConDepSettings settings)
        {
            var runbook = _runbookHandler.GetRunbook(settings);
            var dependantRunbooks =_runbookDependencyHandler.GetDependeciesForRunbook(runbook, settings);
            dependantRunbooks.Add(runbook);
            return dependantRunbooks;
        }
    }
}