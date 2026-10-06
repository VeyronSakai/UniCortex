using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Converts a PointerPosition into Game View coordinates.
    internal sealed class PointerPositionResolver
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IUiPointerTargetOperations _uiPointerTargetOperations;

        public PointerPositionResolver(IMainThreadDispatcher dispatcher,
            IUiPointerTargetOperations uiPointerTargetOperations)
        {
            _dispatcher = dispatcher;
            _uiPointerTargetOperations = uiPointerTargetOperations;
        }

        public async Task<(float x, float y)> ResolveAsync(PointerPosition position,
            CancellationToken cancellationToken)
        {
            switch (position)
            {
                case PointerPosition.Coordinates coordinates:
                    return (coordinates.X, coordinates.Y);

                case PointerPosition.Target target:
                    var task = await _dispatcher.RunOnMainThreadAsync(
                        () => _uiPointerTargetOperations.GetTargetCenterAsync(target.InstanceId, cancellationToken),
                        cancellationToken);
                    return await task;

                default:
                    throw new ArgumentException($"Unsupported pointer position: {position}.");
            }
        }
    }
}
