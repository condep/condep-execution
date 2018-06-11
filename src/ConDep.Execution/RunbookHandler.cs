using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;
using ConDep.Execution.Config;

namespace ConDep.Execution
{
    internal class RunbookHandler : IDiscoverRunbooks
    {
        public Runbook GetRunbook(ConDepSettings settings)
        {
            if (!settings.Options.HasApplicationDefined()) throw new ConDepNoRunbookDefinedException();

            var assembly = settings.Options.Assembly;
            var type = assembly.GetTypes().SingleOrDefault(t => typeof(Runbook).IsAssignableFrom(t) && t.Name == settings.Options.Runbook);
            if (type == null)
            {
                throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] must be present in assembly [{1}] for ConDep to work. No class with name [{2}] found in assembly. Types are: {3}", typeof(Runbook).FullName, assembly.FullName, settings.Options.Runbook, String.Join(", ", assembly.GetTypes().Select(t => t.Name))));
            }

            return CreateRunbook(type);
        }
        

        private static Runbook CreateRunbook(Type type)
        {
            var runbook = Activator.CreateInstance(type) as Runbook;
            if (runbook == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] not found.", type.FullName));

            return runbook;
        }
    }
}