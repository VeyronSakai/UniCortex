using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine.PlayerLoop;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class PlayerLoopDispatcher : IPlayerLoopDispatcher
    {
        private readonly struct Request
        {
            public readonly Action Run;
            public readonly Action<Exception> Fail;

            public Request(Action run, Action<Exception> fail)
            {
                Run = run;
                Fail = fail;
            }
        }

        private readonly IEditorApplication _editorApplication;
        private readonly IPlayerLoop _playerLoop;
        private readonly Queue<Request> _queue = new();

        public PlayerLoopDispatcher(IEditorApplication editorApplication, IPlayerLoop playerLoop)
        {
            _editorApplication = editorApplication;
            _playerLoop = playerLoop;
        }

        public Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            Enqueue(tcs, () => tcs.TrySetResult(func()), cancellationToken);
            return tcs.Task;
        }

        public Task RunAsync(Action action, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Enqueue(tcs, () =>
            {
                action();
                tcs.TrySetResult(true);
            }, cancellationToken);
            return tcs.Task;
        }

        private void Enqueue<T>(TaskCompletionSource<T> tcs, Action run, CancellationToken cancellationToken)
        {
            if (!_editorApplication.IsPlaying)
            {
                throw new InvalidOperationException(
                    "This operation is only available in Play Mode. Enter Play Mode first.");
            }

            // The player loop does not run while the Editor is paused, so the request would never complete.
            if (_editorApplication.IsPaused)
            {
                throw new InvalidOperationException(
                    "This operation is not available while the Editor is paused. Unpause the Editor first.");
            }

            EnsureInstalled();

            // Cancel the task as soon as the token is canceled, without waiting for the next player loop update.
            // The token is canceled when the HTTP server stops (e.g. for a domain reload), and the request is then
            // answered with 503 right away. The queued function is skipped later because the task is already done.
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _queue.Enqueue(new Request(
                () =>
                {
                    // Already canceled (see above), so do not run the function.
                    if (tcs.Task.IsCompleted)
                    {
                        return;
                    }

                    try
                    {
                        run();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                ex => tcs.TrySetException(ex)));
        }

        // Called from the system inserted into the player loop.
        private void OnPlayerLoopUpdate()
        {
            // Run only the requests queued before this update, so that a request queued by a function run here waits
            // for the next frame.
            var count = _queue.Count;
            for (var i = 0; i < count; i++)
            {
                _queue.Dequeue().Run();
            }
        }

        internal void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            // The player loop stops with Play Mode, so fail the pending requests instead of leaving them waiting.
            while (_queue.Count > 0)
            {
                _queue.Dequeue().Fail(
                    new InvalidOperationException("Play Mode was exited before the operation completed."));
            }
        }

        // Runs at the end of PostLateUpdate, after Canvas layout updates and rendering, so that UI positions and
        // raycasts match what the Game View shows for the frame. Before rendering, UI changed in the frame is not
        // laid out yet, and Graphics shown for the first time have no depth, so GraphicRaycaster does not hit them.
        // The player loop may be replaced (e.g. on entering Play Mode or by user code), so check on every request.
        private void EnsureInstalled()
        {
            Action update = OnPlayerLoopUpdate;

            // A delegate equals another only when both its target instance and its method are the same,
            // so this is true only when the installed system calls this instance's OnPlayerLoopUpdate.
            if (_playerLoop.Contains<PostLateUpdate, PlayerLoopDispatcher>(update))
            {
                return;
            }

            // Insert also replaces a system that calls another instance, e.g. one left from before a domain
            // reload. Keeping it would drain that instance's queue instead of this one, so the requests queued
            // here would never run.
            _playerLoop.Insert<PostLateUpdate, PlayerLoopDispatcher>(update);
        }
    }
}
