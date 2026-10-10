using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SetPlayModeViewTypeUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayModeViewOperations _operations;

        public SetPlayModeViewTypeUseCase(IMainThreadDispatcher dispatcher, IPlayModeViewOperations operations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
        }

        public async Task<SetPlayModeViewTypeResponse> ExecuteAsync(string viewType,
            CancellationToken cancellationToken)
        {
            if (viewType != PlayModeViewTypes.GameView && viewType != PlayModeViewTypes.SimulatorView)
            {
                throw new ArgumentException(
                    $"Unknown view type: '{viewType}'. " +
                    $"Valid values: {PlayModeViewTypes.GameView}, {PlayModeViewTypes.SimulatorView}.");
            }

            var current = await _dispatcher.RunOnMainThreadAsync(() =>
            {
                _operations.SetViewType(viewType);
                return _operations.GetViewType();
            }, cancellationToken);
            return new SetPlayModeViewTypeResponse(current);
        }
    }
}
