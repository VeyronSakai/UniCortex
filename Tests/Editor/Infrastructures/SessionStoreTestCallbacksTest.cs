using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UnityEngine;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class SessionStoreTestCallbacksTest
    {
        // A dedicated prefix so that the test does not touch the state of the run executing it.
        private readonly TestResultStore _store = new("UniCortex.Tests.SessionStoreTestCallbacksTest.");

        [SetUp]
        public void SetUp()
        {
            _store.MarkPending();
        }

        [TearDown]
        public void TearDown()
        {
            _store.Clear();
        }

        [Test]
        public void Constructor_RestoresResultsSavedBeforeDomainReload()
        {
            // Arrange
            _store.SavePartialResults(new List<TestResultItem> { new("BeforeReload", "Passed", 0.1f) });

            // Act
            var callbacks = new SessionStoreTestCallbacks(null, _store);
            callbacks.RunFinished(null);

            // Assert
            CollectionAssert.AreEqual(new[] { "BeforeReload" }, callbacks.Results.Select(r => r.Name).ToArray());
            var response = JsonUtility.FromJson<RunTestsResponse>(_store.GetResult());
            Assert.AreEqual(1, response.passed);
            Assert.AreEqual("BeforeReload", response.results[0].name);
            Assert.IsFalse(_store.IsPending);
        }

        [Test]
        public void SavePartialResults_PersistsCurrentResultsForTheNextCallbacks()
        {
            // Arrange
            _store.SavePartialResults(new List<TestResultItem> { new("First", "Failed", 0.2f, "boom") });
            var beforeReload = new SessionStoreTestCallbacks(null, _store);

            // Act
            beforeReload.SavePartialResults();
            var afterReload = new SessionStoreTestCallbacks(null, _store);
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
