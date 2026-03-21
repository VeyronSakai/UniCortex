using System;

namespace UniCortex.Editor.Domains.Models
{
    [Serializable]
    public class GetConsoleLogsRequest
    {
        public int? count;
        public bool? stackTrace;
        public bool? log;
        public bool? warning;
        public bool? error;
    }
}
