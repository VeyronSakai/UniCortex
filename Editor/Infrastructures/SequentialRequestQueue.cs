using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UniCortex.Editor.Infrastructures
{
    internal sealed class SequentialRequestQueue<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> _queue = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);
            _signal.Release();
        }

        public async Task RunAsync(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            try
            {
                while (true)
                {
                    await _signal.WaitAsync(cancellationToken);
                    if (_queue.TryDequeue(out var item))
                    {
                        await handler(item, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }

            while (_queue.TryDequeue(out var item))
            {
                await handler(item, cancellationToken);
            }
        }

        public void Dispose()
        {
            _signal.Dispose();
        }
    }
}
