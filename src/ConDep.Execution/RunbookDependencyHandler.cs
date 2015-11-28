using System.Collections.Generic;
using System.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    internal class RunbookDependencyHandler : IResolveRunbookDependencies
    {
        public bool HasDependenciesDefined(Runbook artifact)
        {
            var typeName = typeof(IDependOn<>).Name;
            var interfaces = artifact.GetType().GetInterfaces();
            return interfaces.Any(x => x.Name == typeName);
        }

        //public void PopulateWithDependencies(Runbook runbook, ConDepSettings settings)
        //{
        //    if (!HasDependenciesDefined(runbook)) return;

        //    runbook.Dependencies = GetDependeciesForRunbook(runbook, settings);
        //}

        public List<Runbook> GetDependeciesForRunbook(Runbook runbook, ConDepSettings settings)
        {
            var typeName = typeof(IDependOn<>).Name;
            var typeInterfaces = runbook.GetType().GetInterfaces();

            var dependencies = typeInterfaces.Where(x => x.Name == typeName);
            var dependencyInstances = new List<Runbook>();

            foreach (var infraInterface in dependencies)
            {
                var dependencyType = infraInterface.GetGenericArguments().Single();

                var dependencyInstance = settings.Options.Assembly.CreateInstance(dependencyType.FullName) as Runbook;

                dependencyInstances.AddRange(new RunbookDependencyHandler().GetDependeciesForRunbook(dependencyInstance, settings));
                dependencyInstances.Add(dependencyInstance);
            }
            return dependencyInstances;
        }
    }
}