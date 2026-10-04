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

        // Sends the event to the center of a UI object given by instanceId (when non-zero) or Hierarchy path.
        public async Task<SendMouseEventResponse> ExecuteAsync(int targetInstanceId, string targetPath,
            string button, string eventType, CancellationToken cancellationToken = default)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _pointerTargetOperations.GetPointerTargetAsync(targetInstanceId, targetPath, cancellationToken),
                cancellationToken);
            var target = await task;

            await SendAsync(target.centerX, target.centerY, button, eventType, cancellationToken);
            return new SendMouseEventResponse(true, target.centerX, target.centerY, target.blocked,
                target.blockedBy);
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
