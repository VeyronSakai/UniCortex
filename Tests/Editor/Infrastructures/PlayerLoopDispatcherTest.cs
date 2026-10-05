using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEditor;
using UnityEngine.PlayerLoop;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class PlayerLoopDispatcherTest
    {
        private static PlayerLoopDispatcher CreateDispatcher(SpyPlayerLoop playerLoop, bool isPlaying = true,
            bool isPaused = false)
        {
            var editorApplication = new SpyEditorApplication { IsPlaying = isPlaying, IsPaused = isPaused };
            return new PlayerLoopDispatcher(editorApplication, playerLoop);
        }

        [Test]
        public void RunAsync_Throws_WhenNotPlaying()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop, isPlaying: false);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => dispatcher.RunAsync(() => 1));
            StringAssert.Contains("Play Mode", ex.Message);
            Assert.AreEqual(0, playerLoop.InsertCallCount);
        }

        [Test]
        public void RunAsync_Throws_WhenPaused()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop, isPaused: true);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => dispatcher.RunAsync(() => 1));
            StringAssert.Contains("paused", ex.Message);
            Assert.AreEqual(0, playerLoop.InsertCallCount);
        }

        [Test]
        public void RunAsync_InsertsSystem_OnlyOnce()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);

            // Act
            dispatcher.RunAsync(() => 1);
            dispatcher.RunAsync(() => 2);

            // Assert
            Assert.AreEqual(1, playerLoop.InsertCallCount);
            Assert.AreEqual(1, playerLoop.Systems.Count);
            Assert.AreEqual(typeof(PostLateUpdate), playerLoop.Systems[0].phase);
            Assert.AreEqual(typeof(PlayerLoopDispatcher), playerLoop.Systems[0].type);
        }

        [Test]
        public void RunAsync_ReplacesSystem_OfAnotherInstance()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var previous = CreateDispatcher(playerLoop);
            previous.RunAsync(() => "previous");
            var current = CreateDispatcher(playerLoop);

            // Act
            var task = current.RunAsync(() => "current");
            playerLoop.Update();

            // Assert
            Assert.AreEqual(2, playerLoop.InsertCallCount);
            Assert.AreEqual(1, playerLoop.Systems.Count);
            Assert.AreEqual("current", task.GetAwaiter().GetResult());
        }

        [Test]
        public void RunAsync_DoesNotRunFunction_UntilPlayerLoopUpdates()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var executed = false;

            // Act
            var task = dispatcher.RunAsync(() =>
            {
                executed = true;
                return 1;
            });

            // Assert
            Assert.IsFalse(executed);
            Assert.IsFalse(task.IsCompleted);
        }

        [Test]
        public void PlayerLoopUpdate_RunsQueuedFunctionsInEnqueueOrder()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
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
            playerLoop.Update();

            // Assert
            Assert.AreEqual("first", first.GetAwaiter().GetResult());
            Assert.AreEqual("second", second.GetAwaiter().GetResult());
            CollectionAssert.AreEqual(new[] { 1, 2 }, order);
        }

        [Test]
        public void PlayerLoopUpdate_PassesException_ToTask()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var task = dispatcher.RunAsync<int>(() => throw new ArgumentException("failed"));

            // Act
            playerLoop.Update();

            // Assert
            var ex = Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
            Assert.AreEqual("failed", ex.Message);
        }

        [Test]
        public void PlayerLoopUpdate_DoesNotRunFunction_WhenCanceledBefore()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var executed = false;
            using var cts = new CancellationTokenSource();
            var task = dispatcher.RunAsync(() =>
            {
                executed = true;
                return 1;
            }, cts.Token);
            cts.Cancel();

            // Act
            playerLoop.Update();

            // Assert
            Assert.IsTrue(task.IsCanceled);
            Assert.IsFalse(executed);
        }

        [Test]
        public void OnPlayModeStateChanged_FailsPendingRequests_WhenExitingPlayMode()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var executed = false;
            var task = dispatcher.RunAsync(() =>
            {
                executed = true;
                return 1;
            });

            // Act
            dispatcher.OnPlayModeStateChanged(PlayModeStateChange.ExitingPlayMode);
            playerLoop.Update();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
            StringAssert.Contains("Play Mode was exited", ex.Message);
            Assert.IsFalse(executed);
        }
    }
}
