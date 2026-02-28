using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;

namespace UniCortex.Editor.Tests.UseCases
{
    [TestFixture]
    internal sealed class RunTestsUseCaseTest
    {
        [Test]
        public void ExecuteAsync_AggregatesResults()
        {
            var spy = new SpyTestRunner(new List<TestResultItem>
            {
                new TestResultItem("Test1", "Passed", 0.1f),
                new TestResultItem("Test2", "Failed", 0.2f, "assertion error"),
                new TestResultItem("Test3", "Passed", 0.05f),
                new TestResultItem("Test4", "Skipped", 0f),
            });
            var useCase = new RunTestsUseCase(spy);

            var result = useCase.ExecuteAsync("EditMode", "", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(2, result.passed);
            Assert.AreEqual(1, result.failed);
            Assert.AreEqual(1, result.skipped);
            Assert.AreEqual(4, result.results.Count);
        }

        [Test]
        public void ExecuteAsync_PassesParametersToTestRunner()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);

            useCase.ExecuteAsync("PlayMode", "MyTest", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(1, spy.RunTestsCallCount);
            Assert.AreEqual("PlayMode", spy.LastTestMode);
            Assert.AreEqual("MyTest", spy.LastNameFilter);
        }

        [Test]
        public void ExecuteAsync_WithEmptyResults_ReturnsZeroCounts()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);

            var result = useCase.ExecuteAsync("EditMode", "", CancellationToken.None)
                .GetAwaiter().GetResult();

            Assert.AreEqual(0, result.passed);
            Assert.AreEqual(0, result.failed);
            Assert.AreEqual(0, result.skipped);
            Assert.AreEqual(0, result.results.Count);
        }
    }
}
