using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Infrastructures;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SequentialRequestQueueTest
    {
        [Test]
        public void RunAsync_ProcessesQueuedItemsSequentially()
        {
            using var queue = new SequentialRequestQueue<int>();
            using var cts = new CancellationTokenSource();
            using var firstStarted = new ManualResetEventSlim();
            using var releaseFirst = new ManualResetEventSlim();
            using var secondStarted = new ManualResetEventSlim();
            var executionOrder = new List<int>();
            var syncRoot = new object();
            var inFlight = 0;
            var maxInFlight = 0;

            var processingTask = Task.Run(() => queue.RunAsync((item, token) =>
            {
                var currentInFlight = Interlocked.Increment(ref inFlight);
                lock (syncRoot)
                {
                    executionOrder.Add(item);
                    if (currentInFlight > maxInFlight)
                    {
                        maxInFlight = currentInFlight;
                    }
                }

                if (item == 1)
                {
                    firstStarted.Set();
                    releaseFirst.Wait(1000);
                }
                else
                {
                    secondStarted.Set();
                }

                Interlocked.Decrement(ref inFlight);
                return Task.CompletedTask;
            }, cts.Token));

            try
            {
                queue.Enqueue(1);
                queue.Enqueue(2);

                Assert.That(firstStarted.Wait(1000), Is.True);
                Assert.That(secondStarted.Wait(100), Is.False);

                releaseFirst.Set();

                Assert.That(secondStarted.Wait(1000), Is.True);
                Assert.That(SpinWait.SpinUntil(() =>
                {
                    lock (syncRoot)
                    {
                        return executionOrder.Count == 2;
                    }
                }, 1000), Is.True);
            }
            finally
            {
                releaseFirst.Set();
                cts.Cancel();
                processingTask.GetAwaiter().GetResult();
            }

            CollectionAssert.AreEqual(new[] { 1, 2 }, executionOrder);
            Assert.That(maxInFlight, Is.EqualTo(1));
        }

        [Test]
        public void RunAsync_DrainsQueuedItemsAfterCancellation()
        {
            using var queue = new SequentialRequestQueue<int>();
            using var cts = new CancellationTokenSource();
            using var firstStarted = new ManualResetEventSlim();
            using var releaseFirst = new ManualResetEventSlim();
            using var secondHandled = new ManualResetEventSlim();
            var secondSawCancelled = 0;

            var processingTask = Task.Run(() => queue.RunAsync((item, token) =>
            {
                if (item == 1)
                {
                    firstStarted.Set();
                    releaseFirst.Wait(1000);
                }

                if (item == 2)
                {
                    if (token.IsCancellationRequested)
                    {
                        Interlocked.Exchange(ref secondSawCancelled, 1);
                    }

                    secondHandled.Set();
                }

                return Task.CompletedTask;
            }, cts.Token));

            try
            {
                queue.Enqueue(1);
                queue.Enqueue(2);

                Assert.That(firstStarted.Wait(1000), Is.True);

                cts.Cancel();
                releaseFirst.Set();

                processingTask.GetAwaiter().GetResult();
            }
            finally
            {
                releaseFirst.Set();
            }

            Assert.That(secondHandled.IsSet, Is.True);
            Assert.That(secondSawCancelled, Is.EqualTo(1));
        }
    }
}
