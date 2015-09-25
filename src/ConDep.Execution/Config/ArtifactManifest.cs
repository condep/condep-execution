using System;
using System.Collections.Generic;
using ConDep.Execution.Relay;

namespace ConDep.Execution.Config
{
    [Serializable]
    public class ArtifactManifest
    {
        private readonly IEnumerable<ApplicationArtifact> _apps = new List<ApplicationArtifact>();

        public ApplicationArtifact ConPack { get; set; }
        public IEnumerable<ApplicationArtifact> Apps { get { return _apps; } }
    }
}