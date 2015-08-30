using System;

namespace ConDep.Execution.Config
{
    /// <summary>
    /// Exception that is thrown when ConDep can't find any servers to deploy to.
    /// </summary>
    public class ConDepNoServersFoundException : Exception
    {
        /// <summary>
        /// Creates a new instance of the exception with a message
        /// </summary>
        /// <param name="message"></param>
        public ConDepNoServersFoundException(string message) : base(message) {}
    }
}