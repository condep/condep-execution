﻿using System.IO;
using System.Text;
using ConDep.Dsl.Config;
using ConDep.Execution.Config;
using ConDep.Execution.Security;
using NUnit.Framework;
using System.Linq;

namespace ConDep.Dsl.Tests
{
    [TestFixture]
    public class JsonParsingTests
    {
        private string _encryptJson =
            @"{
    ""Servers"" :
    [
        {
		    ""Name"" : ""jat-web01""
        }
    ],
    ""OperationsConfig"":
    {
        ""NServiceBusOperation"": 
        {
            ""ServiceUserName"": ""torresdal\\nservicebususer"",
            ""ServicePassword"": 
            {
                ""encrypt"" : ""verySecureP@ssw0rd""
            },
            ""IV"" : ""asdf"",
            ""SomeSecret1"":
            {
                ""encrypt"" : ""asdfasdfasdfasdfwer2343453456""
            },
            ""test"" :
            [
                {
                    ""asldkjf"":
                    {
                        ""encrypt"" : ""92873492734""
                    }
                },
                {
                    ""asdfasdfasdf"":
                    {
                        ""encrypt"" : ""abc""
                    }
                }
            ]
        },
        ""SomeOtherOperation"":
        {
            ""SomeOtherSetting1"": ""asdfasdf"",
            ""SomeOtherSetting2"": ""34tsdfg""
        }
    }
}";

