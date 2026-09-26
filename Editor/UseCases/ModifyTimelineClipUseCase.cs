using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class ModifyTimelineClipUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly ITimelineOperations _operations;

        public ModifyTimelineClipUseCase(IMainThreadDispatcher dispatcher, ITimelineOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task ExecuteAsync(ModifyTimelineClipRequest request, CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() => _operations.ModifyClip(request), cancellationToken);
        }
    }
}
