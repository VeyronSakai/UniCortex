using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses a pointer button at the position and keeps it pressed.
    internal sealed class PressPointerUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;

        public PressPointerUseCase(IMainThreadDispatcher dispatcher, PointerPositionResolver resolver,
            IInputOperations operations)
        {
            _dispatcher = dispatcher;
            _resolver = resolver;
            _operations = operations;
        }

        public async Task<PointerResponse> ExecuteAsync(PointerPosition position, string button,
            CancellationToken cancellationToken = default)
        {
            var (x, y) = await _resolver.ResolveAsync(position, cancellationToken);

            await _dispatcher.RunOnMainThreadAsync(
                () => _operations.PressMouseButton(x, y, button), cancellationToken);
            return new PointerResponse(true, x, y);
        }
    }
}
