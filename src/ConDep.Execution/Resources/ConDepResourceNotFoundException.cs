using System;

namespace ConDep.Execution.Resources
{
    public class ConDepResourceNotFoundException : Exception
    {
        public ConDepResourceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        public ConDepResourceNotFoundException(string message) : base(message) { }
    }
}