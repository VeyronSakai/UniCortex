using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEngine;

namespace UniCortex.Editor.UseCases
{
    internal sealed class UndoUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IUndo _undo;

        public UndoUseCase(IMainThreadDispatcher dispatcher, IUndo undo)
        {
            _dispatcher = dispatcher;
            _undo = undo;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            await _dispatcher.RunOnMainThreadAsync(() =>
            {
                _undo.PerformUndo();
                Debug.Log("[UniCortex] Undo");
            }, cancellationToken);
        }
    }
}
