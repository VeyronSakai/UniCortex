using System;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Interfaces;

namespace UniCortex.Editor.Tests.TestDoubles
{
    // Runs functions immediately and returns completed tasks (see FakeMainThreadDispatcher).
    // Each call is treated as one frame: Time advances by SecondsPerFrame after each call.
    internal sealed class FakePlayerLoopDispatcher : IPlayerLoopDispatcher
    {
        public FakeTime Time { get; } = new();

        // 0.25 is exact in binary, so tests can compare times without rounding errors.
        public double SecondsPerFrame { get; set; } = 0.25d;

        // Number of frames run so far (the number of calls of RunAsync).
        public int FrameCount { get; private set; }

        // Called with the frame number before each frame is run.
        public Action<int> OnFrame { get; set; }

        public Task<T> RunAsync<T>(Func<T> func, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<T>(cancellationToken);

            OnFrame?.Invoke(FrameCount);
            var result = func();
            EndFrame();
            return Task.FromResult(result);
        }

        public Task RunAsync(Action action, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            OnFrame?.Invoke(FrameCount);
            action();
            EndFrame();
            return Task.CompletedTask;
        }

        private void EndFrame()
        {
            FrameCount++;
            Time.UnscaledTime += SecondsPerFrame;
        }
    }
}
