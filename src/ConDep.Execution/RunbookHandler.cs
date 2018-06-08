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
        private readonly Regex _oldBindingRegex = new Regex("^([^-]+)-([^-]+)$");
        public Runbook GetRunbook(ConDepSettings settings)
        {
            if (!settings.Options.HasApplicationDefined()) throw new ConDepNoRunbookDefinedException();

            var assembly = settings.Options.Assembly;
            var type = assembly.GetTypes().SingleOrDefault(t => typeof(Runbook).IsAssignableFrom(t) && t.Name == settings.Options.Runbook);
            if (type == null)
            {
                throw new ConDepConfigurationTypeNotFoundException(string.Format("A class inheriting from [{0}] must be present in assembly [{1}] for ConDep to work. No class with name [{2}] found in assembly. Types are: {3}", typeof(Runbook).FullName, assembly.FullName, settings.Options.Runbook, String.Join(", ", assembly.GetTypes().Select(t => t.Name))));
            }
            
            LoadBindingRedirectsFromConfig(settings.Options.Assembly.Location + ".config");

            return CreateRunbook(type);
        }

        private void LoadBindingRedirectsFromConfig(string config)
        {
            if (File.Exists(config))
            {
                XElement configFile = XElement.Load(config);
                var dependentAssemblies = (
                    from runtime in configFile.Descendants("runtime")
                    from assemblyBinding in runtime.Elements(XName.Get("assemblyBinding",
                        "urn:schemas-microsoft-com:asm.v1"))
                    from dependentAssembly in assemblyBinding.Elements(XName.Get("dependentAssembly",
                        "urn:schemas-microsoft-com:asm.v1"))
                    select dependentAssembly
                );
                if (dependentAssemblies.Any())
                {
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        var requestedName = new AssemblyName(args.Name);
                        foreach (var da in dependentAssemblies)
                        {
                            var assemblyIdentity =
                                da.Elements(XName.Get("assemblyIdentity", "urn:schemas-microsoft-com:asm.v1"))
                                    .FirstOrDefault();
                            var bindingRedirect =
                                da.Elements(XName.Get("bindingRedirect", "urn:schemas-microsoft-com:asm.v1"))
                                    .FirstOrDefault();
                            if (assemblyIdentity == null || bindingRedirect == null)
                                continue;

                            var idNameAttribute = assemblyIdentity.Attribute(XName.Get("name"));
                            var bindingOldAttribute = bindingRedirect.Attribute(XName.Get("oldVersion"));
                            var bindingNewAttribute = bindingRedirect.Attribute(XName.Get("newVersion"));
                            if (bindingNewAttribute == null || idNameAttribute == null || bindingOldAttribute == null)
                                continue;

                            string idName = idNameAttribute.Value;
                            string bindingOld = bindingOldAttribute.Value;
                            string bindingNew = bindingNewAttribute.Value;
                            var match = _oldBindingRegex.Match(bindingOld);
                            if (!match.Success)
                                continue;

                            var bindingStart = Version.Parse(match.Groups[1].Value);
                            var bindingEnd = Version.Parse(match.Groups[2].Value);

                            if (requestedName.Name.Equals(idName) && requestedName.Version >= bindingStart &&
                                requestedName.Version <= bindingEnd)
                            {
                                requestedName.Version = Version.Parse(bindingNew);
                                return Assembly.Load(requestedName.ToString());
                            }
                        }

                        return null;
                    };
                }
            }
        }

        private static Runbook CreateRunbook(Type type)
        {
            var runbook = Activator.CreateInstance(type) as Runbook;
            if (runbook == null) throw new NullReferenceException(string.Format("Instance of application class [{0}] not found.", type.FullName));

            return runbook;
        }
    }
}