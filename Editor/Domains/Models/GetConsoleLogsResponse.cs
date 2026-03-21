using System;
using System.Collections.Generic;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetConsoleLogsResponse
    {
        public List<ConsoleLogEntry> logs;

        public GetConsoleLogsResponse(List<ConsoleLogEntry> logs)
        {
            this.logs = logs;
        }
    }
}
