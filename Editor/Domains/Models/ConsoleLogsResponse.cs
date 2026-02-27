using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class ConsoleLogsResponse
    {
        public List<ConsoleLogEntry> logs;

        public ConsoleLogsResponse(List<ConsoleLogEntry> logs)
        {
            this.logs = logs;
        }
    }
}
