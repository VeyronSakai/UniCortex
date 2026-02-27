using System.Collections.Generic;
using System.Threading;
using UniCortex.Editor.Domains.Interfaces;
using UniCortex.Editor.Domains.Models;
using UniCortex.Editor.Infrastructures;
using UniCortex.Editor.Tests.TestDoubles;
using UniCortex.Editor.UseCases;
using NUnit.Framework;
using UniCortex.Editor.Handlers.Tests;

namespace UniCortex.Editor.Tests.Presentations
{
    [TestFixture]
    internal sealed class RunTestsHandlerTest
    {
        [Test]
        public void HandleRunTests_Returns200WithTestResults()
        {
            var spy = new SpyTestRunner(new List<TestResultItem>
            {
                new("Test1", "Passed", 0.1f), new("Test2", "Failed", 0.2f, "error"),
            });
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun,
                "{\"testMode\":\"EditMode\",\"nameFilter\":\"\"}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            StringAssert.Contains("\"passed\":1", context.ResponseBody);
            StringAssert.Contains("\"failed\":1", context.ResponseBody);
            Assert.AreEqual("EditMode", spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_DefaultsToEditMode_WhenTestModeNotSpecified()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun, "{}");

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual("EditMode", spy.LastTestMode);
        }

        [Test]
        public void HandleRunTests_DefaultsToEditMode_WhenBodyIsEmpty()
        {
            var spy = new SpyTestRunner();
            var useCase = new RunTestsUseCase(spy);
            var handler = new RunTestsHandler(useCase);

            var router = new RequestRouter();
            handler.Register(router);

            var context = new FakeRequestContext("POST", ApiRoutes.TestsRun);

            router.HandleRequestAsync(context, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(200, context.ResponseStatusCode);
            Assert.AreEqual("EditMode", spy.LastTestMode);
        }
    }
}
