using System;
using System.IO;

namespace ConDep.Execution.Security
{
    /// <summary>
    /// The exception that is thrown when failing to encrypt/decrypt a ConDep configuration file 
    /// </summary>
    public class ConDepCryptoException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the crypto exception with a message
        /// </summary>
        /// <param name="message"></param>
        public ConDepCryptoException(string message)
            : base(message)
        {
        }
    }
}