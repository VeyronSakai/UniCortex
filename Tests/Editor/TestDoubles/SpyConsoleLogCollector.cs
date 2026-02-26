using System.Collections.Generic;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Tests.TestDoubles
{
    internal sealed class SpyConsoleLogCollector : IConsoleLogCollector
    {
        public int GetLogsCallCount { get; private set; }
        public int LastCount { get; private set; }
        public bool LastIncludeStackTrace { get; private set; }
        public bool LastShowLog { get; private set; }
        public bool LastShowWarning { get; private set; }
        public bool LastShowError { get; private set; }
        public int ClearCallCount { get; private set; }

        private List<ConsoleLogEntry> _logs = new List<ConsoleLogEntry>();

        public void SetLogs(List<ConsoleLogEntry> logs)
        {
            _logs = logs;
        }

        public List<ConsoleLogEntry> GetLogs(int count, bool includeStackTrace = false,
            bool showLog = true, bool showWarning = true, bool showError = true)
        {
            GetLogsCallCount++;
            LastCount = count;
            LastIncludeStackTrace = includeStackTrace;
            LastShowLog = showLog;
            LastShowWarning = showWarning;
            LastShowError = showError;
            return _logs;
        }

        public void Clear()
        {
            ClearCallCount++;
        }
    }
}
