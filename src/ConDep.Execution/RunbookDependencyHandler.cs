using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    internal class RunbookDependencyHandler : IResolveRunbookDependencies
    {
        public bool HasDependenciesDefined(IProvideRunbook artifact)
        {
            var typeName = typeof(IDependOn<>).Name;
            var interfaces = artifact.GetType().GetInterfaces();
            return interfaces.Any(x => x.Name == typeName);
        }

        public void PopulateWithDependencies(IProvideRunbook runbook, ConDepSettings settings)
        {
            if (!HasDependenciesDefined(runbook)) return;

            runbook.Dependencies = GetDependeciesForRunbook(runbook, settings);
        }

        private IEnumerable<IProvideRunbook> GetDependeciesForRunbook(IProvideRunbook runbook, ConDepSettings settings)
        {
            var typeName = typeof(IDependOn<>).Name;
            var typeInterfaces = runbook.GetType().GetInterfaces();

            var dependencies = typeInterfaces.Where(x => x.Name == typeName);
            var dependencyInstances = new List<IProvideRunbook>();

            foreach (var infraInterface in dependencies)
            {
                var dependencyType = infraInterface.GetGenericArguments().Single();

                var dependencyInstance = settings.Options.Assembly.CreateInstance(dependencyType.FullName) as IProvideRunbook;

                dependencyInstances.AddRange(new RunbookDependencyHandler().GetDependeciesForRunbook(dependencyInstance, settings));
                dependencyInstances.Add(dependencyInstance);
            }
            return dependencyInstances;
        }
    }
}