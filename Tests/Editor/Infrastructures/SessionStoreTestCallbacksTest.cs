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
            _store.MarkPending();
        }

        [Test]
        public void Constructor_RestoresResultsSavedBeforeDomainReload()
        {
            // Arrange
            _store.SavePendingResults(new List<TestResultItem> { new("BeforeReload", "Passed", 0.1f) });

            // Act
            var callbacks = new SessionStoreTestCallbacks(_store);
            callbacks.RunFinished(null);

            // Assert
            CollectionAssert.AreEqual(new[] { "BeforeReload" }, callbacks.Results.Select(r => r.Name).ToArray());
            var response = JsonUtility.FromJson<RunTestsResponse>(_store.GetResult());
            Assert.AreEqual(1, response.passed);
            Assert.AreEqual("BeforeReload", response.results[0].name);
            Assert.IsFalse(_store.IsPending);
        }

        [Test]
        public void SavePendingResults_PersistsCurrentResultsForTheNextCallbacks()
        {
            // Arrange
            _store.SavePendingResults(new List<TestResultItem> { new("First", "Failed", 0.2f, "boom") });
            var beforeReload = new SessionStoreTestCallbacks(_store);

            // Act
            beforeReload.SavePendingResults();
            var afterReload = new SessionStoreTestCallbacks(_store);
            afterReload.RunFinished(null);
            beforeReload.RunFinished(null);

            // Assert
            var restored = afterReload.Results.Single();
            Assert.AreEqual("First", restored.Name);
            Assert.AreEqual("Failed", restored.Status);
            Assert.AreEqual("boom", restored.Message);
        }
    }
}
