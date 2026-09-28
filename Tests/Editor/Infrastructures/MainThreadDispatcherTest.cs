using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UniCortex.Editor.Domains.Exceptions;
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

        [Test]
        public void RunOnMainThreadAsync_NotProcessedWithinThreshold_FailsWithoutExecutingAction()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(10));
            var executed = false;

            // Act
            var task = dispatcher.RunOnMainThreadAsync(() => executed = true);
            Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5))).Wait();
            dispatcher.OnUpdate();

            // Assert
            Assert.IsTrue(task.IsFaulted, $"status={task.Status}");
            Assert.IsInstanceOf<MainThreadUnresponsiveException>(task.Exception.InnerException);
            Assert.IsFalse(executed);
        }

        [Test]
        public void RunOnMainThreadAsync_MainThreadAlreadyUnresponsive_FailsImmediately()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(50), TimeSpan.FromSeconds(10));
            Thread.Sleep(100);

            // Act
            var task = dispatcher.RunOnMainThreadAsync(() => 1);

            // Assert
            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOf<MainThreadUnresponsiveException>(task.Exception.InnerException);
        }

        [Test]
        public void OnUpdate_RecordsHeartbeat_SoLaterRequestsAreQueued()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(50), TimeSpan.FromSeconds(10));
            Thread.Sleep(100);
            dispatcher.OnUpdate();

            // Act
            var task = dispatcher.RunOnMainThreadAsync(() => 42);
            dispatcher.OnUpdate();

            // Assert
            Assert.AreEqual(42, task.GetAwaiter().GetResult());
        }

        [Test]
        public void RunOnMainThreadAsync_ActionRunsLongerThanThreshold_DoesNotFailRunningOrQueuedRequests()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(10));
            var longRunning = dispatcher.RunOnMainThreadAsync(() =>
            {
                Thread.Sleep(300);
                return 1;
            });
            var queued = dispatcher.RunOnMainThreadAsync(() => 2);

            // Act
            dispatcher.OnUpdate();

            // Assert
            Assert.AreEqual(1, longRunning.GetAwaiter().GetResult());
            Assert.AreEqual(2, queued.GetAwaiter().GetResult());
        }

        [Test]
        public void RunOnMainThreadAsync_RequestedWhileActionRunsLongerThanThreshold_IsQueued()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(10));
            Task<int> requestedDuringAction = null;
            dispatcher.RunOnMainThreadAsync(() =>
            {
                Thread.Sleep(100);
                requestedDuringAction = dispatcher.RunOnMainThreadAsync(() => 3);
            });

            // Act
            dispatcher.OnUpdate();

            // Assert
            Assert.AreEqual(3, requestedDuringAction.GetAwaiter().GetResult());
        }
    }
}