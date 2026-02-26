using System.Collections.Generic;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.Domains.Interfaces
{
    internal interface IConsoleLogCollector
    {
        List<ConsoleLogEntry> GetLogs(int count, bool includeStackTrace = false,
            bool showLog = true, bool showWarning = true, bool showError = true);
        void Clear();
    }
}
