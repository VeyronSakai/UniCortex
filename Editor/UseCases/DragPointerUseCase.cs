using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;

namespace UniCortex.Editor.UseCases
{
    // Presses at the start, keeps the button pressed for holdFrames frames, moves along a straight line to the end
    // over the given number of frames (one move per frame), and releases at the end.
    // Completes after the release has been processed, so the next request sees the result.
    internal sealed class DragPointerUseCase
    {
        public const int DefaultFrames = 10;

        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;
        private readonly PointerPositionResolver _resolver;
        private readonly IInputOperations _operations;

        public DragPointerUseCase(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher,
            PointerPositionResolver resolver, IInputOperations operations)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
            _resolver = resolver;
            _operations = operations;
        }

        public async Task<DragPointerResponse> ExecuteAsync(PointerPosition start, PointerPosition end,
            string button, int frames, int holdFrames, CancellationToken cancellationToken = default)
        {
            if (frames < 1)
            {
                throw new ArgumentException("frames must be 1 or greater.");
            }

            if (holdFrames < 0)
            {
                throw new ArgumentException("holdFrames must be 0 or greater.");
            }

            var (x, y) = await _resolver.ResolveAsync(start, cancellationToken);
            var (toX, toY) = await _resolver.ResolveAsync(end, cancellationToken);

            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunEachFrameAsync(
                    frame => Step(frame, x, y, toX, toY, button, frames, holdFrames), cancellationToken),
                cancellationToken);
            await task;

            return new DragPointerResponse(true, x, y, toX, toY);
        }

        // Runs one frame of a drag and returns false when the drag is done.
        // Frame 0 presses, the next holdFrames frames only wait, the next `frames` frames move, the next frame
        // releases, and the last frame waits so that the release is processed in it.
        private bool Step(int frame, float x, float y, float toX, float toY, string button, int frames,
            int holdFrames)
        {
            if (frame == 0)
            {
                _operations.PressMouseButton(x, y, button);
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
                _operations.MoveMouse(x + (toX - x) * t, y + (toY - y) * t);
                return true;
            }

            if (move == frames + 1)
            {
                _operations.ReleaseMouseButton(toX, toY, button);
                return true;
            }

            return false;
        }
    }
}
