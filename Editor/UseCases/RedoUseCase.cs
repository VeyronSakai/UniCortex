using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.UseCases
{
    internal sealed class RedoUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IUndo _undo;

        public RedoUseCase(IMainThreadDispatcher dispatcher, IUndo undo)
        {
            _dispatcher = dispatcher;
            _undo = undo;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                _undo.PerformRedo();
                Debug.Log("[UniCortex] Redo");
            }, cancellationToken);
        }
    }
}
