using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ConsoleLogEntry
    {
        public string message;
        public string stackTrace;
        public string type;
        public string timestamp;

        public ConsoleLogEntry(string message, string stackTrace, string type, string timestamp)
        {
            this.message = message;
            this.stackTrace = stackTrace;
            this.type = type;
            this.timestamp = timestamp;
        }
    }
}
