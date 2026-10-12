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
        public void RunOnMainThreadAsync_AfterUpdateStopped_FailsWithoutRunningAction()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(50));
            var executed = false;
            Thread.Sleep(100);

            // Act
            var task = dispatcher.RunOnMainThreadAsync(() => executed = true);
            dispatcher.OnUpdate();

            // Assert
            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOf<MainThreadNotRespondingException>(task.Exception.InnerException);
            Assert.IsFalse(executed);
        }

        [Test]
        public void RunOnMainThreadAsync_NotStartedWithinThreshold_FailsAndNeverRunsAction()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(200));
            var executed = false;

            // Act
            var task = dispatcher.RunOnMainThreadAsync(() => executed = true);
            var completed = ((Task)task).ContinueWith(_ => { }).Wait(TimeSpan.FromSeconds(5));
            dispatcher.OnUpdate();

            // Assert
            Assert.IsTrue(completed);
            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOf<MainThreadNotRespondingException>(task.Exception.InnerException);
            Assert.IsFalse(executed);
        }

        [Test]
        public void RunOnMainThreadAsync_StartedWithinThreshold_ReturnsResult()
        {
            // Arrange
            var dispatcher = new MainThreadDispatcher(TimeSpan.FromMilliseconds(200));

            // Act
            var task = dispatcher.RunOnMainThreadAsync(() => 42);
            dispatcher.OnUpdate();
            Thread.Sleep(400);

            // Assert
            Assert.IsTrue(task.IsCompletedSuccessfully);
            Assert.AreEqual(42, task.Result);
        }
    }
}
