using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
            bool isPaused = false, FakeTime time = null)
        {
            var editorApplication = new SpyEditorApplication { IsPlaying = isPlaying, IsPaused = isPaused };
            return new PlayerLoopDispatcher(editorApplication, playerLoop, time ?? new FakeTime());
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

        [Test]
        public void RunEachFrameAsync_CallsStepOncePerUpdate_UntilItReturnsFalse()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var calledFrames = new List<int>();

            // Act
            var task = dispatcher.RunEachFrameAsync((frame, _) =>
            {
                calledFrames.Add(frame);
                return frame < 2;
            });
            var calledBeforeUpdate = calledFrames.Count;
            playerLoop.Update();
            playerLoop.Update();
            var completedBeforeLastUpdate = task.IsCompleted;
            playerLoop.Update();
            playerLoop.Update();

            // Assert
            Assert.AreEqual(0, calledBeforeUpdate);
            Assert.IsFalse(completedBeforeLastUpdate);
            Assert.IsTrue(task.IsCompleted);
            task.GetAwaiter().GetResult();
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, calledFrames);
        }

        [Test]
        public void RunEachFrameAsync_PassesSecondsSinceFirstCall()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var time = new FakeTime { UnscaledTime = 10d };
            var dispatcher = CreateDispatcher(playerLoop, time: time);
            var elapsedTimes = new List<double>();
            dispatcher.RunEachFrameAsync((_, elapsed) =>
            {
                elapsedTimes.Add(elapsed);
                return true;
            });

            // Act
            playerLoop.Update();
            time.UnscaledTime = 10.25d;
            playerLoop.Update();
            time.UnscaledTime = 10.5d;
            playerLoop.Update();

            // Assert
            CollectionAssert.AreEqual(new[] { 0d, 0.25d, 0.5d }, elapsedTimes);
        }

        [Test]
        public void RunEachFrameAsync_PassesException_ToTask_AndStops()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var callCount = 0;
            var task = dispatcher.RunEachFrameAsync((frame, _) =>
            {
                callCount++;
                if (frame == 1)
                {
                    throw new ArgumentException("failed");
                }

                return true;
            });

            // Act
            playerLoop.Update();
            playerLoop.Update();
            playerLoop.Update();

            // Assert
            var ex = Assert.Throws<ArgumentException>(() => task.GetAwaiter().GetResult());
            Assert.AreEqual("failed", ex.Message);
            Assert.AreEqual(2, callCount);
        }

        [Test]
        public void RunEachFrameAsync_StopsCallingStep_WhenCanceled()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var callCount = 0;
            using var cts = new CancellationTokenSource();
            var task = dispatcher.RunEachFrameAsync((_, _) =>
            {
                callCount++;
                return true;
            }, cts.Token);
            playerLoop.Update();

            // Act
            cts.Cancel();
            playerLoop.Update();

            // Assert
            Assert.IsTrue(task.IsCanceled);
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void PlayerLoopUpdate_RunsRequestQueuedDuringUpdate_InNextFrame()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            Task<int> inner = null;
            dispatcher.RunAsync(() =>
            {
                inner = dispatcher.RunAsync(() => 2);
                return 1;
            });

            // Act
            playerLoop.Update();
            var completedInSameFrame = inner.IsCompleted;
            playerLoop.Update();

            // Assert
            Assert.IsFalse(completedInSameFrame);
            Assert.AreEqual(2, inner.GetAwaiter().GetResult());
        }

        [Test]
        public void OnPlayModeStateChanged_FailsRunningRequest_WhenExitingPlayMode()
        {
            // Arrange
            var playerLoop = new SpyPlayerLoop();
            var dispatcher = CreateDispatcher(playerLoop);
            var callCount = 0;
            var task = dispatcher.RunEachFrameAsync((_, _) =>
            {
                callCount++;
                return true;
            });
            playerLoop.Update();

            // Act
            dispatcher.OnPlayModeStateChanged(PlayModeStateChange.ExitingPlayMode);
            playerLoop.Update();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
            StringAssert.Contains("Play Mode was exited", ex.Message);
            Assert.AreEqual(1, callCount);
        }
    }
}
