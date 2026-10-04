using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace UniCortex.Editor.Infrastructures
{
    // Runs functions inside the player loop in Play Mode.
    // Some APIs (e.g. Screen.width/height, which GraphicRaycaster uses) return Game View values only while
    // the player loop runs; from EditorApplication.update they return the size of another view.
    internal static class PlayerLoopRunner
    {
        // Marker type that identifies the system inserted into the player loop.
        private struct UniCortexPlayerLoopRunner
        {
        }

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

        private static readonly Queue<Request> s_queue = new();

        static PlayerLoopRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // Must be called on the main thread.
        public static Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException(
                    "This operation is only available in Play Mode. Enter Play Mode first.");
            }

            // The player loop does not run while the Editor is paused, so the request would never complete.
            if (EditorApplication.isPaused)
            {
                throw new InvalidOperationException(
                    "This operation is not available while the Editor is paused. Unpause the Editor first.");
            }

            EnsureInstalled();

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            s_queue.Enqueue(new Request(
                () =>
                {
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

        private static void Run()
        {
            while (s_queue.Count > 0)
            {
                s_queue.Dequeue().Run();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            // The player loop stops with Play Mode, so fail the pending requests instead of leaving them waiting.
            while (s_queue.Count > 0)
            {
                s_queue.Dequeue().Fail(
                    new InvalidOperationException("Play Mode was exited before the operation ran."));
            }
        }

        // The player loop may be replaced (e.g. on entering Play Mode or by user code), so check on every request.
        private static void EnsureInstalled()
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (Contains(loop))
            {
                return;
            }

            for (var i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type != typeof(PreLateUpdate))
                {
                    continue;
                }

                var phase = loop.subSystemList[i];
                var systems = (phase.subSystemList ?? Array.Empty<PlayerLoopSystem>()).ToList();
                systems.Add(new PlayerLoopSystem
                {
                    type = typeof(UniCortexPlayerLoopRunner),
                    updateDelegate = Run
                });
                phase.subSystemList = systems.ToArray();
                loop.subSystemList[i] = phase;
                PlayerLoop.SetPlayerLoop(loop);
                return;
            }

            throw new InvalidOperationException("PreLateUpdate phase was not found in the player loop.");
        }

        private static bool Contains(PlayerLoopSystem system)
        {
            return system.type == typeof(UniCortexPlayerLoopRunner)
                   || (system.subSystemList != null && system.subSystemList.Any(Contains));
        }
    }
}
