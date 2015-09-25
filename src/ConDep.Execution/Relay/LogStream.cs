using System;
using System.IO;
using System.Runtime.Serialization;

namespace ConDep.Execution.Relay
{
    [Serializable]
    public class LogStream : MemoryStream
    {
        [DataMember]
        public bool ExecutionFinished { get; set; }
    }
}