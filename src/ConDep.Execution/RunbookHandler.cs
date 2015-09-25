using System;
using System.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;
using ConDep.Execution.Config;

namespace ConDep.Execution
{
    internal class RunbookHandler : IDiscoverRunbooks
    {
        public IProvideRunbook GetRunbook(ConDepSettings settings)
        {
            if (!settings.Options.HasApplicationDefined()) throw new ConDepNoRunbookDefinedException();

            var assembly = settings.Options.Assembly;

            var type = assembly.GetTypes().SingleOrDefault(t => typeof(IProvideRunbook).IsAssignableFrom(t) && t.Name == settings.Options.Runbook);
            if (type == null)
            {
                throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] or [{1}] must be present in assembly [{2}] for ConDep to work. No calss with name [{3}] found in assembly. ", typeof(Runbook.Local).FullName, typeof(Runbook.Remote).FullName, assembly.FullName, settings.Options.Runbook));
            }
            return CreateRunbook(type);
        }

        private static IProvideRunbook CreateRunbook(Type type)
        {
            var runbook = Activator.CreateInstance(type) as IProvideRunbook;
            if (runbook == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] not found.", type.FullName));

            return runbook;
        }
    }
}