using System.Collections.Generic;
using NUnit.Framework;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class TestResultStoreTest
    {
        // A dedicated prefix so that the test does not touch the state of the run executing it.
        private readonly TestResultStore _store = new("UniCortex.Tests.TestResultStoreTest.");

        [TearDown]
        public void TearDown()
        {
            _store.Clear();
        }

        [Test]
        public void SavePartialResults_ThenLoad_RoundTripsResults()
        {
            // Arrange
            var results = new List<TestResultItem>
            {
                new("TestA", "Passed", 0.5f),
                new("TestB", "Failed", 1.25f, "Expected 1 but was 2")
            };

            // Act
            _store.SavePartialResults(results);
            var loaded = _store.LoadPartialResults();

            // Assert
            Assert.AreEqual(2, loaded.Count);
            Assert.AreEqual("TestA", loaded[0].Name);
            Assert.AreEqual("Passed", loaded[0].Status);
            Assert.AreEqual(0.5f, loaded[0].Duration);
            Assert.AreEqual("TestB", loaded[1].Name);
            Assert.AreEqual("Failed", loaded[1].Status);
            Assert.AreEqual("Expected 1 but was 2", loaded[1].Message);
        }

        [Test]
        public void LoadPartialResults_WhenNothingSaved_ReturnsEmpty()
        {
            // Act
            var loaded = _store.LoadPartialResults();

            // Assert
            Assert.AreEqual(0, loaded.Count);
        }

        [Test]
        public void MarkPending_ClearsPartialResultsOfPreviousRun()
        {
            // Arrange
            _store.SavePartialResults(new List<TestResultItem> { new("Stale", "Passed", 0f) });

            // Act
            _store.MarkPending();

            // Assert
            Assert.IsTrue(_store.IsPending);
            Assert.AreEqual(0, _store.LoadPartialResults().Count);
        }

        [Test]
        public void StoreResult_ClearsPendingAndPartialResults()
        {
            // Arrange
            _store.MarkPending();
            _store.SavePartialResults(new List<TestResultItem> { new("TestA", "Passed", 0f) });

            // Act
            _store.StoreResult("{}");

            // Assert
            Assert.IsFalse(_store.IsPending);
            Assert.AreEqual("{}", _store.GetResult());
            Assert.AreEqual(0, _store.LoadPartialResults().Count);
        }
    }
}
