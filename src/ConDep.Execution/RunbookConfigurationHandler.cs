using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution
{
    internal class RunbookConfigurationHandler
    {
        private readonly IDiscoverRunbooks _runbookHandler;
        private readonly IResolveRunbookDependencies _runbookDependencyHandler;
        private readonly ILoadBalance _loadBalancer;
        private static readonly Regex OldBindingRegex = new Regex("^([^-]+)-([^-]+)$");
        private ResolveEventHandler bindingRedirectHandler = null;

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
        
        public void LoadBindingRedirects(ConDepSettings settings)
        {
            if(bindingRedirectHandler != null)
                throw new Exception("Previous binding redirects was not unloaded before loading new assembly");
            bindingRedirectHandler = LoadBindingRedirectsFromConfig(settings.Options.Assembly.Location + ".config");
        }

        public void UnloadBindingRedirects()
        {
            if(bindingRedirectHandler != null)
                AppDomain.CurrentDomain.AssemblyResolve -= bindingRedirectHandler;
        }
        
        private ResolveEventHandler LoadBindingRedirectsFromConfig(string config)
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
                    ResolveEventHandler eventHandler = (sender, args) =>
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
                            var match = OldBindingRegex.Match(bindingOld);
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
                    AppDomain.CurrentDomain.AssemblyResolve += eventHandler;
                    return eventHandler;
                }
            }

            return null;
        }
    }
}