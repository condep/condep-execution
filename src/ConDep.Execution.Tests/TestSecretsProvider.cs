using System.Security.Cryptography.X509Certificates;
using ConDep.Dsl;
using ConDep.Dsl.Config;

namespace ConDep.Execution.Tests
{
    public class TestSecretsProvider : IProvideSecrets
    {
        private readonly SecretsProviderConfig _secretsProviderSettings;

        public TestSecretsProvider(SecretsProviderConfig secretsProviderSettings)
        {
            _secretsProviderSettings = secretsProviderSettings;
        }

        public DeploymentUserConfig GetDeploymentUser()
        {
            return new DeploymentUserConfig
            {
                UserName = "username_from_secrets_provider",
                Password = "password_from_secrets_provider"
            };
        }
    }
}
