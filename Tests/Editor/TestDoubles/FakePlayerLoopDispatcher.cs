using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    // Runs functions immediately and returns completed tasks (see FakeMainThreadDispatcher).
    internal sealed class FakePlayerLoopDispatcher : IPlayerLoopDispatcher
    {
        public int RunEachFrameCallCount { get; private set; }

        // Called with the frame number before each step of RunEachFrameAsync.
        public Action<int> OnFrame { get; set; }

        public Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<T>(cancellationToken);
            return Task.FromResult(func());
        }

        public Task RunEachFrameAsync(Func<int, bool> step, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            RunEachFrameCallCount++;
            var frame = 0;
            bool keepRunning;
            do
            {
                OnFrame?.Invoke(frame);
                keepRunning = step(frame);
                frame++;
            } while (keepRunning);

            return Task.CompletedTask;
        }
    }
}
