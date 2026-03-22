using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    internal sealed class CaptureSceneViewUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ICaptureOperations _operations;

        public CaptureSceneViewUseCase(IMainThreadDispatcher dispatcher, ICaptureOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<byte[]> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            return await _dispatcher.RunOnMainThreadAsync(
                () => _operations.CaptureSceneView(), cancellationToken);
        }
    }
}
