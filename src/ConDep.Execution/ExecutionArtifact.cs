using System;
using System.Net;

namespace ConDep.Execution
{
    [Serializable]
    public class ExecutionArtifact
    {
        public Uri Url { get; set; }
        public NetworkCredential Credentials { get; set; }
        public string RelativeTargetPath { get; set; }
    }
}