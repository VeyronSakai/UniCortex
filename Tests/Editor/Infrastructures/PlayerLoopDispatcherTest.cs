using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEditor;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class PlayerLoopDispatcherTest
    {
        [Test]
        public void RunAsync_Throws_WhenNotPlaying()
        {
            // Arrange
            var dispatcher = new PlayerLoopDispatcher(new SpyEditorApplication { IsPlaying = false });

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => dispatcher.RunAsync(() => 1));
            StringAssert.Contains("Play Mode", ex.Message);
        }

        [Test]
        public void RunAsync_Throws_WhenPaused()
        {
            // Arrange
            var dispatcher = new PlayerLoopDispatcher(
                new SpyEditorApplication { IsPlaying = true, IsPaused = true });

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => dispatcher.RunAsync(() => 1));
            StringAssert.Contains("paused", ex.Message);
        }

        [Test]
        public void OnPlayerLoopUpdate_RunsQueuedFunctionsInEnqueueOrder()
        {
            // Arrange
            var dispatcher = new PlayerLoopDispatcher(new SpyEditorApplication { IsPlaying = true });
            var order = new List<int>();
            var first = dispatcher.RunAsync(() =>
            {
                order.Add(1);
                return "first";
            });
            var second = dispatcher.RunAsync(() =>
            {
                order.Add(2);
                return "second";
            });

            // Act
            dispatcher.OnPlayerLoopUpdate();

            // Assert
            Assert.AreEqual("first", first.GetAwaiter().GetResult());
            Assert.AreEqual("second", second.GetAwaiter().GetResult());
            CollectionAssert.AreEqual(new[] { 1, 2 }, order);
        }

        [Test]
        public void OnPlayerLoopUpdate_PassesException_ToTask()
        {
            // Arrange
            var dispatcher = new PlayerLoopDispatcher(new SpyEditorApplication { IsPlaying = true });
            var task = dispatcher.RunAsync<int>(() => throw new ArgumentException("failed"));

            // Act
            dispatcher.OnPlayerLoopUpdate();

            // Assert
            var ex = Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
            Assert.AreEqual("failed", ex.Message);
        }

        [Test]
        public void OnPlayerLoopUpdate_DoesNotRunFunction_WhenCanceledBefore()
        {
            // Arrange
            var dispatcher = new PlayerLoopDispatcher(new SpyEditorApplication { IsPlaying = true });
            var executed = false;
            using var cts = new CancellationTokenSource();
            var task = dispatcher.RunAsync(() =>
            {
                executed = true;
                return 1;
            }, cts.Token);
            cts.Cancel();

            // Act
            dispatcher.OnPlayerLoopUpdate();

            // Assert
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(executed);
        }

        [Test]
        public void OnPlayModeStateChanged_FailsPendingRequests_WhenExitingPlayMode()
        {
            // Arrange
            var dispatcher = new PlayerLoopDispatcher(new SpyEditorApplication { IsPlaying = true });
            var executed = false;
            var task = dispatcher.RunAsync(() =>
            {
                executed = true;
                return 1;
            });

            // Act
            dispatcher.OnPlayModeStateChanged(PlayModeStateChange.ExitingPlayMode);
            dispatcher.OnPlayerLoopUpdate();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
            StringAssert.Contains("Play Mode was exited", ex.Message);
            Assert.IsFalse(executed);
        }
    }
}
