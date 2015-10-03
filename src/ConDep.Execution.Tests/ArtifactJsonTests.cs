using ConDep.Execution.Config;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ConDep.Execution.Tests
{
    [TestFixture]
    public class ArtifactJsonTests
    {
        private string _json = @"
{
    ""conpack"": {
        ""rest_bearer"": {
            ""url"": ""https://ci.appveyor.com/api/buildjobs/diuxqy3gps4l0ffu/artifacts/src/VM.Deploy/bin/Release.zip"",
            ""token"": ""hkh93w7y13nanmbt92by"",
            ""relativeTargetPath"": ""ConDep"",
            ""name"": ""vm-deploy""
        }
    },
    ""apps"": [
        {
            ""http_basic"": {
                ""url"": ""https://appveyor.com/download/?item=lkjlkjoijoijr"",
                ""credentials"": {
                    ""username"": ""admin"",
                    ""password"": ""alskdfjalksdjflkajsdflk""
                },
                ""relativeTargetPath"": ""myApp"",
                ""name"": ""My application""
            }
        }
    ]
}";
        [Test]
        public void TestThat_alsdkj()
        {
            var manifest = JsonConvert.DeserializeObject<ArtifactManifest>(_json);
        }
    }
}