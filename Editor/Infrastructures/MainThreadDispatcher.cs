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
        // EditorApplication.update stops while a modal dialog is shown, so requests would otherwise
        // wait until the client's HTTP timeout. Fail fast once the main thread stops making progress.
        private static readonly TimeSpan s_defaultUnresponsiveThreshold = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan s_defaultWatchdogInterval = TimeSpan.FromMilliseconds(500);

        private readonly ConcurrentQueue<Action> _queue = new();
        private readonly TimeSpan _unresponsiveThreshold;
        private readonly TimeSpan _watchdogInterval;
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly object _gate = new();
        private long _lastHeartbeatTicks;
        private bool _isExecuting;

        public MainThreadDispatcher() : this(s_defaultUnresponsiveThreshold, s_defaultWatchdogInterval)
        {
        }

        internal MainThreadDispatcher(TimeSpan unresponsiveThreshold, TimeSpan watchdogInterval)
        {
            _unresponsiveThreshold = unresponsiveThreshold;
            _watchdogInterval = watchdogInterval;
            RecordHeartbeat();
        }

        internal void OnUpdate()
        {
            RecordHeartbeat();
            while (_queue.TryDequeue(out var action))
            {
                // A dispatched action that takes long (e.g. a large asset import inside AssetDatabase.Refresh)
                // is legitimate work, so the main thread is not treated as unresponsive while it runs.
                lock (_gate)
                {
                    _isExecuting = true;
                }

                try
                {
                    action();
                }
                finally
                {
                    lock (_gate)
                    {
                        _isExecuting = false;
                        RecordHeartbeat();
                    }
                }
            }
        }

        public Task<T> RunOnMainThreadAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<T>(cancellationToken);
            }

            lock (_gate)
            {
                if (TryCreateUnresponsiveException(out var unresponsiveException))
                {
                    return Task.FromException<T>(unresponsiveException);
                }
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            _queue.Enqueue(() =>
            {
                // Skip requests that were already cancelled or failed by the watchdog.
                if (tcs.Task.IsCompleted)
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

            _ = WatchAsync(tcs);
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

        private async Task WatchAsync<T>(TaskCompletionSource<T> tcs)
        {
            while (!tcs.Task.IsCompleted)
            {
                // ConfigureAwait(false) keeps the watchdog off the main thread, which may be blocked.
                await Task.WhenAny(tcs.Task, Task.Delay(_watchdogInterval)).ConfigureAwait(false);

                // Checked under the gate so that a request is never failed after its action has started.
                lock (_gate)
                {
                    if (!tcs.Task.IsCompleted && TryCreateUnresponsiveException(out var exception))
                    {
                        tcs.TrySetException(exception);
                        return;
                    }
                }
            }
        }

        private void RecordHeartbeat()
        {
            Interlocked.Exchange(ref _lastHeartbeatTicks, _clock.Elapsed.Ticks);
        }

        // Must be called while holding _gate.
        private bool TryCreateUnresponsiveException(out MainThreadUnresponsiveException exception)
        {
            exception = null;
            if (_isExecuting)
            {
                return false;
            }

            var elapsed = TimeSpan.FromTicks(_clock.Elapsed.Ticks - Interlocked.Read(ref _lastHeartbeatTicks));
            if (elapsed < _unresponsiveThreshold)
            {
                return false;
            }

            exception = new MainThreadUnresponsiveException(elapsed);
            return true;
        }
    }
}
