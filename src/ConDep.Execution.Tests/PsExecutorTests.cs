using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using ConDep.Dsl.Config;
using ConDep.Dsl.Remote;
using ConDep.Dsl.Tests;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using NUnit.Framework;

namespace ConDep.Execution.Tests
{
    [TestFixture]
    public class PsExecutorTests
    {
        private UnitTestLogger CreateMemoryLogger()
        {
            var memAppender = new MemoryAppender { Name = "MemoryAppender" };
            memAppender.ActivateOptions();

            var repo = LogManager.GetRepository() as Hierarchy;
            repo.Root.AddAppender(memAppender);
            repo.Configured = true;
            repo.RaiseConfigurationChanged(EventArgs.Empty);

            return new UnitTestLogger(LogManager.GetLogger("root"), memAppender);
        }

        [Test]
        [Ignore]
        public void TestThat_Something()
        {
            ConDep.Dsl.Logging.Logger.Initialize(CreateMemoryLogger());

            var executor = new PowerShellExecutor();
            var result = executor.ExecuteLocal(new ServerConfig(), "$psVersionTable.PSVersion.Major", load => load.LoadConDepModule = false);

            //var versionResult = GetExecutionResult();

            ////---------------------------
            var version = result.First();
            //dynamic version = ((Collection<PSObject>)versionResult).First();
            Assert.That(version >= 3);
        }

        private IEnumerable<dynamic> GetExecutionResult()
        {
            Collection<PSObject> col = new Collection<PSObject>();
            col.Add(3);

            return col;
        }
    }

}
