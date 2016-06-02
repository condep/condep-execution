using System;
using System.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;
using ConDep.Dsl.LoadBalancer;
using ConDep.Execution.Config;

namespace ConDep.Execution
{
    internal class LoadBalancerLookup : ILookupLoadBalancer
    {
        private readonly LoadBalancerConfig _loadBalancerSettings;

        public LoadBalancerLookup(LoadBalancerConfig loadBalancerSettings)
        {
            _loadBalancerSettings = loadBalancerSettings;
        }

        public ILoadBalance GetLoadBalancer()
        {
            if (_loadBalancerSettings != null)
            {
                if(!string.IsNullOrWhiteSpace(_loadBalancerSettings.Provider))
                {
                    var assemblyHandler = new ConDepAssemblyHandler(_loadBalancerSettings.Provider);
                    var assembly = assemblyHandler.GetAssembly();

                    var type = assembly.GetTypes().FirstOrDefault(t => typeof(ILoadBalance).IsAssignableFrom(t));
                    var loadBalancer = Activator.CreateInstance(type, _loadBalancerSettings) as ILoadBalance;
                    loadBalancer.Mode = _loadBalancerSettings.GetModeAsEnum();
                    return loadBalancer;
                }
            }
            return new DefaultLoadBalancer();
        }
        
    }

    public class DefaultLoadBalancer : ILoadBalance
    {
        public DefaultLoadBalancer()
        {
            Mode = LoadBalancerMode.RoundRobin;
        }

        public Result BringOffline(string serverName, string farm, LoadBalancerSuspendMethod suspendMethod)
        {
            return Result.SuccessUnChanged();
        }

        public Result BringOnline(string serverName, string farm)
        {
            return Result.SuccessUnChanged();
        }

        public LoadBalancerMode Mode { get; set; }
    }
}