using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ConsoleLogEntry
    {
        public string message;
        public string stackTrace;
        public string type;

        public ConsoleLogEntry(string message, string stackTrace, string type)
        {
            this.message = message;
            this.stackTrace = stackTrace;
            this.type = type;
        }
    }
}
