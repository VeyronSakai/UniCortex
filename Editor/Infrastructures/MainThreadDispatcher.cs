using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class MainThreadDispatcher : IMainThreadDispatcher
    {
        // EditorApplication.update does not run while a modal dialog is open, so a queued request would wait until
        // the client times out. A request that does not start within this time fails instead, without running.
        private static readonly TimeSpan s_defaultUnresponsiveThreshold = TimeSpan.FromSeconds(5);

        private readonly ConcurrentQueue<Action> _queue = new();
        private readonly TimeSpan _unresponsiveThreshold;

        // Stopwatch timestamp of the last OnUpdate. Written on the main thread and read on HTTP threads.
        private long _lastUpdateTimestamp = Stopwatch.GetTimestamp();

        public MainThreadDispatcher() : this(s_defaultUnresponsiveThreshold)
        {
        }

        internal MainThreadDispatcher(TimeSpan unresponsiveThreshold)
        {
            _unresponsiveThreshold = unresponsiveThreshold;
        }

        internal void OnUpdate()
        {
            Interlocked.Exchange(ref _lastUpdateTimestamp, Stopwatch.GetTimestamp());

            while (_queue.TryDequeue(out var action))
            {
                action();
            }

            // An action may take long. Record the end too, so that a request arriving right after it is not rejected.
            Interlocked.Exchange(ref _lastUpdateTimestamp, Stopwatch.GetTimestamp());
        }

        public Task<T> RunOnMainThreadAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<T>(cancellationToken);
            }

            var unresponsiveTime = GetUnresponsiveTime();
            if (unresponsiveTime >= _unresponsiveThreshold)
            {
                return Task.FromException<T>(new MainThreadNotRespondingException(unresponsiveTime));
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            var state = new QueuedActionState();
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _queue.Enqueue(() =>
            {
                if (!state.TryStart())
                {
                    return;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.TrySetCanceled(cancellationToken);
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
            });

            // The main thread may stop right after the check above, e.g. when a modal dialog opens.
            Task.Delay(_unresponsiveThreshold).ContinueWith(_ =>
            {
                if (state.TryAbandon())
                {
                    tcs.TrySetException(new MainThreadNotRespondingException(GetUnresponsiveTime()));
                }
            }, TaskScheduler.Default);

            return tcs.Task;
        }

        public Task RunOnMainThreadAsync(Action action, CancellationToken cancellationToken = default)
        {
            return RunOnMainThreadAsync(() =>
            {
                action();
                return true;
            }, cancellationToken);
        }

        private TimeSpan GetUnresponsiveTime()
        {
            var elapsedTicks = Stopwatch.GetTimestamp() - Interlocked.Read(ref _lastUpdateTimestamp);
            return TimeSpan.FromSeconds((double)elapsedTicks / Stopwatch.Frequency);
        }

        /// <summary>
        /// Decides once whether a queued action runs on the main thread or is abandoned because it did not start in
        /// time, so that an abandoned action never runs.
        /// </summary>
        private sealed class QueuedActionState
        {
            private const int Queued = 0;
            private const int Started = 1;
            private const int Abandoned = 2;

            private int _state = Queued;

            public bool TryStart()
            {
                return Interlocked.CompareExchange(ref _state, Started, Queued) == Queued;
            }

            public bool TryAbandon()
            {
                return Interlocked.CompareExchange(ref _state, Abandoned, Queued) == Queued;
            }
        }
    }
}
