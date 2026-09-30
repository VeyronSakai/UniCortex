using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SessionStoreTestCallbacksTest
    {
        private FakeTestResultStore _store;

        [SetUp]
        public void SetUp()
        {
            _store = new FakeTestResultStore();
        }

        [Test]
        public void Constructor_RestoresResultsSavedBeforeDomainReload()
        {
            // Arrange
            _store.MarkPending();
            _store.SavePendingResults(new List<TestResultItem> { new("BeforeReload", "Passed", 0.1f) });

            // Act
            var callbacks = new SessionStoreTestCallbacks(_store);
            callbacks.RunFinished(null);

            // Assert
            var response = JsonUtility.FromJson<RunTestsResponse>(_store.GetResult());
            Assert.AreEqual(1, response.passed);
            Assert.AreEqual("BeforeReload", response.results[0].name);
            Assert.IsFalse(_store.IsPending);
        }

        [Test]
        public void RunStarted_StartsFromPendingResultsOfTheRun()
        {
            // Arrange
            var callbacks = new SessionStoreTestCallbacks(_store);
            _store.MarkPending();
            _store.SavePendingResults(new List<TestResultItem> { new("Stored", "Passed", 0.1f) });

            // Act
            callbacks.RunStarted(null);

            // Assert
            CollectionAssert.AreEqual(new[] { "Stored" }, callbacks.Results.Select(r => r.Name).ToArray());
        }

        [Test]
        public void RunFinished_CountsResultsByStatus()
        {
            // Arrange
            _store.MarkPending();
            _store.SavePendingResults(new List<TestResultItem>
            {
                new("Test1", TestStatuses.Passed, 0.1f),
                new("Test2", TestStatuses.Failed, 0.2f, "assertion error"),
                new("Test3", TestStatuses.Passed, 0.05f),
                new("Test4", "Skipped", 0f)
            });
            var callbacks = new SessionStoreTestCallbacks(_store);

            // Act
            callbacks.RunFinished(null);

            // Assert
            var response = JsonUtility.FromJson<RunTestsResponse>(_store.GetResult());
            Assert.AreEqual(2, response.passed);
            Assert.AreEqual(1, response.failed);
            Assert.AreEqual(1, response.skipped);
            Assert.AreEqual(4, response.results.Count);
            Assert.AreEqual(0, callbacks.Results.Count);
        }

        [Test]
        public void RunFinished_WhenNoRunIsPending_KeepsStoredResult()
        {
            // Arrange
            _store.StoreResult("{\"passed\":3}");
            var callbacks = new SessionStoreTestCallbacks(_store);

            // Act
            callbacks.RunStarted(null);
            callbacks.RunFinished(null);

            // Assert
            Assert.AreEqual("{\"passed\":3}", _store.GetResult());
            Assert.IsFalse(_store.IsPending);
        }

        [Test]
        public void SavePendingResults_WhenNoRunIsPending_DoesNotStartRun()
        {
            // Arrange
            var callbacks = new SessionStoreTestCallbacks(_store);

            // Act
            callbacks.SavePendingResults();

            // Assert
            Assert.IsFalse(_store.IsPending);
        }

        [Test]
        public void SavePendingResults_PersistsCurrentResultsForTheNextCallbacks()
        {
            // Arrange
            _store.MarkPending();
            _store.SavePendingResults(new List<TestResultItem> { new("First", "Failed", 0.2f, "boom") });
            var beforeReload = new SessionStoreTestCallbacks(_store);

            // Act
            beforeReload.SavePendingResults();
            var afterReload = new SessionStoreTestCallbacks(_store);

            // Assert
            var restored = afterReload.Results.Single();
            Assert.AreEqual("First", restored.Name);
            Assert.AreEqual("Failed", restored.Status);
            Assert.AreEqual("boom", restored.Message);
        }
    }
}
