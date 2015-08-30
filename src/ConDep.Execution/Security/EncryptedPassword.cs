namespace ConDep.Execution.Security
{
    internal class EncryptedValue
    {
        public EncryptedValue(string iv, string value)
        {
            IV = iv;
            Value = value;
        }

        public string IV { get; private set; }

        public string Value { get; private set; }
    }
}