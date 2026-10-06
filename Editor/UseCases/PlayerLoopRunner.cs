using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.UseCases
{
    // Runs steps of input simulation inside the player loop (see IPlayerLoopDispatcher) from any thread.
    // Each call runs in a later frame than the previous one; a frame may be skipped between calls, because each
    // call goes back to the main thread through IMainThreadDispatcher first.
    internal sealed class PlayerLoopRunner
    {
        private readonly IMainThreadDispatcher _dispatcher;
        private readonly IPlayerLoopDispatcher _playerLoopDispatcher;

        public PlayerLoopRunner(IMainThreadDispatcher dispatcher, IPlayerLoopDispatcher playerLoopDispatcher)
        {
            _dispatcher = dispatcher;
            _playerLoopDispatcher = playerLoopDispatcher;
        }

        public async Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunAsync(func, cancellationToken), cancellationToken);
            return await task;
        }

        public async Task RunAsync(Action action, CancellationToken cancellationToken)
        {
            var task = await _dispatcher.RunOnMainThreadAsync(
                () => _playerLoopDispatcher.RunAsync(action, cancellationToken), cancellationToken);
            await task;
        }

        // Waits until the input queued by the previous step has been processed by the game.
        // Input queued in a frame is processed in the next frame, before MonoBehaviour.Update and the EventSystem
        // (e.g. wasReleasedThisFrame, Button.onClick). Waiting until a later frame lets the request return after
        // the game has reacted to it, so that the next request (e.g. capture_game_view) sees the result.
        public Task WaitForInputProcessedAsync(CancellationToken cancellationToken)
        {
            return RunAsync(() => { }, cancellationToken);
        }
    }
}
