using ConDep.Dsl.Config;
using ConDep.Dsl.LoadBalancer;
using ConDep.Execution;
using NUnit.Framework;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class TiersTests
    {
        private ConDepSettings _tierSettings;
        private ConDepSettings _serverSettings;

        [SetUp]
        public void Setup()
        {
            _tierSettings = new ConDepSettings
            {
                Config =
                {
                    Tiers = new[]
                    {
                        new TiersConfig
                        {
                            Name = Tier.Web.ToString(),
                            Servers = new[]
                            {
                                new ServerConfig
                                {
                                    Name = "TierServer1"
                                },
                                new ServerConfig
                                {
                                    Name = "TierServer2"
                                }
                            }
                        }
                    }
                },
                Options = { Assembly = typeof(MyArtifactWithTierTag).Assembly, Runbook = "MyArtifactWithTierTag" }
            };

            _serverSettings = new ConDepSettings
            {
                Config =
                {
                    Servers = new[]
                    {
                        new ServerConfig
                        {
                            Name = "Server1"
                        },
                        new ServerConfig
                        {
                            Name = "Server2"
                        }
                    }
                },
                Options = { Assembly = typeof(MyArtifactWithTierTag).Assembly, Runbook = "MyArtifactWithTierTag" }
            };
        }

        [Test]
        [ExpectedException(typeof(ConDepNoRunbookTierDefinedException))]
        [Ignore]
        public void TestThat_ArtifactFailsWhenNotTaggedWithTierForTierConfig()
        {
            //var configHandler = new RunbookConfigurationHandler(new RunbookHandler(), new RunbookDependencyHandler(),
            //    new ServerHandler(), new DefaultLoadBalancer());

            //_tierSettings.Options.Runbook = typeof (MyArtifactWithoutTierTag).Name;
            //configHandler.CreateExecutionSequence(_tierSettings);
        }

        [Test]
        public void TestThat_ArtifactSucceedsWhenTaggedWithTierForTierConfig()
        {
            //var configHandler = new RunbookConfigurationHandler(new RunbookHandler(), new RunbookDependencyHandler(),
            //    new ServerHandler(), new DefaultLoadBalancer());

            //_tierSettings.Options.Runbook = typeof(MyArtifactWithTierTag).Name;
            //configHandler.CreateExecutionSequence(_tierSettings);
        }
    }

    [Tier(Tier.Web)]
    public class MyArtifactWithTierTag : Runbook
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
        }
    }
    public class MyArtifactWithoutTierTag : Runbook
    {
        public override void Execute(IOfferOperations dsl, ConDepSettings settings)
        {
        }
    }
}