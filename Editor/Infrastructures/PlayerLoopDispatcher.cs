using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;
using UnityEditor;
using UnityEngine.LowLevel;
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
        private readonly Queue<Request> _queue = new();

        public PlayerLoopDispatcher(IEditorApplication editorApplication)
        {
            _editorApplication = editorApplication;
        }

        public Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
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

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

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
                        tcs.TrySetResult(func());
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                ex => tcs.TrySetException(ex)));
            return tcs.Task;
        }

        // Called from the system inserted into the player loop.
        internal void OnPlayerLoopUpdate()
        {
            while (_queue.Count > 0)
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
                    new InvalidOperationException("Play Mode was exited before the operation ran."));
            }
        }

        // The player loop may be replaced (e.g. on entering Play Mode or by user code), and may still hold
        // the system of another instance (e.g. from before a domain reload), so check on every request.
        private void EnsureInstalled()
        {
            var system = new PlayerLoopSystem
            {
                type = typeof(PlayerLoopDispatcher),
                updateDelegate = OnPlayerLoopUpdate
            };

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (var i = 0; i < loop.subSystemList.Length; i++)
            {
                var phase = loop.subSystemList[i];
                if (phase.type != typeof(PreLateUpdate))
                {
                    continue;
                }

                var systems = (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>()).ToList();
                var index = systems.FindIndex(s => s.type == typeof(PlayerLoopDispatcher));

                // A delegate equals another only when both its target instance and its method are the same,
                // so this is true only when the installed system calls this instance's OnPlayerLoopUpdate.
                if (index >= 0 && Equals(systems[index].updateDelegate, system.updateDelegate))
                {
                    return;
                }

                if (index >= 0)
                {
                    // The installed system calls another instance, e.g. one created by a test or one left from
                    // before a domain reload. Keeping it would drain that instance's queue instead of this one,
                    // so the requests queued here would never run. Replace it with this instance's system.
                    systems[index] = system;
                }
                else
                {
                    systems.Add(system);
                }

                phase.subSystemList = systems.ToArray();
                loop.subSystemList[i] = phase;
                PlayerLoop.SetPlayerLoop(loop);
                return;
            }

            throw new InvalidOperationException("PreLateUpdate phase was not found in the player loop.");
        }
    }
}
