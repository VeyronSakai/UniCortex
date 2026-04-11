#nullable enable

using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class CustomToolManifestEntry
    {
        public string name;
        public string description;
        public string cliCommand;
        public bool exposeToMcp;
        public bool exposeToCli;
        public CustomToolParameterDefinition[] parameters;

        public CustomToolManifestEntry(
            string name,
            string description,
            string cliCommand,
            bool exposeToMcp,
            bool exposeToCli,
            CustomToolParameterDefinition[] parameters)
        {
            this.name = name;
            this.description = description;
            this.cliCommand = cliCommand;
            this.exposeToMcp = exposeToMcp;
            this.exposeToCli = exposeToCli;
            this.parameters = parameters ?? Array.Empty<CustomToolParameterDefinition>();
        }
    }
}
