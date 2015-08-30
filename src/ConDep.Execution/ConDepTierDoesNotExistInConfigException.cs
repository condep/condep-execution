using System;

namespace ConDep.Execution
{
    /// <summary>
    /// Exception that is thrown when a expected Tier does not exist in ConDep configuration
    /// </summary>
    public class ConDepTierDoesNotExistInConfigException : Exception
    {
        /// <summary>
        /// Creates a new instance of the exception with message
        /// </summary>
        /// <param name="message"></param>
        public ConDepTierDoesNotExistInConfigException(string message) : base(message)
        {
        }
    }
}