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
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;
        private readonly IInputOperations _operations;
        private readonly IPointerTargetOperations _pointerTargetOperations;

        public SendMouseEventUseCase(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher,
            IInputOperations operations, IPointerTargetOperations pointerTargetOperations)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
            _operations = operations;
            _pointerTargetOperations = pointerTargetOperations;
        }

        public async Task<SendMouseEventResponse> ExecuteAsync(float x, float y, string button, string eventType,
            CancellationToken cancellationToken = default)
        {
            await SendAsync(x, y, button, eventType, cancellationToken);
            return new SendMouseEventResponse(true, x, y, x, y);
        }

        // Sends the event to the center of the UI object with the given instanceId.
        public async Task<SendMouseEventResponse> ExecuteAsync(int instanceId, string button,
            string eventType, CancellationToken cancellationToken = default)
        {
            var (x, y) = await ResolveAsync(MousePosition.CenterOf(instanceId), cancellationToken);

            await SendAsync(x, y, button, eventType, cancellationToken);
            return new SendMouseEventResponse(true, x, y, x, y);
        }

        // Presses at the start, keeps the button pressed for holdFrames frames, moves along a straight line to the
        // end over the given number of frames (one move per frame), and releases at the end.
        // Completes after the release has been processed, so the next request sees the result.
        public async Task<SendMouseEventResponse> DragAsync(MousePosition start, MousePosition end, string button,
            int frames, int holdFrames, CancellationToken cancellationToken = default)
        {
            if (frames < 1)
            {
                throw new ArgumentException("frames must be 1 or greater.");
            }

            if (holdFrames < 0)
            {
                throw new ArgumentException("holdFrames must be 0 or greater.");
            }

            var (x, y) = await ResolveAsync(start, cancellationToken);
            var (toX, toY) = await ResolveAsync(end, cancellationToken);

            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunEachFrameAsync(
                    frame => DragStep(frame, x, y, toX, toY, button, frames, holdFrames), cancellationToken),
                cancellationToken);
            await task;

            return new SendMouseEventResponse(true, x, y, toX, toY);
        }

        // Runs one frame of a drag and returns false when the drag is done.
        // Frame 0 presses, the next holdFrames frames only wait, the next `frames` frames move, the next frame
        // releases, and the last frame waits so that the release is processed in it.
        private bool DragStep(int frame, float x, float y, float toX, float toY, string button, int frames,
            int holdFrames)
        {
            if (frame == 0)
            {
                _operations.SendMouseEvent(x, y, button, InputEventType.Press);
                return true;
            }

            var move = frame - holdFrames;
            if (move <= 0)
            {
                return true;
            }

            if (move <= frames)
            {
                var t = (float)move / frames;
                _operations.SendMouseEvent(x + (toX - x) * t, y + (toY - y) * t, button, InputEventType.Move);
                return true;
            }

            if (move == frames + 1)
            {
                _operations.SendMouseEvent(toX, toY, button, InputEventType.Release);
                return true;
            }

            return false;
        }

        private async Task<(float x, float y)> ResolveAsync(MousePosition position,
            CancellationToken cancellationToken)
        {
            if (position.InstanceId is not { } instanceId)
            {
                return (position.X, position.Y);
            }

            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _pointerTargetOperations.GetTargetCenterAsync(instanceId, cancellationToken),
                cancellationToken);
            return await task;
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
