using System.Collections.Generic;
using NUnit.Framework;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Infrastructures;

namespace UniCortex.Editor.Tests.Infrastructures
{
    [TestFixture]
    internal sealed class TestResultStoreTest
    {
        // Dedicated keys so that the test does not touch the state of the run executing it.
        private readonly TestResultStore _store = new(
            "UniCortex.Tests.TestResultStoreTest.TestResultJson",
            "UniCortex.Tests.TestResultStoreTest.TestPendingResultsJson");

        [TearDown]
        public void TearDown()
        {
            _store.Clear();
        }

        [Test]
        public void IsPending_BeforeAnyRun_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(_store.IsPending);
            Assert.AreEqual(string.Empty, _store.GetResult());
        }

        [Test]
        public void MarkPending_StartsRunWithoutPendingResults()
        {
            // Arrange
            _store.StoreResult("{\"previous\":true}");

            // Act
            _store.MarkPending();

            // Assert
            Assert.IsTrue(_store.IsPending);
            Assert.AreEqual(string.Empty, _store.GetResult());
            Assert.AreEqual(0, _store.LoadPendingResults().Count);
        }

        [Test]
        public void SavePendingResults_ThenLoad_RoundTripsResults()
        {
            // Arrange
            _store.MarkPending();
            var results = new List<TestResultItem>
            {
                new("TestA", "Passed", 0.5f),
                new("TestB", "Failed", 1.25f, "Expected 1 but was 2")
            };

            // Act
            _store.SavePendingResults(results);
            var loaded = _store.LoadPendingResults();

            // Assert
            Assert.IsTrue(_store.IsPending);
            Assert.AreEqual(2, loaded.Count);
            Assert.AreEqual("TestA", loaded[0].Name);
            Assert.AreEqual("Passed", loaded[0].Status);
            Assert.AreEqual(0.5f, loaded[0].Duration);
            Assert.AreEqual("TestB", loaded[1].Name);
            Assert.AreEqual("Failed", loaded[1].Status);
            Assert.AreEqual("Expected 1 but was 2", loaded[1].Message);
        }

        [Test]
        public void StoreResult_FinishesRunAndClearsPendingResults()
        {
            // Arrange
            _store.MarkPending();
            _store.SavePendingResults(new List<TestResultItem> { new("TestA", "Passed", 0f) });

            // Act
            _store.StoreResult("{}");

            // Assert
            Assert.IsFalse(_store.IsPending);
            Assert.AreEqual("{}", _store.GetResult());
            Assert.AreEqual(0, _store.LoadPendingResults().Count);
        }
    }
}
