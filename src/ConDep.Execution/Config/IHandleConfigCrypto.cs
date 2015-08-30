namespace ConDep.Execution.Config
{
    /// <summary>
    /// Encrypt and decrypt ConDep configuration files
    /// </summary>
    public interface IHandleConfigCrypto
    {
        /// <summary>
        /// Decrypt configuration
        /// </summary>
        /// <param name="config">Configuration to decrypt</param>
        /// <returns></returns>
        string Decrypt(string config);

        /// <summary>
        /// Decrypt configuration file. 
        /// </summary>
        /// <param name="filePath">Path to configuration file</param>
        void DecryptFile(string filePath);

        /// <summary>
        /// Encrypt configuration
        /// </summary>
        /// <param name="config">Configuration to encrypt</param>
        /// <returns></returns>
        string Encrypt(string config);

        /// <summary>
        /// Encrypt configuration File
        /// </summary>
        /// <param name="filePath">Path to configuration file</param>
        void EncryptFile(string filePath);

        /// <summary>
        /// Tells if configuration is already encrypted
        /// </summary>
        /// <param name="config">Configuration to check for encryption</param>
        /// <returns></returns>
        bool IsEncrypted(string config);
    }
}