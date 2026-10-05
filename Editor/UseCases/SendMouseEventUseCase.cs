using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    internal sealed class SendMouseEventUseCase
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IInputOperations _operations;
        private readonly IPointerTargetOperations _pointerTargetOperations;

        public SendMouseEventUseCase(IMainThreadDispatcher dispatcher,
            IInputOperations operations, IPointerTargetOperations pointerTargetOperations)
        {
            _dispatcher = dispatcher;
            _operations = operations;
            _pointerTargetOperations = pointerTargetOperations;
        }

        public async Task<SendMouseEventResponse> ExecuteAsync(float x, float y, string button, string eventType,
            CancellationToken cancellationToken = default)
        {
            await SendAsync(x, y, button, eventType, cancellationToken);
            return new SendMouseEventResponse(true, x, y);
        }

        // Sends the event to the center of the UI object with the given instanceId.
        public async Task<SendMouseEventResponse> ExecuteAsync(int instanceId, string button,
            string eventType, CancellationToken cancellationToken = default)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _pointerTargetOperations.GetTargetCenterAsync(instanceId, cancellationToken),
                cancellationToken);
            var (x, y) = await task;

            await SendAsync(x, y, button, eventType, cancellationToken);
            return new SendMouseEventResponse(true, x, y);
        }

        private async Task SendAsync(float x, float y, string button, string eventType,
            CancellationToken cancellationToken)
        {
            if (string.Equals(eventType, InputEventType.Click, StringComparison.OrdinalIgnoreCase))
            {
                await _dispatcher.RunOnMainThreadAsync(
                    () => _operations.SendMouseEvent(x, y, button, InputEventType.Press),
                    cancellationToken);
                await _dispatcher.RunOnMainThreadAsync(
                    () => _operations.SendMouseEvent(x, y, button, InputEventType.Release),
                    cancellationToken);
            }
            else
            {
                await _dispatcher.RunOnMainThreadAsync(
                    () => _operations.SendMouseEvent(x, y, button, eventType), cancellationToken);
            }
        }
    }
}
