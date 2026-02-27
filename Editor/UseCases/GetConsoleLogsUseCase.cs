using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class GetConsoleLogsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IConsoleLogCollector _collector;

        public GetConsoleLogsUseCase(IMainThreadDispatcher dispatcher, IConsoleLogCollector collector)
        {
            _dispatcher = dispatcher;
            _collector = collector;
        }

        public async Task<List<ConsoleLogEntry>> ExecuteAsync(int count, bool includeStackTrace,
            bool showLog, bool showWarning, bool showError,
            CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _collector.GetLogs(count, includeStackTrace, showLog, showWarning, showError),
                cancellationToken);
        }
    }
}
