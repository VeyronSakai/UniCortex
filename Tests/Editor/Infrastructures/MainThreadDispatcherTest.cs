using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Infrastructures;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class MainThreadDispatcherTest
    {
        [Test]
        public void OnUpdate_ExecutesQueuedActionsInEnqueueOrder()
        {
            var dispatcher = new MainThreadDispatcher();
            var order = new List<int>();

            var first = dispatcher.RunOnMainThreadAsync(() => order.Add(1));
            var second = dispatcher.RunOnMainThreadAsync(() => order.Add(2));
            var third = dispatcher.RunOnMainThreadAsync(() => order.Add(3));

            Assert.IsFalse(first.IsCompleted);
            Assert.IsFalse(second.IsCompleted);
            Assert.IsFalse(third.IsCompleted);

            dispatcher.OnUpdate();

            first.GetAwaiter().GetResult();
            second.GetAwaiter().GetResult();
            third.GetAwaiter().GetResult();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order);
        }

        [Test]
        public void RunOnMainThreadAsync_CanceledBeforeUpdate_DoesNotExecuteAction()
        {
            var dispatcher = new MainThreadDispatcher();
            var executed = false;
            using var cts = new CancellationTokenSource();

            var task = dispatcher.RunOnMainThreadAsync(() => executed = true, cts.Token);

            cts.Cancel();
            dispatcher.OnUpdate();

            Assert.IsFalse(executed);
            Assert.Throws<TaskCanceledException>(() => task.GetAwaiter().GetResult());
        }
    }
}