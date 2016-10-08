using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ConDep.Execution.Tests
{
    [TestFixture]
    public class PsExecutorTests
    {
        [Test]
        public void TestThat_Something()
        {
            var versionResult = GetExecutionResult();

            //---------------------------
            dynamic version = versionResult.First();
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
