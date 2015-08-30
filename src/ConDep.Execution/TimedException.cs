using System;

namespace ConDep.Execution
{
    [Serializable]
    public class TimedException
    {
        public DateTime DateTime { get; set; }
        public Exception Exception { get; set; }
    }
}