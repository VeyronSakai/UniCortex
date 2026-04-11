using System;

namespace UniCortex.Editor.Extensibility
{
    [Serializable]
    public class UniCortexCustomToolDefinition
    {
        public string name;
        public string description;
        public string cliCommand;
        public bool exposeToMcp;
        public bool exposeToCli;

        public UniCortexCustomToolDefinition(
            string name,
            string description,
            bool exposeToMcp = true,
            bool exposeToCli = true,
            string cliCommand = "")
        {
            this.name = name;
            this.description = description;
            this.cliCommand = cliCommand;
            this.exposeToMcp = exposeToMcp;
            this.exposeToCli = exposeToCli;
        }
    }
}
