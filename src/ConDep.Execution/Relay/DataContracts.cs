using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ConDep.Dsl.Config;

namespace ConDep.Execution.Relay
{
    [DataContract]
    public class RelaySettings
    {
        [DataMember]
        public ApplicationArtifact ConPackArtifact { get; set; }

        [DataMember]
        public ApplicationArtifact[] ApplicationArtifacts { get; set; }
        
        [DataMember]
        public DeployOptions ConDepOptions { get; set; }
    }

    [DataContract]
    public class ApplicationArtifact
    {
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public ClearTextCredentials Credentials { get; set; }
        
        [DataMember]
        public string RelativeTargetPath { get; set; }
        
        [DataMember]
        public Uri Url { get; set; }
    }

    [DataContract]
    public class ClearTextCredentials
    {
        [DataMember]
        public string Username { get; set; }
        
        [DataMember]
        public string Password { get; set; }
    }

    [DataContract]
    public class DeployOptions
    {
        [DataMember]
        public string Runbook { get; set; }

        [DataMember]
        public bool StopAfterMarkedServer { get; set; }
        
        [DataMember]
        public bool ContinueAfterMarkedServer { get; set; }
        
        [DataMember]
        public string TraceLevel { get; set; }
        
        [DataMember]
        public string WebQAddress { get; set; }
        
        [DataMember]
        public bool BypassLB { get; set; }
        
        [DataMember]
        public string Environment { get; set; }
        
        [DataMember]
        public string AssemblyName { get; set; }
        
        [DataMember]
        public string CryptoKey { get; set; }
        
        [DataMember]
        public bool DryRun { get; set; }
        
        [DataMember]
        public bool SkipHarvesting { get; set; }

        public bool HasRunbookDefined()
        {
            return !string.IsNullOrWhiteSpace(Runbook);
        }

        public void ValidateMandatoryOptions()
        {
            var missingOptions = new List<string>();
            if (string.IsNullOrWhiteSpace(AssemblyName)) missingOptions.Add("AssemblyName");
            if (string.IsNullOrWhiteSpace(Environment)) missingOptions.Add("Environment");
            if (string.IsNullOrWhiteSpace(Runbook)) missingOptions.Add("Runbook");

            if (missingOptions.Any())
            {
                throw new ConDepMissingOptionsException(missingOptions);
            }
        }

    }

    [DataContract]
    public class ExecutionStatus
    {
        [DataMember]
        public Guid ExecutionId { get; set; }

        [DataMember]
        public bool Finished { get; set; }

        [DataMember]
        public bool Cancelled { get; set; }

        [DataMember]
        public bool Failed { get; set; }

        [DataMember]
        public DateTime Started { get; set; }

        [DataMember]
        public DateTime Ended { get; set; }
    }

    [DataContract]
    public class ExecutionStartedStatus
    {
        [DataMember]
        public Guid ExecutionId { get; set; }

        [DataMember]
        public DateTime Started { get; set; }

        [DataMember]
        public List<Link> SeeOther { get; set; } 
    }

    [DataContract]
    public class Link
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }

    [DataContract]
    public class ExecutionLog
    {
        [DataMember]
        public long Start { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public long End { get; set; }

        [DataMember]
        public bool Finished { get; set; }
    }

}