        private string _tiersJson =
            @"{
	""Tiers"" :
	[
        {
            ""Name"" : ""Web"",
			""Servers"" :
			[
				{
					""Name"" : ""jat-web01""
				},
				{
					""Name"" : ""jat-web02""
				}
			],
            ""LoadBalancer"": 
            {
                ""Name"": ""jat-nlb01"",
                ""Provider"": ""ConDep.Dsl.LoadBalancer.Ace.dll"",
                ""UserName"": ""torresdal\\nlbUser"",
                ""Password"": ""verySecureP@ssw0rd"",
                ""Mode"": ""Sticky""
            }
        },
        {
            ""Name"" : ""Application"",
			""Servers"" :
			[
				{
					""Name"" : ""jat-app01""
				},
				{
					""Name"" : ""jat-app02""
				}
			]
        },
        {
            ""Name"" : ""Database"",
			""Servers"" :
			[
				{
					""Name"" : ""jat-db01""
				},
				{
					""Name"" : ""jat-db02""
				}
			]
        }
	],
    ""DeploymentUser"": 
    {
        ""UserName"": ""torresdal\\condepuser"",
        ""Password"": ""verySecureP@ssw0rd""
    },
    ""OperationsConfig"":
    {
        ""NServiceBusOperation"": 
        {
            ""ServiceUserName"": ""torresdal\\nservicebususer"",
            ""ServicePassword"": ""verySecureP@ssw0rd""
        },
        ""SomeOtherOperation"":
        {
            ""SomeOtherSetting1"": ""asdfasdf"",
            ""SomeOtherSetting2"": ""34tsdfg""
        }
    }
}";

        private string _json =
            @"{
    ""LoadBalancer"": 
    {
        ""Name"": ""jat-nlb01"",
        ""Provider"": ""ConDep.Dsl.LoadBalancer.Ace.dll"",
        ""UserName"": ""torresdal\\nlbUser"",
        ""Password"": ""verySecureP@ssw0rd"",
        ""Mode"": ""Sticky"",
		""SuspendMode"" : ""Graceful"",
        ""CustomValues"" :
        [
            {
                ""Key"" : ""AwsSuspendWaitTime"",
                ""Value"" : ""30""
            },
            {
                ""Key"" : ""AwsActivateWaitTime"",
                ""Value"" : ""40""
            }
        ]
    },
    ""PowerShellScriptFolders"" : 
    [
        ""psScripts"",
        ""psScripts\\subScripts"",
        ""psScripts\\subScripts\\subSubScripts""
    ],
	""Servers"":
    [
        {
            ""Name"" : ""jat-web01"",
            ""LoadBalancerFarm"": ""farm1"",
            ""StopServer"": true,
		    ""WebSites"" : 
		    [
			    { 
                    ""Name"" : ""WebSite1"", 
                    ""Bindings"": 
                    [
                        { ""BindingType"": ""http"", ""Port"" : ""80"", ""Ip"" : ""10.0.0.111"", ""HostHeader"" : """" },
                        { ""BindingType"": ""https"", ""Port"" : ""443"", ""Ip"" : ""10.0.0.111"", ""HostHeader"" : """" }
                    ]
                },
			    { 
                    ""Name"" : ""WebSite2"", 
                    ""Bindings"": 
                    [
                        { ""BindingType"": ""http"", ""Port"" : ""80"", ""Ip"" : ""10.0.0.112"", ""HostHeader"" : """" },
                        { ""BindingType"": ""https"", ""Port"" : ""443"", ""Ip"" : ""10.0.0.112"", ""HostHeader"" : """" }
                    ]
                },
			    { 
                    ""Name"" : ""WebSite3"", 
                    ""Bindings"": 
                    [
                        { ""BindingType"": ""http"", ""Port"" : ""80"", ""Ip"" : ""10.0.0.113"", ""HostHeader"" : """" },
                        { ""BindingType"": ""https"", ""Port"" : ""443"", ""Ip"" : ""10.0.0.113"", ""HostHeader"" : """" }
                    ]
                }
			]
        },
        {
            ""Name"" : ""jat-web02"",
            ""LoadBalancerFarm"": ""farm1"",
		    ""WebSites"" : 
		    [
			    { 
                    ""Name"" : ""WebSite1"", 
                    ""Bindings"": 
                    [
                        { ""BindingType"": ""http"", ""Port"" : ""80"", ""Ip"" : ""10.0.0.121"", ""HostHeader"" : """" },
                        { ""BindingType"": ""https"", ""Port"" : ""443"", ""Ip"" : ""10.0.0.121"", ""HostHeader"" : """" }
                    ]
                },
			    { 
                    ""Name"" : ""WebSite2"", 
                    ""Bindings"": 
                    [
                        { ""BindingType"": ""http"", ""Port"" : ""80"", ""Ip"" : ""10.0.0.122"", ""HostHeader"" : """" },
                        { ""BindingType"": ""https"", ""Port"" : ""443"", ""Ip"" : ""10.0.0.122"", ""HostHeader"" : """" }
                    ]
                },
			    { 
                    ""Name"" : ""WebSite3"", 
                    ""Bindings"": 
                    [
                        { ""BindingType"": ""http"", ""Port"" : ""80"", ""Ip"" : ""10.0.0.123"", ""HostHeader"" : """" },
                        { ""BindingType"": ""https"", ""Port"" : ""443"", ""Ip"" : ""10.0.0.123"", ""HostHeader"" : """" }
                    ]
                }
			]
        }
    ],
    ""DeploymentUser"": 
    {
        ""UserName"": ""torresdal\\condepuser"",
        ""Password"": ""verySecureP@ssw0rd""
    },
    ""OperationsConfig"":
    {
        ""NServiceBusOperation"": 
        {
            ""ServiceUserName"": ""torresdal\\nservicebususer"",
            ""ServicePassword"": ""verySecureP@ssw0rd""
        },
        ""SomeOtherOperation"":
        {
            ""SomeOtherSetting1"": ""asdfasdf"",
            ""SomeOtherSetting2"": ""34tsdfg""
        }
    }
}";

        private string _secretsProviderJson =
                    @"{
    ""LoadBalancer"": 
    {
        ""Name"": ""jat-nlb01"",
        ""Provider"": ""ConDep.Dsl.LoadBalancer.Ace.dll"",
        ""UserName"": ""torresdal\\nlbUser"",
        ""Password"": ""verySecureP@ssw0rd"",
        ""Mode"": ""Sticky"",
		""SuspendMode"" : ""Graceful"",
        ""CustomValues"" :
        [
            {
                ""Key"" : ""AwsSuspendWaitTime"",
                ""Value"" : ""30""
            },
            {
                ""Key"" : ""AwsActivateWaitTime"",
                ""Value"" : ""40""
            }
        ]
    },
    ""SecretsProvider"": 
    {
        ""Provider"": ""ConDep.Execution.Tests.dll""
    }
}";

        private ConDepEnvConfig _config;
        private ConDepEnvConfig _tiersConfig;
        private ConDepEnvConfig _secretsProviderConfig;
        private string _cryptoKey;

        [SetUp]
        public void Setup()
        {
            var memStream = new MemoryStream(Encoding.UTF8.GetBytes(_json));
            var tiersMemStream = new MemoryStream(Encoding.UTF8.GetBytes(_tiersJson));
            var secretsProviderMemStream = new MemoryStream(Encoding.UTF8.GetBytes(_secretsProviderJson));

            _cryptoKey = JsonPasswordCrypto.GenerateKey(256);
            var parser = new EnvConfigParser(new JsonSerializer<ConDepEnvConfig>(new JsonConfigCrypto(_cryptoKey)));
            _config = parser.GetTypedEnvConfig(memStream);
            _tiersConfig = parser.GetTypedEnvConfig(tiersMemStream);
            _secretsProviderConfig = parser.GetTypedEnvConfig(secretsProviderMemStream);
        }

        [Test]
        public void TestThatLoadBalancerExist()
        {
            Assert.That(_config.LoadBalancer, Is.Not.Null);
        }

        [Test]
        public void TestThatLoadBalancerHasValuesInAllFields()
        {
            Assert.That(_config.LoadBalancer.Name, Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.Password, Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.Provider, Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.UserName, Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.Mode, Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.GetModeAsEnum(), Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.SuspendMode, Is.Not.Null.Or.Empty);
            Assert.That(_config.LoadBalancer.GetSuspendModeAsEnum(), Is.Not.Null.Or.Empty);
        }

        [Test]
        public void TestThatEmptyPowerShellScriptFoldersIsNotNullAndEmpty()
        {
            var config = new ConDepEnvConfig();
            Assert.That(config.PowerShellScriptFolders, Is.Not.Null);
            Assert.That(config.PowerShellScriptFolders.Length, Is.EqualTo(0));
        }

        [Test]
        public void TestThatPowerShellScriptFoldersCanBeIterated()
        {
            Assert.That(_config.PowerShellScriptFolders.Length, Is.EqualTo(3));
        }

        [Test]
        public void TestThatDeploymentUserExist()
        {
            Assert.That(_config.DeploymentUser, Is.Not.Null);
        }

        [Test]
        public void TestThatDeploymentUserHasValuesInAllFields()
        {
            Assert.That(_config.DeploymentUser.UserName, Is.Not.Null.Or.Empty);
            Assert.That(_config.DeploymentUser.Password, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void TestThatSecretsProviderSuppliesCredentials()
        {
            Assert.That(_secretsProviderConfig.SecretsProvider, Is.Not.Null);
            Assert.That(_secretsProviderConfig.DeploymentUser.UserName, Is.EqualTo("username_from_secrets_provider"));
            Assert.That(_secretsProviderConfig.DeploymentUser.Password, Is.EqualTo("password_from_secrets_provider"));
        }

        [Test]
        public void TestThatOperationsConfigExist()
        {
            Assert.That(_config.OperationsConfig, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void TestThatNServiceBusOperationHasValues()
        {
            Assert.That(_config.OperationsConfig.NServiceBusOperation, Is.Not.Null);
        }

        [Test]
        public void TestThatSomeOtherProviderHasValues()
        {
            Assert.That(_config.OperationsConfig.SomeOtherOperation, Is.Not.Null);
        }

        [Test]
        public void TestThatServersExist()
        {
            Assert.That(_config.Servers, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void TestThatServersContainsExactlyTwo()
        {
            Assert.That(_config.Servers.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestThatServersHaveNames()
        {
            foreach (var server in _config.Servers)
            {
                Assert.That(server.Name, Is.Not.Null.Or.Empty);
            }
        }

        [Test]
        public void TestThatOnlyOneServerIsTestServer()
        {
            var value = _config.Servers.Single(x => x.StopServer);
            Assert.That(value.StopServer);
        }

        [Test]
        public void TestThatServersHaveFarm()
        {
            foreach(var server in _config.Servers)
            {
                Assert.That(server.LoadBalancerFarm, Is.Not.Null.Or.Empty);
            }
        }

        [Test]
        public void TestThatRootDeploymentUserIsInheritedForServersUnlessExplicitlyDefined()
        {
            foreach (var server in _config.Servers)
            {
                Assert.That(server.DeploymentUser, Is.SameAs(_config.DeploymentUser));
            }
        }

        [Test]
        public void TestThatUnencryptedJsonIsNotIdentifiedAsEncrypted()
        {
            var crypto = new JsonConfigCrypto(_cryptoKey);
            Assert.That(crypto.IsEncrypted(_json), Is.False);
            Assert.That(crypto.IsEncrypted(_tiersJson), Is.False);
        }

        [Test]
        public void TestThatEncryptedJsonCanBeDecryptedIntoTypedConfig()
        {
            //var parser = new EnvConfigParser(new ConfigJsonSerializer(new JsonConfigCrypto()), new JsonConfigCrypto());
            var cryptoHandler = new JsonConfigCrypto(_cryptoKey);
            var serializer = new JsonSerializer<ConDepEnvConfig>(cryptoHandler);

            //parser.Encrypted(_json, out config);
            var config = serializer.DeSerialize(_json);

            string deploymentPassword = config.DeploymentUser.Password;
            string lbPassword = config.LoadBalancer.Password;

            var encryptedJson = cryptoHandler.Encrypt(_json);

            //parser.EncryptJsonConfig(config, crypto);

            //var encryptedJson = parser.ConvertToJsonText(config);
            Assert.That(cryptoHandler.IsEncrypted(encryptedJson), Is.True);

            var decryptedConfig = serializer.DeSerialize(encryptedJson);

            Assert.That(decryptedConfig.DeploymentUser.Password, Is.EqualTo(deploymentPassword));
            Assert.That(decryptedConfig.LoadBalancer.Password, Is.EqualTo(lbPassword));
        }

        [Test]
        public void TestThatEncryptTagGetsEncrypted()
        {
            var crypto = new JsonConfigCrypto(_cryptoKey);
            var encryptedJson = crypto.Encrypt(_encryptJson);

            var decryptedJson = crypto.Decrypt(encryptedJson);

            var serializer = new JsonSerializer<ConDepEnvConfig>(crypto);
            serializer.DeSerialize(decryptedJson);
        }
    }
}