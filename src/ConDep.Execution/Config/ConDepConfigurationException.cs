using System;

namespace ConDep.Execution.Config
{
    /// <summary>
    /// Exception that is thrown when errors is found in a ConDep configuration file
    /// </summary>
    public class ConDepConfigurationException : Exception
    {
        /// <summary>
        /// Creates a new instance of the exception with message
        /// </summary>
        /// <param name="message"></param>
        public ConDepConfigurationException(string message) : base(message)
        {
            
        }
    }
}