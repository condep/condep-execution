using System;
using System.Linq;
using ConDep.Dsl;
using ConDep.Dsl.Config;
using ConDep.Dsl.SecretsProvider;
using ConDep.Execution.Config;

namespace ConDep.Execution
{
    internal class SecretsProviderLookup : ILookupSecretsProvider
    {
        private readonly SecretsProviderConfig _secretsProviderSettings;

        public SecretsProviderLookup(SecretsProviderConfig secretsProviderSettings)
        {
            _secretsProviderSettings = secretsProviderSettings;
        }

        public IProvideSecrets GetSecretsProvider()
        {
            if (_secretsProviderSettings != null)
            {
                if (!string.IsNullOrWhiteSpace(_secretsProviderSettings.Provider))
                {
                    var assemblyHandler = new ConDepAssemblyHandler(_secretsProviderSettings.Provider);
                    var assembly = assemblyHandler.GetAssembly();

                    var type = assembly.GetTypes().FirstOrDefault(t => typeof(IProvideSecrets).IsAssignableFrom(t));
                    var secretsProvider = Activator.CreateInstance(type, _secretsProviderSettings) as IProvideSecrets;
                    return secretsProvider;
                }
            }
            return new DefaultSecretsProvider();
        }
    }

    internal class DefaultSecretsProvider : IProvideSecrets
    {
        public DeploymentUserConfig GetDeploymentUser()
        {
            throw new NotImplementedException();
        }
    }
}
