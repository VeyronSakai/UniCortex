using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ClearConsoleLogsUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IConsoleLogCollector _collector;

        public ClearConsoleLogsUseCase(IMainThreadDispatcher dispatcher, IConsoleLogCollector collector)
        {
            _dispatcher = dispatcher;
            _collector = collector;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() => _collector.Clear(), cancellationToken);
        }
    }
}
