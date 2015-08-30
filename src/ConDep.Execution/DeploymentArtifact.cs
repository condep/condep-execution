using System;
using System.Net;

namespace ConDep.Execution
{
    [Serializable]
    public class DeploymentArtifact
    {
        public Uri Url { get; set; }
        public NetworkCredential Credentials { get; set; }
        public string RelativeTargetPath { get; set; }
    }
}