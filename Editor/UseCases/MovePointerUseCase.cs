using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Moves the pointer to the position without changing the button state.
    internal sealed class MovePointerUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;

        public MovePointerUseCase(IMainThreadDispatcher dispatcher, PointerPositionResolver resolver,
            IInputOperations operations)
        {
            _dispatcher = dispatcher;
            _resolver = resolver;
            _operations = operations;
        }

        public async Task<PointerResponse> ExecuteAsync(PointerPosition position,
            CancellationToken cancellationToken = default)
        {
            var (x, y) = await _resolver.ResolveAsync(position, cancellationToken);

            await _dispatcher.RunOnMainThreadAsync(() => _operations.MoveMouse(x, y), cancellationToken);
            return new PointerResponse(true, x, y);
        }
    }
}
