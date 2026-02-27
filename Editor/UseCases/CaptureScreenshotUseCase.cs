using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CaptureScreenshotUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IUtilityOperations _operations;

        public CaptureScreenshotUseCase(IMainThreadDispatcher dispatcher, IUtilityOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<byte[]> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.CaptureScreenshot(), cancellationToken);
        }
    }
}
