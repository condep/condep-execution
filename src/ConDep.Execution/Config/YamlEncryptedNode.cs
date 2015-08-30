using System.Collections.Generic;
using ConDep.Execution.Security;
using YamlDotNet.RepresentationModel;

namespace ConDep.Execution.Config
{
    internal class YamlEncryptedNode
    {
        public YamlMappingNode Parent { get; set; }
        public KeyValuePair<YamlNode, YamlNode> Container { get; set; }

        public EncryptedValue EncryptedValue
        {
            get
            {
                return new EncryptedValue(((YamlMappingNode)Container.Value).Children[new YamlScalarNode("IV")].ToString(), ((YamlMappingNode)Container.Value).Children[new YamlScalarNode("Value")].ToString());
            }
        }
    }
}